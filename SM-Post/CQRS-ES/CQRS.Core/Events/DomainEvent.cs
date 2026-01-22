using CQRS.Core.Messages;

namespace CQRS.Core.Events
{
    public record DomainEvent : EventBase
    {
        protected DomainEvent(string type)
        {
            Type = type;
        }

        public string Type { get; set; }
        public int Version { get; set; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}