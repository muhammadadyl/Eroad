using Eroad.CQRS.Core.Queries;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindDriversByStatusQuery : BaseQuery
    {
        public required string Status { get; set; }
    }
}
