using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Eroad.DeliveryTracking.Query.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Repositories
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public IncidentRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(IncidentEntity incident)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Incidents.Add(incident);
            _ = await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(IncidentEntity incident)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Incidents.Update(incident);
            _ = await context.SaveChangesAsync();
        }

        public async Task<IncidentEntity> GetByIdAsync(Guid incidentId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Incidents
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == incidentId);
        }

        public async Task<List<IncidentEntity>> GetByDeliveryIdAsync(Guid deliveryId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Incidents
                .AsNoTracking()
                .Where(i => i.DeliveryId == deliveryId)
                .OrderByDescending(i => i.ReportedTimestamp)
                .ToListAsync();
        }

        public async Task<List<IncidentEntity>> GetAllUnresolvedAsync()
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Incidents
                .AsNoTracking()
                .Where(i => !i.Resolved)
                .OrderByDescending(i => i.ReportedTimestamp)
                .ToListAsync();
        }
    }
}
