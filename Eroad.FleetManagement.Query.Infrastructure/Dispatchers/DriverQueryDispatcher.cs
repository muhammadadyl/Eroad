using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.Infrastructure.Dispatchers
{
    public class DriverQueryDispatcher : IQueryDispatcher<DriverEntity>
    {
        private readonly Dictionary<Type, Func<BaseQuery, Task<List<DriverEntity>>>> _driverHandlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<DriverEntity>>> handler) where TQuery : BaseQuery
        {
            if (_driverHandlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }

            _driverHandlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public async Task<List<DriverEntity>> SendAsync(BaseQuery query)
        {
            if (_driverHandlers.TryGetValue(query.GetType(), out Func<BaseQuery, Task<List<DriverEntity>>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}
