using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Commands.Driver.Handlers
{
    public class ChangeDriverStatusCommandHandler : IRequestHandler<ChangeDriverStatusCommand>
    {
        private readonly IEventSourcingHandler<DriverAggregate> _eventSourcingHandler;

        public ChangeDriverStatusCommandHandler(IEventSourcingHandler<DriverAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(ChangeDriverStatusCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Driver aggregate with ID {request.Id} not found.");
            }

            aggregate.ChangeDriverStatus(request.OldStatus, request.NewStatus);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
