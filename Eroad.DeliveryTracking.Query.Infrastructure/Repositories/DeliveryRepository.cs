using Eroad.DeliveryTracking.Common;
using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Eroad.DeliveryTracking.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Repositories
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public DeliveryRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(DeliveryEntity delivery)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Deliveries.Add(delivery);
            _ = await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DeliveryEntity delivery)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Deliveries.Update(delivery);
            _ = await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid deliveryId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            var delivery = await context.Deliveries.FirstOrDefaultAsync(x => x.Id == deliveryId);
            if (delivery == null) return;

            context.Deliveries.Remove(delivery);
            _ = await context.SaveChangesAsync();
        }

        public async Task<DeliveryEntity> GetByIdAsync(Guid deliveryId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Deliveries
                .Include(d => d.Incidents)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == deliveryId);
        }

        public async Task<List<DeliveryEntity>> GetAllAsync()
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Deliveries
                .Include(d => d.Incidents)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<DeliveryEntity>> GetByStatusAsync(string status)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Deliveries
                .Include(d => d.Incidents)
                .AsNoTracking()
                .Where(d => d.Status == status)
                .ToListAsync();
        }

        public async Task<List<DeliveryEntity>> GetByRouteIdAsync(Guid routeId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Deliveries
                .Include(d => d.Incidents)
                .AsNoTracking()
                .Where(d => d.RouteId == routeId)
                .ToListAsync();
        }

        public async Task<List<DeliveryEntity>> GetActiveDeliveriesByDriverAsync(Guid driverId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            var activeStatuses = new[] 
            { 
                DeliveryStatus.PickedUp.ToString(), 
                DeliveryStatus.InTransit.ToString(), 
                DeliveryStatus.OutForDelivery.ToString() 
            };
            
            return await context.Deliveries
                .Include(d => d.Incidents)
                .AsNoTracking()
                .Where(d => d.DriverId == driverId && activeStatuses.Contains(d.Status))
                .ToListAsync();
        }

        public async Task<List<DeliveryEntity>> GetActiveDeliveriesByVehicleAsync(Guid vehicleId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            var activeStatuses = new[] 
            { 
                DeliveryStatus.PickedUp.ToString(), 
                DeliveryStatus.InTransit.ToString(), 
                DeliveryStatus.OutForDelivery.ToString() 
            };
            
            return await context.Deliveries
                .Include(d => d.Incidents)
                .AsNoTracking()
                .Where(d => d.VehicleId == vehicleId && activeStatuses.Contains(d.Status))
                .ToListAsync();
        }
    }
}
