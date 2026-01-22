using CQRS.Core.Events;

namespace CQRS.Core.Domain
{
    public abstract class EventAggregateRoot
    {
        protected Guid _id;
        private readonly List<DomainEvent> _changes = new();

        public Guid Id
        {
            get { return _id; }
        }

        public int Version { get; set; } = -1;

        public IEnumerable<DomainEvent> GetUncommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        private void ApplyChange(DomainEvent @event, bool isNew)
        {
            var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method), $"The Apply method was not found in the aggregate for {@event.GetType().Name}!");
            }

            method.Invoke(this, new object[] { @event });

            if (isNew)
            {
                _changes.Add(@event);
            }
        }

        protected void RaiseEvent(DomainEvent @event)
        {
            ApplyChange(@event, true);
        }

        public void ReplayEvents(IEnumerable<DomainEvent> events)
        {
            foreach (var @event in events)
            {
                ApplyChange(@event, false);
            }
        }
    }
}