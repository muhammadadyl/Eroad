using Eroad.CQRS.Core.Queries;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindRoutesByVehicleIdQuery : BaseQuery
    {
        public Guid VehicleId { get; set; }
    }
}
