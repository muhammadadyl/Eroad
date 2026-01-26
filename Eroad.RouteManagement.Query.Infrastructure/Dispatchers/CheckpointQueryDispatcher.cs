using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.Infrastructure.Dispatchers
{
    public class CheckpointQueryDispatcher : IQueryDispatcher<CheckpointEntity>
    {
        private readonly Dictionary<Type, Func<BaseQuery<CheckpointEntity>, Task<List<CheckpointEntity>>>> _handlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<CheckpointEntity>>> handler) where TQuery : BaseQuery<CheckpointEntity>
        {
            if (_handlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }

            _handlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public async Task<List<CheckpointEntity>> SendAsync(BaseQuery<CheckpointEntity> query)
        {
            if (_handlers.TryGetValue(query.GetType(), out Func<BaseQuery<CheckpointEntity>, Task<List<CheckpointEntity>>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}
