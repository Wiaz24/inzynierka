using Microsoft.EntityFrameworkCore;
using PowerPredictor.Models;

namespace PowerPredictor.Services.Interfaces
{
    public interface ILoadService
    {
        Load? GetLoad(int id);
        Load? GetLoadByDate(DateTime date);
        List<Load> GetLoads(DateTime start, DateTime stop, bool dayInterval);
        void AddLoad(Load load);
        void AddLoads(IEnumerable<Load> loads);
        Load UpdateLoad(Load load);
        Load? DeleteLoad(int id);
        IEnumerable<Load> DownloadLoads(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues = true);
        Load? GetEarliestData();
        Load? GetLatestData();
        int GetNumberOfLoads();
        int GetNumberOfPredictions();
        void DeleteAllLoads();
        List<DateTime> GetMissingRealLoad();
        List<DateTime> GetMissingPPForecast();

        int InterpolateMissingRealLoad();
    }
}
