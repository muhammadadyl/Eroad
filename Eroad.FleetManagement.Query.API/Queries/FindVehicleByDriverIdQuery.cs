using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindVehicleByDriverIdQuery : BaseQuery<VehicleEntity>
    {
        public Guid DriverId { get; set; }
    }
}
