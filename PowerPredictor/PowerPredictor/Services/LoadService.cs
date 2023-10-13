using Microsoft.EntityFrameworkCore;
using PowerPredictor;
using PowerPredictor.Models;

namespace PowerPredictor.Services
{
    public class LoadService
    {
        private readonly AppDbContext _context;
        public LoadService(AppDbContext dbContext)
        {
            _context = dbContext;
        }
        public async Task<Load?> GetLoadAsync(int id)
        {
            return await _context.FindAsync<Load>(id);
        }
        public async Task<Load> AddLoadAsync(Load load)
        {
            _context.Loads.Add(load);
            await _context.SaveChangesAsync();
            return load;
        }
        public async Task<Load> UpdateLoadAsync(Load load)
        {
            _context.Entry(load).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return load;
        }
        public async Task<Load?> DeleteLoadAsync(int id)
        {
            var load = await _context.Loads.FindAsync(id);
            if (load == null)
            {
                return null;
            }
            _context.Loads.Remove(load);
            await _context.SaveChangesAsync();
            return load;
        }
    }
}
