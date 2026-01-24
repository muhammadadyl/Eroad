using Eroad.CQRS.Core.Queries;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRouteByIdQuery : BaseQuery
    {
        public Guid Id { get; set; }
    }
}
