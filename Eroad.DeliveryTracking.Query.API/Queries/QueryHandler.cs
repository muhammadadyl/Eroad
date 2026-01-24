using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IIncidentRepository _incidentRepository;

        public QueryHandler(IDeliveryRepository deliveryRepository, IIncidentRepository incidentRepository)
        {
            _deliveryRepository = deliveryRepository;
            _incidentRepository = incidentRepository;
        }

        public async Task<List<DeliveryEntity>> HandleAsync(FindAllDeliveriesQuery query)
        {
            return await _deliveryRepository.GetAllAsync();
        }

        public async Task<List<DeliveryEntity>> HandleAsync(FindDeliveryByIdQuery query)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(query.Id);
            return delivery != null ? new List<DeliveryEntity> { delivery } : new List<DeliveryEntity>();
        }

        public async Task<List<DeliveryEntity>> HandleAsync(FindDeliveriesByStatusQuery query)
        {
            return await _deliveryRepository.GetByStatusAsync(query.Status);
        }

        public async Task<List<DeliveryEntity>> HandleAsync(FindDeliveriesByRouteIdQuery query)
        {
            return await _deliveryRepository.GetByRouteIdAsync(query.RouteId);
        }

        public async Task<List<IncidentEntity>> HandleAsync(FindIncidentsByDeliveryIdQuery query)
        {
            return await _incidentRepository.GetByDeliveryIdAsync(query.DeliveryId);
        }

        public async Task<List<IncidentEntity>> HandleAsync(FindAllUnresolvedIncidentsQuery query)
        {
            return await _incidentRepository.GetAllUnresolvedAsync();
        }
    }
}
