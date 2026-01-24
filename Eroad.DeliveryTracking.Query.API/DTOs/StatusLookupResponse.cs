using Eroad.Common.DTOs;

namespace Eroad.DeliveryTracking.Query.API.DTOs
{
    public class StatusLookupResponse : BaseResponse
    {
        public List<StatusInfo> Statuses { get; set; }
    }
}
