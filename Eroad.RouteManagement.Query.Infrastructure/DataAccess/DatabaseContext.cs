using Eroad.RouteManagement.Query.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eroad.RouteManagement.Query.Infrastructure.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RouteEntity> Routes { get; set; }
        public DbSet<CheckpointEntity> Checkpoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for CheckpointEntity
            modelBuilder.Entity<CheckpointEntity>()
                .HasKey(c => new { c.RouteId, c.Sequence });

            // Add indexes on Status column for better query performance
            modelBuilder.Entity<RouteEntity>()
                .HasIndex(r => r.Status);
        }
    }
}
