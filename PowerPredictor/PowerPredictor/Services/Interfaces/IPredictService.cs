namespace PowerPredictor.Services.Interfaces
{
    public interface IPredictService
    {
        /// <summary>
        /// Make prediction using neural network model
        /// </summary>
        /// <param name="input"> 168 previous data values with hour interval</param>
        /// <returns> 24 predicted data values with hour interval</returns>
        float[] Predict(float[] input);

        /// <summary>
        /// Updates database with predicted values for given date range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="progress"> Progress object to track progress</param>
        /// <param name="overrideValues"> If true, will override existing database entries</param>
        /// <returns></returns>
        void PredictDataOnRange(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues);
    }
}
