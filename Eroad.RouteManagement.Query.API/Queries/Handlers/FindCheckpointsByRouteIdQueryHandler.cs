using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.RouteManagement.Query.API.Queries.Handlers
{
    public class FindCheckpointsByRouteIdQueryHandler : IRequestHandler<FindCheckpointsByRouteIdQuery, List<CheckpointEntity>>
    {
        private readonly ICheckpointRepository _checkpointRepository;

        public FindCheckpointsByRouteIdQueryHandler(ICheckpointRepository checkpointRepository)
        {
            _checkpointRepository = checkpointRepository;
        }

        public async Task<List<CheckpointEntity>> Handle(FindCheckpointsByRouteIdQuery query, CancellationToken cancellationToken)
        {
            return await _checkpointRepository.GetByRouteIdAsync(query.RouteId);
        }
    }
}
