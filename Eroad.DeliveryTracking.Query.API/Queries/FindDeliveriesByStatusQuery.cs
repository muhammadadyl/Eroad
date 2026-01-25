using Eroad.CQRS.Core.Queries;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindDeliveriesByStatusQuery : BaseQuery
    {
        public required string Status { get; set; }
    }
}
