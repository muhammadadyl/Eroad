namespace Eroad.DeliveryTracking.Query.API.DTOs
{
    public class StatusLookupResponse
    {
        public string Message { get; set; }
        public List<StatusInfo> Statuses { get; set; }
    }
}
