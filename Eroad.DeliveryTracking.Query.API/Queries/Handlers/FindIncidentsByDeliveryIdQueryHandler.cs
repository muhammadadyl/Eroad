using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindIncidentsByDeliveryIdQueryHandler : IRequestHandler<FindIncidentsByDeliveryIdQuery, List<IncidentEntity>>
    {
        private readonly IIncidentRepository _incidentRepository;

        public FindIncidentsByDeliveryIdQueryHandler(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<List<IncidentEntity>> Handle(FindIncidentsByDeliveryIdQuery request, CancellationToken cancellationToken)
        {
            return await _incidentRepository.GetByDeliveryIdAsync(request.DeliveryId);
        }
    }
}
