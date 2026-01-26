using Eroad.CQRS.Core.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRoutesByDriverIdQuery : BaseQuery<RouteEntity>
    {
        public Guid DriverId { get; set; }
    }
}
