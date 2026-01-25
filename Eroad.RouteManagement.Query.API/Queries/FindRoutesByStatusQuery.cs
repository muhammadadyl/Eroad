using Eroad.CQRS.Core.Queries;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRoutesByStatusQuery : BaseQuery
    {
        public required string Status { get; set; }
    }
}
