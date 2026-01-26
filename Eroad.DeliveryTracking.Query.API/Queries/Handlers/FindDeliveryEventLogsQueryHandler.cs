using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindDeliveryEventLogsQueryHandler : IRequestHandler<FindDeliveryEventLogsQuery, List<DeliveryEventLogEntity>>
    {
        private readonly IDeliveryEventLogRepository _eventLogRepository;

        public FindDeliveryEventLogsQueryHandler(IDeliveryEventLogRepository eventLogRepository)
        {
            _eventLogRepository = eventLogRepository;
        }

        public async Task<List<DeliveryEventLogEntity>> Handle(FindDeliveryEventLogsQuery request, CancellationToken cancellationToken)
        {
            return await _eventLogRepository.GetByDeliveryIdAsync(request.DeliveryId);
        }
    }
}
