using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class VehicleLookupResponse
    {
        public string Message { get; set; }
        public required List<VehicleEntity> Vehicles { get; set; }
    }
}
