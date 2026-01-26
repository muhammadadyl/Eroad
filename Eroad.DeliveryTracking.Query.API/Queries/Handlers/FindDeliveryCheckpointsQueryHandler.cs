using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindDeliveryCheckpointsQueryHandler : IRequestHandler<FindDeliveryCheckpointsQuery, List<DeliveryCheckpointEntity>>
    {
        private readonly IDeliveryCheckpointRepository _checkpointRepository;

        public FindDeliveryCheckpointsQueryHandler(IDeliveryCheckpointRepository checkpointRepository)
        {
            _checkpointRepository = checkpointRepository;
        }

        public async Task<List<DeliveryCheckpointEntity>> Handle(FindDeliveryCheckpointsQuery request, CancellationToken cancellationToken)
        {
            var checkpoints = await _checkpointRepository.GetByDeliveryIdAsync(request.DeliveryId);
            return checkpoints;
        }
    }
}
