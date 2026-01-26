using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindAllUnresolvedIncidentsQueryHandler : IRequestHandler<FindAllUnresolvedIncidentsQuery, List<IncidentEntity>>
    {
        private readonly IIncidentRepository _incidentRepository;

        public FindAllUnresolvedIncidentsQueryHandler(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<List<IncidentEntity>> Handle(FindAllUnresolvedIncidentsQuery request, CancellationToken cancellationToken)
        {
            return await _incidentRepository.GetAllUnresolvedAsync();
        }
    }
}
