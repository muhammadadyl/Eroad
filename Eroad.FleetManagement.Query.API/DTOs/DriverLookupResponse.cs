using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class DriverLookupResponse
    {
        public string Message { get; set; }
        public required List<DriverEntity> Drivers { get; set; }
    }
}
