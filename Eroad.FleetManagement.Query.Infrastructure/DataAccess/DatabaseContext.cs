using Eroad.FleetManagement.Query.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eroad.FleetManagement.Query.Infrastructure.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DriverEntity> Drivers { get; set; }
        public DbSet<VehicleEntity> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add indexes on Status columns for better query performance
            modelBuilder.Entity<DriverEntity>()
                .HasIndex(d => d.Status)
                .HasDatabaseName("IX_Driver_Status");

            modelBuilder.Entity<VehicleEntity>()
                .HasIndex(v => v.Status)
                .HasDatabaseName("IX_Vehicle_Status");
        }
    }
}
