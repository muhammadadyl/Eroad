using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.DTOs
{
    public class IncidentLookupResponse
    {
        public string Message { get; set; }
        public required List<IncidentEntity> Incidents { get; set; }
    }
}
