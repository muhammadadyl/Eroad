using Eroad.CQRS.Core.Queries;
using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindDeliveryCheckpointsQuery : BaseQuery<DeliveryCheckpointEntity>
    {
        public Guid DeliveryId { get; set; }
    }
}
