using Eroad.CQRS.Core.Queries;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRoutesByStatusQuery : BaseQuery
    {
        public string Status { get; set; }
    }
}
