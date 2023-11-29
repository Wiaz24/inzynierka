namespace PowerPredictor.Services.Interfaces
{
    public interface IPredictService
    {
        /// <summary>
        /// Updates database with predicted values for given date range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="progress"> Progress object to track progress</param>
        /// <param name="overrideValues"> If true, will override existing database entries</param>
        /// <returns></returns>
        Task PredictDataOnRangeAsync(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues);
    }
}
