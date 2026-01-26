using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.RouteManagement.Query.API.Queries.Handlers
{
    public class FindRouteByIdQueryHandler : IRequestHandler<FindRouteByIdQuery, List<RouteEntity>>
    {
        private readonly IRouteRepository _routeRepository;

        public FindRouteByIdQueryHandler(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task<List<RouteEntity>> Handle(FindRouteByIdQuery query, CancellationToken cancellationToken)
        {
            var route = await _routeRepository.GetByIdAsync(query.Id);
            return route != null ? new List<RouteEntity> { route } : new List<RouteEntity>();
        }
    }
}
