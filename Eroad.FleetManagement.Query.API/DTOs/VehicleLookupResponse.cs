using Eroad.Common.DTOs;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class VehicleLookupResponse : BaseResponse
    {
        public List<VehicleEntity> Vehicles { get; set; }
    }
}
