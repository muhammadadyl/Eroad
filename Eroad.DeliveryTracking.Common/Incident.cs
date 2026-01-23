namespace Eroad.DeliveryTracking.Common
{
    public class Incident
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Resolved { get; set; }

        public Incident() { }

        public Incident(string type, string description, DateTime timestamp, bool resolved = false)
        {
            Type = type;
            Description = description;
            Timestamp = timestamp;
            Resolved = resolved;
        }
    }
}
