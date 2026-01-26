using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using Eroad.FleetManagement.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.FleetManagement.Query.Infrastructure.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public VehicleRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(VehicleEntity vehicle)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Vehicles.Add(vehicle);
            _ = await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VehicleEntity vehicle)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Vehicles.Update(vehicle);
            _ = await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid vehicleId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
            if (vehicle == null) return;

            context.Vehicles.Remove(vehicle);
            _ = await context.SaveChangesAsync();
        }

        public async Task<VehicleEntity> GetByIdAsync(Guid vehicleId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == vehicleId);
        }

        public async Task<List<VehicleEntity>> GetAllAsync()
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Vehicles
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<VehicleEntity>> GetByDriverIdAsync(Guid driverId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Vehicles
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<VehicleEntity>> GetByStatusAsync(string status)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Vehicles
                .AsNoTracking()
                .Where(v => v.Status == status)
                .ToListAsync();
        }
    }
}
