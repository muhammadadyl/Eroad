using Eroad.CQRS.Core.Queries;
using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindIncidentsByDeliveryIdQuery : BaseQuery<IncidentEntity>
    {
        public Guid DeliveryId { get; set; }
    }
}
