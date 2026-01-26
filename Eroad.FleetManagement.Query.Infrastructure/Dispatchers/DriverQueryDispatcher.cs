using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.Infrastructure.Dispatchers
{
    public class DriverQueryDispatcher : IQueryDispatcher<DriverEntity>
    {
        private readonly Dictionary<Type, Func<BaseQuery<DriverEntity>, Task<List<DriverEntity>>>> _handlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<DriverEntity>>> handler) where TQuery : BaseQuery<DriverEntity>
        {
            if (_handlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }

            _handlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public async Task<List<DriverEntity>> SendAsync(BaseQuery<DriverEntity> query)
        {
            if (_handlers.TryGetValue(query.GetType(), out Func<BaseQuery<DriverEntity>, Task<List<DriverEntity>>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}
