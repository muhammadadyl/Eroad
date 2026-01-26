using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery.Handlers
{
    public class ReportIncidentCommandHandler : IRequestHandler<ReportIncidentCommand>
    {
        private readonly IEventSourcingHandler<DeliveryAggregate> _eventSourcingHandler;

        public ReportIncidentCommandHandler(IEventSourcingHandler<DeliveryAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(ReportIncidentCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Delivery aggregate with ID {request.Id} not found.");
            }

            aggregate.ReportIncident(request.Incident);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
