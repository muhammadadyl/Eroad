using Eroad.CQRS.Core.Messages;
using MediatR;

namespace Eroad.CQRS.Core.Commands
{
    public abstract record BaseCommand : Entity, IRequest
    {

    }
}