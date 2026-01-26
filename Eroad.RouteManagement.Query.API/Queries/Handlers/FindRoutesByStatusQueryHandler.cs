using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.RouteManagement.Query.API.Queries.Handlers
{
    public class FindRoutesByStatusQueryHandler : IRequestHandler<FindRoutesByStatusQuery, List<RouteEntity>>
    {
        private readonly IRouteRepository _routeRepository;

        public FindRoutesByStatusQueryHandler(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task<List<RouteEntity>> Handle(FindRoutesByStatusQuery query, CancellationToken cancellationToken)
        {
            return await _routeRepository.GetByStatusAsync(query.Status);
        }
    }
}
