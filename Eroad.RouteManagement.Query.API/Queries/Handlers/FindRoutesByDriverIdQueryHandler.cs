using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.RouteManagement.Query.API.Queries.Handlers
{
    public class FindRoutesByDriverIdQueryHandler : IRequestHandler<FindRoutesByDriverIdQuery, List<RouteEntity>>
    {
        private readonly IRouteRepository _routeRepository;

        public FindRoutesByDriverIdQueryHandler(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task<List<RouteEntity>> Handle(FindRoutesByDriverIdQuery query, CancellationToken cancellationToken)
        {
            return await _routeRepository.GetByDriverIdAsync(query.DriverId);
        }
    }
}
