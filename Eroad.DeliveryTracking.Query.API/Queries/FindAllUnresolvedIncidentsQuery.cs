using Eroad.CQRS.Core.Queries;
using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.Queries
{
    public class FindAllUnresolvedIncidentsQuery : BaseQuery<IncidentEntity>
    {
    }
}
