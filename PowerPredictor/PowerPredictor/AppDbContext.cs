using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PowerPredictor.Models;
using System.Reflection.Emit;

namespace PowerPredictor
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options){}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Load>()
                .HasIndex(l => l.Date)
                .IsUnique();
        }
        public DbSet<Load> Loads { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
    }
}
