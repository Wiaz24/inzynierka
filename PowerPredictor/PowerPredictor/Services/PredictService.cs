using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using PowerPredictor.Models;
using PowerPredictor.Services.Interfaces;

namespace PowerPredictor.Services
{
    public class PredictServiceConfiguration
    {
        public string ModelPath { get; set; } = null!;
        public string InputName { get; set; } = null!;
        public string OutputName { get; set; } = null!;
        public int InputLength { get; set; }
        public int OutputLength { get; set; }
        public int[] InputShape { get; set; } = null!;
        public float MinScalerValue { get; set; }
        public float MaxScalerValue { get; set; }
    }


    public class PredictService : IPredictService
    {
        private readonly PredictServiceConfiguration config;
        private readonly InferenceSession onnxSession;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PredictService(  IOptions<PredictServiceConfiguration> _config, 
                                IWebHostEnvironment webHostEnvironment, 
                                IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _contextFactory = dbContextFactory;
            _webHostEnvironment = webHostEnvironment;
            config = _config.Value;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, config.ModelPath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Cant find neural network file");
            onnxSession = new InferenceSession(fullPath);
        }

        /// <summary>
        /// Performs min-max normalization on input data
        /// </summary>
        /// <param name="input"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private float[] MinMaxScaler(float[] input, float minValue, float maxValue)
        {
            float[] output = new float[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (input[i] - minValue) / (maxValue - minValue);
            }
            return output;
        }

        /// <summary>
        /// Performs inverse min-max normalization on input data
        /// </summary>
        /// <param name="normalizedInput"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private float[] InverseMinMaxScaler(float[] normalizedInput, float minValue, float maxValue)
        {
            float[] originalData = new float[normalizedInput.Length];

            for (int i = 0; i < normalizedInput.Length; i++)
            {
                originalData[i] = normalizedInput[i] * (maxValue - minValue) + minValue;
            }

            return originalData;
        }

        public float[] Predict(float[] input)
        {
            if (input.Length != config.InputLength)
                throw new ArgumentException("Input must be " + config.InputLength + " length array");

            var normalizedInput = MinMaxScaler(input, config.MinScalerValue, config.MaxScalerValue);

            Tensor<float> inputTensor = new DenseTensor<float>(normalizedInput, config.InputShape);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(config.InputName, inputTensor)
            };

            var output = onnxSession.Run(inputs);

            var normalizedResult = output.First().AsTensor<float>().ToArray();

            var result = InverseMinMaxScaler(normalizedResult, config.MinScalerValue, config.MaxScalerValue);

            return result;
        }

        private List<Load> GetLoads(DateTime start, DateTime stop, bool dayInterval = false)
        {
            using(var context = _contextFactory.CreateDbContext())
            {
                var query = context.Loads
                    .Where(load => load.Date >= start && load.Date <= stop);
                if (dayInterval)
                    query = query.Where(load => load.Date.Hour == 0 && load.Date.Minute == 0);

                return query.ToList();
            }
        }
        private Load? GetLoadByDate(DateTime date, AppDbContext context)
        {
            return context.Loads.Where(load => load.Date == date).FirstOrDefault();
        }

        public async Task PredictDataOnRangeAsync(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                DateTime currentDate = start.ToDateTime(TimeOnly.Parse("00:00"));
                int numOfDays = (stop.DayNumber - start.DayNumber) + 1;

                for (int dayNumber = 0; dayNumber < numOfDays; dayNumber++)
                {

                    if (progress != null)
                    {
                        int percent = (int)Math.Round((double)dayNumber / numOfDays * 100);
                        progress.Report(percent);
                    }

                    var dateStart = currentDate.AddDays(-7).AddHours(1);

                    //take 168 previous loads
                    var loads = GetLoads(dateStart, currentDate, false);

                    //if there is not enough data, skip this day
                    if (loads.Count() < config.InputLength || loads.Any(x => x.ActualTotalLoad is null))
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    //take only real load values
                    float?[] tmp = loads.Select(load => load.ActualTotalLoad).ToArray();

                    float[] input = tmp.Select(x => x.Value).ToArray();

                    //predict next 24 hours
                    float[] output = Predict(input);

                    //save predicted values to database
                    for (int i = 0; i < output.Length; i++)
                    {
                        DateTime dateTime = currentDate.AddHours(i + 1);

                        var existingLoad = GetLoadByDate(dateTime, context);

                        if (existingLoad is not null && overrideValues == false)
                            continue;

                        if (existingLoad is null)
                        {
                            var newLoad = new Load
                            {
                                Date = dateTime,
                                PPForecastedTotalLoad = output[i]
                            };
                            context.Loads.Add(newLoad);
                        }
                        else
                        {
                            existingLoad.PPForecastedTotalLoad = output[i];
                            context.Loads.Update(existingLoad);
                        }
                    }
                    await context.SaveChangesAsync();
                    currentDate = currentDate.AddDays(1);
                }
            }          
        }
    }
}
