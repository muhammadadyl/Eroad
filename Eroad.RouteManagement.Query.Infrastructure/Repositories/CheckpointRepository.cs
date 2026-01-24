using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using Eroad.RouteManagement.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.RouteManagement.Query.Infrastructure.Repositories
{
    public class CheckpointRepository : ICheckpointRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public CheckpointRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(CheckpointEntity checkpoint)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Checkpoints.Add(checkpoint);
            _ = await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CheckpointEntity checkpoint)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Checkpoints.Update(checkpoint);
            _ = await context.SaveChangesAsync();
        }

        public async Task<CheckpointEntity> GetByIdAsync(Guid routeId, int sequence)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Checkpoints
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.RouteId == routeId && c.Sequence == sequence);
        }

        public async Task<List<CheckpointEntity>> GetByRouteIdAsync(Guid routeId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Checkpoints
                .AsNoTracking()
                .Where(c => c.RouteId == routeId)
                .OrderBy(c => c.Sequence)
                .ToListAsync();
        }
    }
}
