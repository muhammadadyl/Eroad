using Eroad.CQRS.Core.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRouteByIdQuery : BaseQuery<RouteEntity>
    {
        public Guid Id { get; set; }
    }
}
