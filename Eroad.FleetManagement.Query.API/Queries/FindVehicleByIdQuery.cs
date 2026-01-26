using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindVehicleByIdQuery : BaseQuery<VehicleEntity>
    {
        public Guid Id { get; set; }
    }
}
