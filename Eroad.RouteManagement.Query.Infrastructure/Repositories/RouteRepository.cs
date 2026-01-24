using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using Eroad.RouteManagement.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.RouteManagement.Query.Infrastructure.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public RouteRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(RouteEntity route)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Routes.Add(route);
            _ = await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RouteEntity route)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Routes.Update(route);
            _ = await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid routeId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            var route = await context.Routes.FirstOrDefaultAsync(x => x.Id == routeId);
            if (route == null) return;

            context.Routes.Remove(route);
            _ = await context.SaveChangesAsync();
        }

        public async Task<RouteEntity> GetByIdAsync(Guid routeId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Routes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == routeId);
        }

        public async Task<List<RouteEntity>> GetAllAsync()
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Routes
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<RouteEntity>> GetByStatusAsync(string status)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Routes
                .AsNoTracking()
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<List<RouteEntity>> GetByDriverIdAsync(Guid driverId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Routes
                .AsNoTracking()
                .Where(r => r.AssignedDriverId == driverId)
                .ToListAsync();
        }

        public async Task<List<RouteEntity>> GetByVehicleIdAsync(Guid vehicleId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Routes
                .AsNoTracking()
                .Where(r => r.AssignedVehicleId == vehicleId)
                .ToListAsync();
        }
    }
}
