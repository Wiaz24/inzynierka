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
        /// Get load by given date
        /// </summary>
        /// <param name="date"></param>
        /// <returns> Load if found, null otherwise </returns>
        Load? GetLoadByDate(DateTime date);

        /// <summary>
        /// Get loads between given dates
        /// </summary>
        /// <param name="start"> Earliest load date in returned list</param>
        /// <param name="stop"> Earliest load date in returned list</param>
        /// <param name="dayInterval"> If true, will return only values at 00:00 hour</param>
        /// <returns> List with loads, can be empty</returns>
        List<Load> GetLoads(DateTime start, DateTime stop, bool dayInterval);

        /// <summary>
        /// Add load to database
        /// </summary>
        /// <param name="load"></param>
        void AddLoad(Load load);

        /// <summary>
        /// Add loads to database
        /// </summary>
        /// <param name="loads"></param>
        void AddLoads(IEnumerable<Load> loads);

        /// <summary>
        /// Update load in database
        /// </summary>
        /// <param name="load"></param>
        /// <returns> Updated load, null if load with given Id was not found </returns>
        Load? UpdateLoad(Load load);

        /// <summary>
        /// Delete load from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Deleted load, null if load with given Id was not found</returns>
        Load? DeleteLoad(int id);

        /// <summary>
        /// Runs web scrapper and downloads loads from given date range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="progress"> Progress object for progress tracking </param>
        /// <param name="overrideValues"> If true, will override existing entries</param>
        /// <returns></returns>
        IEnumerable<Load> DownloadLoads(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues = true);

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
        void DeleteAllLoads();

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
        int InterpolateMissingRealLoad();
    }
}
