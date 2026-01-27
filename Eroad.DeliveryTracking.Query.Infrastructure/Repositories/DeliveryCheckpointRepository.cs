using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Eroad.DeliveryTracking.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Repositories
{
    public class DeliveryCheckpointRepository : IDeliveryCheckpointRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public DeliveryCheckpointRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(DeliveryCheckpointEntity checkpoint)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.DeliveryCheckpoints.Add(checkpoint);
            await context.SaveChangesAsync();
        }

        public async Task<DeliveryCheckpointEntity?> GetByIdAsync(Guid deliveryId, int sequence)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.DeliveryCheckpoints
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.DeliveryId == deliveryId && c.Sequence == sequence);
        }

        public async Task<List<DeliveryCheckpointEntity>> GetByDeliveryIdAsync(Guid deliveryId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.DeliveryCheckpoints
                .AsNoTracking()
                .Where(c => c.DeliveryId == deliveryId)
                .OrderBy(c => c.Sequence)
                .ToListAsync();
        }
    }
}
