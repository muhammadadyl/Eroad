using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.Domain.Repositories
{
    public interface IRouteRepository
    {
        Task CreateAsync(RouteEntity route);
        Task UpdateAsync(RouteEntity route);
        Task DeleteAsync(Guid routeId);
        Task<RouteEntity> GetByIdAsync(Guid routeId);
        Task<List<RouteEntity>> GetAllAsync();
        Task<List<RouteEntity>> GetByStatusAsync(string status);
    }
}
