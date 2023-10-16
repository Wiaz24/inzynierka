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
        Task<IEnumerable<Load>> DownloadLoadsAsync(DateOnly start, DateOnly stop);
    }
}
