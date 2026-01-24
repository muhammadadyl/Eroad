using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.Domain.Repositories
{
    public interface ICheckpointRepository
    {
        Task CreateAsync(CheckpointEntity checkpoint);
        Task UpdateAsync(CheckpointEntity checkpoint);
        Task<CheckpointEntity> GetByIdAsync(Guid routeId, int sequence);
        Task<List<CheckpointEntity>> GetByRouteIdAsync(Guid routeId);
    }
}
