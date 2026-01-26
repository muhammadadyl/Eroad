using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.Infrastructure.Dispatchers
{
    public class RouteQueryDispatcher : IQueryDispatcher<RouteEntity>
    {
        private readonly Dictionary<Type, Func<BaseQuery<RouteEntity>, Task<List<RouteEntity>>>> _handlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<RouteEntity>>> handler) where TQuery : BaseQuery<RouteEntity>
        {
            if (_handlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }

            _handlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public async Task<List<RouteEntity>> SendAsync(BaseQuery<RouteEntity> query)
        {
            if (_handlers.TryGetValue(query.GetType(), out Func<BaseQuery<RouteEntity>, Task<List<RouteEntity>>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}
