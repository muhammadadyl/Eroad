using Eroad.Common.DTOs;

namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class VehicleStatusListResponse : BaseResponse
    {
        public required List<StatusInfo> Statuses { get; set; }
    }
}
