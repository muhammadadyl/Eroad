using Eroad.CQRS.Core.Queries;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindVehiclesByStatusQuery : BaseQuery
    {
        public string Status { get; set; }
    }
}
