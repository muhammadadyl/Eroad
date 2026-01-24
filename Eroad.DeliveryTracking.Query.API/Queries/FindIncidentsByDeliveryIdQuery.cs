using Eroad.CQRS.Core.Queries;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindIncidentsByDeliveryIdQuery : BaseQuery
    {
        public Guid DeliveryId { get; set; }
    }
}
