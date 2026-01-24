namespace Eroad.DeliveryTracking.Common
{
    public class Incident
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime ReportedTimestamp { get; set; }
        public DateTime? ResolvedTimestamp { get; set; }
        public bool Resolved { get; set; }

        public Incident() { }

        public Incident(Guid id, string type, string description, DateTime reportedTimestamp, bool resolved = false, DateTime? resolvedTimestamp = null)
        {
            Id = id;
            Type = type;
            Description = description;
            ReportedTimestamp = reportedTimestamp;
            Resolved = resolved;
            ResolvedTimestamp = resolvedTimestamp;
        }
    }
}
