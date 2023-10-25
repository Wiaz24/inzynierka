namespace PowerPredictor.Services.Interfaces
{
    public interface IPredictService
    {
        float[] Predict(float[] input);

        Task PredictDataOnRangeAsync(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues);
    }
}
