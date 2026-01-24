using Eroad.Common.DTOs;
using Eroad.DeliveryTracking.Query.Domain.Entities;

namespace Eroad.DeliveryTracking.Query.API.DTOs
{
    public class DeliveryLookupResponse : BaseResponse
    {
        public List<DeliveryEntity> Deliveries { get; set; }
    }
}
