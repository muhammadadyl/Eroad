using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public interface IQueryHandler
    {
        Task<List<DeliveryEntity>> HandleAsync(FindAllDeliveriesQuery query);
        Task<List<DeliveryEntity>> HandleAsync(FindDeliveryByIdQuery query);
        Task<List<DeliveryEntity>> HandleAsync(FindDeliveriesByStatusQuery query);
        Task<List<DeliveryEntity>> HandleAsync(FindDeliveriesByRouteIdQuery query);
        Task<List<IncidentEntity>> HandleAsync(FindIncidentsByDeliveryIdQuery query);
        Task<List<IncidentEntity>> HandleAsync(FindAllUnresolvedIncidentsQuery query);
    }
}
