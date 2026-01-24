using Eroad.Common.DTOs;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.DTOs
{
    public class RouteLookupResponse : BaseResponse
    {
        public List<RouteEntity> Routes { get; set; }
    }
}
