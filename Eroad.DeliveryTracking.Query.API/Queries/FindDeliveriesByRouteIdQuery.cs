using Eroad.CQRS.Core.Queries;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindDeliveriesByRouteIdQuery : BaseQuery
    {
        public Guid RouteId { get; set; }
    }
}
