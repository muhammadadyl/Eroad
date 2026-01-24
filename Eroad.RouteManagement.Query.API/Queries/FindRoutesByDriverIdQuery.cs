using Eroad.CQRS.Core.Queries;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRoutesByDriverIdQuery : BaseQuery
    {
        public Guid DriverId { get; set; }
    }
}
