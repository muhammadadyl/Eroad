using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.Infrastructure.Dispatchers
{
    public class VehicleQueryDispatcher : IQueryDispatcher<VehicleEntity>
    {
        private readonly Dictionary<Type, Func<BaseQuery, Task<List<VehicleEntity>>>> _vehicleHandlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<VehicleEntity>>> handler) where TQuery : BaseQuery
        {
            if (_vehicleHandlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }

            _vehicleHandlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public async Task<List<VehicleEntity>> SendAsync(BaseQuery query)
        {
            if (_vehicleHandlers.TryGetValue(query.GetType(), out Func<BaseQuery, Task<List<VehicleEntity>>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}
