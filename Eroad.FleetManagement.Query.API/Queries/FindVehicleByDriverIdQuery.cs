using Eroad.CQRS.Core.Queries;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindVehicleByDriverIdQuery : BaseQuery
    {
        public Guid DriverId { get; set; }
    }
}
