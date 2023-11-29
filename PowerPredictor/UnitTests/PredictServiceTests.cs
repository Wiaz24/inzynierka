using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowerPredictor;
using PowerPredictor.Services;

namespace UnitTests
{
    public class PredictServiceTests
    {
        private readonly Mock<IOptions<PredictServiceConfiguration>> mockOptions;
        private readonly Mock<IDbContextFactory<AppDbContext>> mockContextFactory;
        private readonly Mock<IWebHostEnvironment> mockWebHostEnvironment;


        private readonly PredictServiceConfiguration predictServiceConfiguration;
        private readonly PredictService predictService;
        public PredictServiceTests()
        {
            predictServiceConfiguration = new PredictServiceConfiguration()
            {
                ModelPath = "onnx/LSTM_predictor.onnx",
                InputName = "modelInput",
                OutputName = "modelOutput",
                InputLength = 168,
                OutputLength = 24,
                InputShape = new int[] { 1, 168, 1 },
                MinScalerValue = 10600.0f,
                MaxScalerValue = 7374.68f
            };

            mockOptions = new Mock<IOptions<PredictServiceConfiguration>>();
            mockOptions.Setup(x => x.Value).Returns(predictServiceConfiguration);

            mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();

            mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns("C:\\Users\\Ja\\Desktop\\praca inżynierska\\inzynierka\\PowerPredictor\\PowerPredictor\\wwwroot");

            predictService = new PredictService(mockOptions.Object, mockWebHostEnvironment.Object, mockContextFactory.Object);
        }


        [Fact]
        public void MinMaxScalerTest()
        {
            // Arrange
            float[] input = { 1, 2, 3, 4, 5 };
            float minValue = 1;
            float maxValue = 5;
            float[] expected = { 0, 0.25f, 0.5f, 0.75f, 1 };

            // Act
            float[] actual = PredictService.MinMaxScaler(input, minValue, maxValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InverseMinMaxScalerTest()
        {
            // Arrange
            float[] input = { 0, 0.25f, 0.5f, 0.75f, 1 };
            float minValue = 1;
            float maxValue = 5;
            float[] expected = { 1, 2, 3, 4, 5 };

            // Act
            float[] actual = PredictService.InverseMinMaxScaler(input, minValue, maxValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Predict_WithValidInput_ReturnsExpectedResult()
        {
            // Przygotowanie danych testowych
            float acceptedError = 0.4f;
            float[] input = new float[168];
            float[] dailyInput = new float[]{
                16057.4f, 15026.86f, 14516.26f, 14027.0f, 13897.89f, 14021.81f, 
                14231.11f, 14724.05f, 14924.23f, 15934.59f, 17129.39f, 17881.43f, 
                18155.0f, 18356.43f, 18516.54f, 18229.81f, 17701.39f, 17415.93f, 
                17957.54f, 18822.11f, 18947.9f, 18612.66f, 17633.71f, 16724.71f,
            };

            for (int days = 0; days < 7; days++)
                for (int hours = 0; hours < 24; hours++)
                    input[days * 24 + hours] = dailyInput[hours];

            float[] expectedResult = dailyInput;
            
            // Wywołanie metody testowanej
            var result = predictService.Predict(input);

            // Sprawdzenie czy wynik jest zgodny z oczekiwaniami
            Assert.All(result, value =>
            {
                int i = 0;
                Assert.InRange(value, expectedResult[i]*(1.0f-acceptedError),  expectedResult[i]*(1.0f+acceptedError));
                i++;
            });
        }
    }
}