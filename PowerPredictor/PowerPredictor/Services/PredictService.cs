using Microsoft.AspNetCore.Hosting;
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
        private readonly ILoadService _loadService;

        public PredictService(IOptions<PredictServiceConfiguration> _config, IWebHostEnvironment webHostEnvironment, ILoadService loadService)
        {
            _webHostEnvironment = webHostEnvironment;
            _loadService = loadService;
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

        public void PredictDataOnRange(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues)
        {
            DateTime currentDate = start.ToDateTime(TimeOnly.Parse("00:00"));
            int numOfDays = (stop.DayNumber - start.DayNumber) + 1;

            for (int dayNumber = 0; dayNumber < numOfDays; dayNumber++)
            {
                
                if (progress != null)
                {
                    int percent = (int)Math.Round((double)dayNumber / numOfDays * 100);
                    //int percent = (int)Math.Round((double)(currentDate - start.ToDateTime(TimeOnly.Parse("00:00"))).TotalHours / (stop.ToDateTime(TimeOnly.Parse("23:00")) - start.ToDateTime(TimeOnly.Parse("00:00"))).TotalHours * 100);
                    progress.Report(percent);
                }

                var dateStart = currentDate.AddDays(-7).AddHours(1);

                //take 168 previous loads
                var loads = _loadService.GetLoads(dateStart, currentDate, false);
                
                //if there is not enough data, skip this day
                if (loads.Count() < config.InputLength || loads.Any(x => x.ActualTotalLoad is null ))
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
                    var existingLoad = _loadService.GetLoadByDate(dateTime);
                    if (existingLoad is not null && overrideValues == false)
                        continue;

                    if (existingLoad is null)
                    {
                        var newLoad = new Load
                        {
                            Date = dateTime,
                            PPForecastedTotalLoad = output[i]
                        };

                        _loadService.AddLoad(newLoad);

                    }
                    else
                    {
                        existingLoad.PPForecastedTotalLoad = output[i];

                        _loadService.UpdateLoad(existingLoad);
                    }
                }
                currentDate = currentDate.AddDays(1);
            }
        }
    }
}
