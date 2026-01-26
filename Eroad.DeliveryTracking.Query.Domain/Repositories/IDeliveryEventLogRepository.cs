using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.Domain.Repositories
{
    public interface IDeliveryEventLogRepository
    {
        Task CreateAsync(DeliveryEventLogEntity eventLog);
        Task<List<DeliveryEventLogEntity>> GetByDeliveryIdAsync(Guid deliveryId);
        Task<DeliveryEventLogEntity?> GetByIdAsync(Guid id);
    }
}
