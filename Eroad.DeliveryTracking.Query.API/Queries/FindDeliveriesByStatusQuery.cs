using Eroad.CQRS.Core.Queries;
using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindDeliveriesByStatusQuery : BaseQuery<DeliveryEntity>
    {
        public required string Status { get; set; }
    }
}
