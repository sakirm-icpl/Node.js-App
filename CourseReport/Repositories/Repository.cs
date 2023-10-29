using Microsoft.EntityFrameworkCore;
using CourseReport.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CourseReport.API.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected DbContext _context;
        protected DbSet<TEntity> _entities;

        public Repository(DbContext context)
        {
            _context = context;
            _entities = context.Set<TEntity>();
        }

        public virtual async Task Add(TEntity entity)
        {
            _entities.Add(entity);
            await _context.SaveChangesAsync();

        }

        public virtual async Task AddRange(IEnumerable<TEntity> entities)
        {
            _entities.AddRange(entities);
            await _context.SaveChangesAsync();
        }


        public virtual async Task Update(TEntity entity)
        {

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

        }

        public virtual async Task UpdateRange(IEnumerable<TEntity> entities)
        {
            _entities.UpdateRange(entities);
            await _context.SaveChangesAsync();

        }



        public virtual async Task Remove(TEntity entity)
        {
            _entities.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            _entities.RemoveRange(entities);
        }


        public virtual int Count()
        {
            return _entities.Count();
        }


        public virtual async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return  _entities.Where(predicate);
        }

        public virtual async Task<TEntity> GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<TEntity> Get(int id)
        {
            TEntity entity = await _entities.FindAsync(id);
            if (entity != null)
                this._context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _entities.ToListAsync();
        }
        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> match)
        {
            return await this._entities.Where(match).ToListAsync();
        }
        public virtual async Task<TEntity> Get(Expression<Func<TEntity, bool>> match)
        {
            return await this._entities.SingleOrDefaultAsync(match);
        }
        public virtual async Task AddRecord(TEntity entity)
        {
            this._context.Entry(entity).State = EntityState.Added;
            await this._context.SaveChangesAsync();
        }
        public virtual async Task UpdateRecord(TEntity entity)
        {
            this._context.Entry(entity).State = EntityState.Modified;
            await this._context.SaveChangesAsync();
        }
        public virtual async Task<TEntity> GetRecord(int id)
        {
            Task<TEntity> entity = this._entities.FindAsync(id).AsTask();
            this._context.Entry(entity.Result).State = EntityState.Detached;
            return await entity;
        }
        public virtual async Task AddRangeRecord(IEnumerable<TEntity> entities)
        {
            await _entities.AddRangeAsync(entities);
            await this._context.SaveChangesAsync();
        }

        public virtual async Task UpdateRangeRecord(IEnumerable<TEntity> entities)
        {
            _entities.UpdateRange(entities);
            await this._context.SaveChangesAsync();
        }
    }
}
