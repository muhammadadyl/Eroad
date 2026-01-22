namespace CQRS.Core.Messages
{
    public record EventBase
    {
        public Guid Id { get; set; }
    }
}