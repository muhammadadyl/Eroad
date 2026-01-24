using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IRouteRepository _routeRepository;
        private readonly ICheckpointRepository _checkpointRepository;

        public QueryHandler(IRouteRepository routeRepository, ICheckpointRepository checkpointRepository)
        {
            _routeRepository = routeRepository;
            _checkpointRepository = checkpointRepository;
        }

        public async Task<List<RouteEntity>> HandleAsync(FindAllRoutesQuery query)
        {
            return await _routeRepository.GetAllAsync();
        }

        public async Task<List<RouteEntity>> HandleAsync(FindRouteByIdQuery query)
        {
            var route = await _routeRepository.GetByIdAsync(query.Id);
            return route != null ? new List<RouteEntity> { route } : new List<RouteEntity>();
        }

        public async Task<List<RouteEntity>> HandleAsync(FindRoutesByStatusQuery query)
        {
            return await _routeRepository.GetByStatusAsync(query.Status);
        }

        public async Task<List<RouteEntity>> HandleAsync(FindRoutesByDriverIdQuery query)
        {
            return await _routeRepository.GetByDriverIdAsync(query.DriverId);
        }

        public async Task<List<RouteEntity>> HandleAsync(FindRoutesByVehicleIdQuery query)
        {
            return await _routeRepository.GetByVehicleIdAsync(query.VehicleId);
        }

        public async Task<List<CheckpointEntity>> HandleAsync(FindCheckpointsByRouteIdQuery query)
        {
            return await _checkpointRepository.GetByRouteIdAsync(query.RouteId);
        }
    }
}
