using Eroad.Common.DTOs;
using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.DTOs
{
    public class IncidentLookupResponse : BaseResponse
    {
        public List<IncidentEntity> Incidents { get; set; }
    }
}
