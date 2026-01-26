using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.Infrastructure.Dispatchers
{
    public class VehicleQueryDispatcher : IQueryDispatcher<VehicleEntity>
    {
        private readonly Dictionary<Type, Func<BaseQuery<VehicleEntity>, Task<List<VehicleEntity>>>> _handlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<VehicleEntity>>> handler) where TQuery : BaseQuery<VehicleEntity>
        {
            if (_handlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }

            _handlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public async Task<List<VehicleEntity>> SendAsync(BaseQuery<VehicleEntity> query)
        {
            if (_handlers.TryGetValue(query.GetType(), out Func<BaseQuery<VehicleEntity>, Task<List<VehicleEntity>>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}
