using Eroad.CQRS.Core.Queries;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindCheckpointsByRouteIdQuery : BaseQuery
    {
        public Guid RouteId { get; set; }
    }
}
