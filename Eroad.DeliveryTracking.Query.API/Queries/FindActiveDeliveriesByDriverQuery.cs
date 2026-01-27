using Eroad.CQRS.Core.Queries;
using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindActiveDeliveriesByDriverQuery : BaseQuery<DeliveryEntity>
    {
        public Guid DriverId { get; set; }
    }
}
