using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.Domain.Repositories
{
    public interface IDriverRepository
    {
        Task CreateAsync(DriverEntity driver);
        Task UpdateAsync(DriverEntity driver);
        Task DeleteAsync(Guid driverId);
        Task<DriverEntity> GetByIdAsync(Guid driverId);
        Task<List<DriverEntity>> GetAllAsync();
        Task<List<DriverEntity>> GetByStatusAsync(string status);
    }
}
