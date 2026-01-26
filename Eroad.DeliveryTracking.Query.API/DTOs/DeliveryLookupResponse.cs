using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.DTOs
{
    public class DeliveryLookupResponse
    {
        public string Message { get; set; }
        public required List<DeliveryEntity> Deliveries { get; set; }
    }
}
