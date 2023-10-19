using Microsoft.EntityFrameworkCore;
using PowerPredictor.Models;

namespace PowerPredictor.Services.Interfaces
{
    public interface ILoadService
    {
        Task<Load?> GetLoadAsync(int id);
        Task<Load> AddLoadAsync(Load load);
        Task AddLoadsAsync(IEnumerable<Load> loads);
        Task<Load> UpdateLoadAsync(Load load);
        Task<Load?> DeleteLoadAsync(int id);
        Task<IEnumerable<Load>> DownloadLoadsAsync(DateOnly start, DateOnly stop, IProgress<int>? progress, bool overrideValues = true);
        Task<Load?> GetEarliestData();
        Task<Load?> GetLatestData();
        Task<int> GetNumberOfLoads();
        Task<int> GetNumberOfPredictions();
        Task DeleteAllLoadsAsync();
    }
}
