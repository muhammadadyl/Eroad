using Eroad.Common.DTOs;

namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class VehicleStatusListResponse : BaseResponse
    {
        public List<StatusInfo> Statuses { get; set; }
    }
}
