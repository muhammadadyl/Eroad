using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindDriverByIdQuery : BaseQuery<DriverEntity>
    {
        public Guid Id { get; set; }
    }
}
