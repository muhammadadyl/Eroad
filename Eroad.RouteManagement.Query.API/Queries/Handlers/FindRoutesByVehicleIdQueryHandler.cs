using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.RouteManagement.Query.API.Queries.Handlers
{
    public class FindRoutesByVehicleIdQueryHandler : IRequestHandler<FindRoutesByVehicleIdQuery, List<RouteEntity>>
    {
        private readonly IRouteRepository _routeRepository;

        public FindRoutesByVehicleIdQueryHandler(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task<List<RouteEntity>> Handle(FindRoutesByVehicleIdQuery query, CancellationToken cancellationToken)
        {
            return await _routeRepository.GetByVehicleIdAsync(query.VehicleId);
        }
    }
}
