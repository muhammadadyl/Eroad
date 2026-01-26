using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using Eroad.FleetManagement.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.FleetManagement.Query.Infrastructure.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public DriverRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(DriverEntity driver)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Drivers.Add(driver);
            _ = await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DriverEntity driver)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Drivers.Update(driver);
            _ = await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid driverId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            var driver = await context.Drivers.FirstOrDefaultAsync(x => x.Id == driverId);
            if (driver == null) return;

            context.Drivers.Remove(driver);
            _ = await context.SaveChangesAsync();
        }

        public async Task<DriverEntity> GetByIdAsync(Guid driverId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == driverId);
        }

        public async Task<List<DriverEntity>> GetAllAsync()
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Drivers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<DriverEntity>> GetByStatusAsync(string status)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Drivers
                .AsNoTracking()
                .Where(d => d.Status == status)
                .ToListAsync();
        }
    }
}
