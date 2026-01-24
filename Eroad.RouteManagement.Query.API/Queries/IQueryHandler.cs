using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public interface IQueryHandler
    {
        Task<List<RouteEntity>> HandleAsync(FindAllRoutesQuery query);
        Task<List<RouteEntity>> HandleAsync(FindRouteByIdQuery query);
        Task<List<RouteEntity>> HandleAsync(FindRoutesByStatusQuery query);
        Task<List<RouteEntity>> HandleAsync(FindRoutesByDriverIdQuery query);
        Task<List<RouteEntity>> HandleAsync(FindRoutesByVehicleIdQuery query);
        Task<List<CheckpointEntity>> HandleAsync(FindCheckpointsByRouteIdQuery query);
    }
}
