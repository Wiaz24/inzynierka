namespace PowerPredictor.Services.Interfaces
{
    public interface IPredictService
    {
        float[] Predict(float[] input);
    }
}
