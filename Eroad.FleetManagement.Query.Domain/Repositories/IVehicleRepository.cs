using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.Domain.Repositories
{
    public interface IVehicleRepository
    {
        Task CreateAsync(VehicleEntity vehicle);
        Task UpdateAsync(VehicleEntity vehicle);
        Task DeleteAsync(Guid vehicleId);
        Task<VehicleEntity> GetByIdAsync(Guid vehicleId);
        Task<List<VehicleEntity>> GetAllAsync();
        Task<List<VehicleEntity>> GetByDriverIdAsync(Guid driverId);
        Task<List<VehicleEntity>> GetByStatusAsync(string status);
    }
}
