using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Eroad.DeliveryTracking.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Repositories
{
    public class DeliveryEventLogRepository : IDeliveryEventLogRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public DeliveryEventLogRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(DeliveryEventLogEntity eventLog)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            await context.DeliveryEventLogs.AddAsync(eventLog);
            await context.SaveChangesAsync();
        }

        public async Task<List<DeliveryEventLogEntity>> GetByDeliveryIdAsync(Guid deliveryId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.DeliveryEventLogs
                .Where(e => e.DeliveryId == deliveryId)
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<DeliveryEventLogEntity?> GetByIdAsync(Guid id)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.DeliveryEventLogs.FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
