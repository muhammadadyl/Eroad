namespace Eroad.BFF.Gateway.Application.Models
{
    public class DeliveryEventLogViewModel
    {
        public Guid Id { get; set; }
        public Guid DeliveryId { get; set; }
        public string EventCategory { get; set; }
        public string EventType { get; set; }
        public string EventData { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
