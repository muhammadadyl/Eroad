using Eroad.CQRS.Core.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.Queries
{
    public class FindCheckpointsByRouteIdQuery : BaseQuery<CheckpointEntity>
    {
        public Guid RouteId { get; set; }
    }
}
