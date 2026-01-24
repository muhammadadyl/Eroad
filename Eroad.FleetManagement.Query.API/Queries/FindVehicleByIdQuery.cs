using Eroad.CQRS.Core.Queries;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindVehicleByIdQuery : BaseQuery
    {
        public Guid Id { get; set; }
    }
}
