using Eroad.DeliveryTracking.Query.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eroad.DeliveryTracking.Query.Infrastructure.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DeliveryEntity> Deliveries { get; set; }
        public DbSet<IncidentEntity> Incidents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add indexes on Status column for better query performance
            modelBuilder.Entity<DeliveryEntity>()
                .HasIndex(d => d.Status);

            // Add indexes on RouteId
            modelBuilder.Entity<DeliveryEntity>()
                .HasIndex(d => d.RouteId);

            // Add indexes on DeliveryId for Incidents
            modelBuilder.Entity<IncidentEntity>()
                .HasIndex(i => i.DeliveryId);

            // Add index on Resolved status
            modelBuilder.Entity<IncidentEntity>()
                .HasIndex(i => i.Resolved);
        }
    }
}
