using Microsoft.EntityFrameworkCore;
using PowerPredictor.DbModels;

namespace PowerPredictor
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options){}
        public DbSet<Load> Loads { get; set; }
    }
}
