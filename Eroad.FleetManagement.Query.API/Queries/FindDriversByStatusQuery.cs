using Eroad.CQRS.Core.Queries;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class FindDriversByStatusQuery : BaseQuery
    {
        public string Status { get; set; }
    }
}
