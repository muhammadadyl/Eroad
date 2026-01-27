using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.Domain.Repositories
{
    public interface IDeliveryRepository
    {
        Task CreateAsync(DeliveryEntity delivery);
        Task UpdateAsync(DeliveryEntity delivery);
        Task DeleteAsync(Guid deliveryId);
        Task<DeliveryEntity> GetByIdAsync(Guid deliveryId);
        Task<List<DeliveryEntity>> GetAllAsync();
        Task<List<DeliveryEntity>> GetByStatusAsync(string status);
        Task<List<DeliveryEntity>> GetByRouteIdAsync(Guid routeId);
        Task<List<DeliveryEntity>> GetActiveDeliveriesByDriverAsync(Guid driverId);
        Task<List<DeliveryEntity>> GetActiveDeliveriesByVehicleAsync(Guid vehicleId);
    }
}
