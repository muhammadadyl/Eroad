using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.Domain.Repositories
{
    public interface IDeliveryCheckpointRepository
    {
        Task CreateAsync(DeliveryCheckpointEntity checkpoint);
        Task<DeliveryCheckpointEntity?> GetByIdAsync(Guid deliveryId, int sequence);
        Task<List<DeliveryCheckpointEntity>> GetByDeliveryIdAsync(Guid deliveryId);
    }
}
