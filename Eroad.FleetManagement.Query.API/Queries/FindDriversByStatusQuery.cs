using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindDriversByStatusQuery : BaseQuery<DriverEntity>
    {
        public required string Status { get; set; }
    }
}
