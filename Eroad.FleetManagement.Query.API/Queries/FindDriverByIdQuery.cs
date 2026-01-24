using Eroad.CQRS.Core.Queries;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindDriverByIdQuery : BaseQuery
    {
        public Guid Id { get; set; }
    }
}
