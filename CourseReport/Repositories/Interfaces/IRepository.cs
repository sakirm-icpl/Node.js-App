using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CourseReport.API.Repositories.Interface
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task Add(TEntity entity);
        Task AddRange(IEnumerable<TEntity> entities);
        Task Update(TEntity entity);
        Task UpdateRange(IEnumerable<TEntity> entities);
        Task Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        int Count();
        Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> Get(int id);
        Task<IEnumerable<TEntity>> GetAll();
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> match);
        Task<TEntity> Get(Expression<Func<TEntity, bool>> match);
        Task AddRecord(TEntity entity);
        Task UpdateRecord(TEntity entity);
        Task<TEntity> GetRecord(int id);
        Task AddRangeRecord(IEnumerable<TEntity> entities);
        Task UpdateRangeRecord(IEnumerable<TEntity> entities);
    }
}
