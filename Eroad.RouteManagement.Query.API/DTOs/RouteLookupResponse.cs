using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.DTOs
{
    public class RouteLookupResponse
    {
        public string Message { get; set; }
        public required List<RouteEntity> Routes { get; set; }
    }
}
