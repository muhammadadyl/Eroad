using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.Domain.Repositories
{
    public interface IIncidentRepository
    {
        Task CreateAsync(IncidentEntity incident);
        Task UpdateAsync(IncidentEntity incident);
        Task<IncidentEntity> GetByIdAsync(Guid incidentId);
        Task<List<IncidentEntity>> GetByDeliveryIdAsync(Guid deliveryId);
        Task<List<IncidentEntity>> GetAllUnresolvedAsync();
    }
}
