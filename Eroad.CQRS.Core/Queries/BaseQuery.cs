using MediatR;

namespace Eroad.CQRS.Core.Queries
{
    public abstract class BaseQuery<TResponse> : IRequest<List<TResponse>>
    {

    }
}