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
        public DbSet<DeliveryCheckpointEntity> DeliveryCheckpoints { get; set; }
        public DbSet<DeliveryEventLogEntity> DeliveryEventLogs { get; set; }

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

            // Configure composite key for DeliveryCheckpoint
            modelBuilder.Entity<DeliveryCheckpointEntity>()
                .HasKey(dc => new { dc.DeliveryId, dc.Sequence });

            // Add indexes for DeliveryCheckpoint
            modelBuilder.Entity<DeliveryCheckpointEntity>()
                .HasIndex(dc => dc.DeliveryId);

            modelBuilder.Entity<DeliveryCheckpointEntity>()
                .HasIndex(dc => dc.RouteId);

            // Configure indexes for DeliveryEventLog
            modelBuilder.Entity<DeliveryEventLogEntity>()
                .HasIndex(e => e.DeliveryId);

            modelBuilder.Entity<DeliveryEventLogEntity>()
                .HasIndex(e => e.EventCategory);

            modelBuilder.Entity<DeliveryEventLogEntity>()
                .HasIndex(e => e.OccurredAt);
        }
    }
}
