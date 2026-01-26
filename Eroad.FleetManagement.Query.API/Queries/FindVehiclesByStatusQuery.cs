using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindVehiclesByStatusQuery : BaseQuery<VehicleEntity>
    {
        public required string Status { get; set; }
    }
}
