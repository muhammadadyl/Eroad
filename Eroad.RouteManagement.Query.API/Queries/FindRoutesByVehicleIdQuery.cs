using Eroad.CQRS.Core.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRoutesByVehicleIdQuery : BaseQuery<RouteEntity>
    {
        public Guid VehicleId { get; set; }
    }
}
