using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.RouteManagement.Query.API.Queries.Handlers
{
    public class FindAllRoutesQueryHandler : IRequestHandler<FindAllRoutesQuery, List<RouteEntity>>
    {
        private readonly IRouteRepository _routeRepository;

        public FindAllRoutesQueryHandler(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task<List<RouteEntity>> Handle(FindAllRoutesQuery query, CancellationToken cancellationToken)
        {
            return await _routeRepository.GetAllAsync();
        }
    }
}
