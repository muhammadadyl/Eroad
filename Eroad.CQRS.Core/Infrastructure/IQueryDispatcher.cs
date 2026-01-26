using Eroad.CQRS.Core.Queries;

namespace Eroad.CQRS.Core.Infrastructure
{
    public interface IQueryDispatcher<TEntity>
    {
        void RegisterHandler<TQuery>(Func<TQuery, Task<List<TEntity>>> handler) where TQuery : BaseQuery<TEntity>;
        Task<List<TEntity>> SendAsync(BaseQuery<TEntity> query);
    }
}