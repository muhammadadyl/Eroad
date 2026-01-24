using Eroad.CQRS.Core.Queries;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindDeliveryByIdQuery : BaseQuery
    {
        public Guid Id { get; set; }
    }
}
