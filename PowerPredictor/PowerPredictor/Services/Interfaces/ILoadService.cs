using Microsoft.EntityFrameworkCore;
using PowerPredictor.Models;

namespace PowerPredictor.Services.Interfaces
{
    public interface ILoadService
    {
        /// <summary>
        /// Get load by given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Load if found, null otherwise </returns>
        Load? GetLoadById(int id);

        /// <summary>
        /// Get loads between given dates
        /// </summary>
        /// <param name="start"> Earliest load date in returned list</param>
        /// <param name="stop"> Earliest load date in returned list</param>
        /// <param name="dayInterval"> If true, will return only values at 00:00 hour</param>
        /// <returns> List with loads, can be empty</returns>
        Task<List<Load>> GetLoadsAsync(DateTime start, DateTime stop, bool dayInterval);


        /// <summary>
        /// Runs web scrapper and downloads loads from given date range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="progress"> Progress object for progress tracking </param>
        /// <param name="overrideValues"> If true, will override existing entries</param>
        /// <returns></returns>
        Task<IEnumerable<Load>> DownloadLoadsAsync(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues = true);

        /// <summary>
        /// Get earliest load in database
        /// </summary>
        /// <returns></returns>
        Load? GetEarliestData();

        /// <summary>
        /// Get latest load in database
        /// </summary>
        /// <returns></returns>
        Load? GetLatestData();

        /// <summary>
        /// Get number of loads in database
        /// </summary>
        /// <returns></returns>
        int GetNumberOfLoads();

        /// <summary>
        /// Get number of predictions in database
        /// </summary>
        /// <returns></returns>
        int GetNumberOfPredictions();

        /// <summary>
        /// Delete all loads from database
        /// </summary>
        Task DeleteAllLoadsAsync();

        /// <summary>
        /// Get missing real load dates
        /// </summary>
        /// <returns></returns>
        List<DateTime> GetMissingRealLoad();

        /// <summary>
        /// Get missing PP forecast dates
        /// </summary>
        /// <returns></returns>
        List<DateTime> GetMissingPPForecast();

        /// <summary>
        /// Interpolate missing PP forecast values
        /// </summary>
        /// <returns> Number of interpolated database entries</returns>
        Task<int> InterpolateMissingRealLoadAsync();
    }
}
