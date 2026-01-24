using Eroad.Common.DTOs;

namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class DriverStatusListResponse : BaseResponse
    {
        public List<StatusInfo> Statuses { get; set; }
    }
}
