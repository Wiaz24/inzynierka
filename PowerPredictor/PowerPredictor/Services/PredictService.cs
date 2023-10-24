using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using PowerPredictor.Services.Interfaces;

namespace PowerPredictor.Services
{
    public class PredictServiceConfiguration
    {
        public string ModelPath { get; set; }
    }


    public class PredictService : IPredictService
    {
        private readonly PredictServiceConfiguration config;
        private readonly InferenceSession onnxSession;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly float minScalerValue = 10600.0f;
        private readonly float maxScalerValue = 27374.68f;


        public PredictService(IOptions<PredictServiceConfiguration> config, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            this.config = config.Value;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "onnx", "LSTM_predictor.onnx");

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Cant find neural network file");
            onnxSession = new InferenceSession(fullPath);
        }

        private float[] MinMaxScaler(float[] input, float minValue, float maxValue)
        {
            float[] output = new float[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (input[i] - minValue) / (maxValue - minValue);
            }
            return output;
        }

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
            var normalizedInput = MinMaxScaler(input, minScalerValue, maxScalerValue);

            int[] inputShape = new int[] { 1, normalizedInput.Length, 1 };    //batch size, frame length, channels
            Tensor<float> inputTensor = new DenseTensor<float>(normalizedInput, inputShape);

            var inputName = "modelInput";
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
            };

            var output = onnxSession.Run(inputs);

            var normalizedResult = output.First().AsTensor<float>().ToArray();

            var result = InverseMinMaxScaler(normalizedResult, minScalerValue, maxScalerValue);

            return result;
        }
    }
}
