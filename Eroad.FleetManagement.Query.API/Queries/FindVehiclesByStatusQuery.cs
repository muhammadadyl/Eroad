using Eroad.CQRS.Core.Queries;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindVehiclesByStatusQuery : BaseQuery
    {
        public required string Status { get; set; }
    }
}
