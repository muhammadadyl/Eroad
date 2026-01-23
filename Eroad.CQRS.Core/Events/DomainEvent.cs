using Eroad.CQRS.Core.Messages;

namespace Eroad.CQRS.Core.Events
{
    public record DomainEvent : Entity
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