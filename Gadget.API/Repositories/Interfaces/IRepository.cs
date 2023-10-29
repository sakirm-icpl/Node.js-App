using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task Add(TEntity entity);
        Task Update(TEntity entity);
        Task Remove(TEntity entity);
        Task<int> Count();
        Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> Get(int id);
        Task<TEntity> Get(Expression<Func<TEntity, bool>> match);
        Task<List<TEntity>> GetAll();
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> match);
        Task AddRange(IEnumerable<TEntity> entities);
        Task UpdateRange(IEnumerable<TEntity> entities);
        Task RemoveRange(IEnumerable<TEntity> entities);

    }
}
