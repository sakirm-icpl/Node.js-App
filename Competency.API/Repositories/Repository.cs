using Competency.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using log4net;
using Competency.API.Helper;
namespace Competency.API.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Repository<TEntity>));
        protected DbContext _context;
        protected DbSet<TEntity> _entities;
        DbSet<TEntity> _set;

        public string EagerLoading { get; set; }

        public Repository(DbContext context)
        {
            _context = context;
            _set = context.Set<TEntity>();
            _entities = context.Set<TEntity>();
        }

        public async virtual Task Add(TEntity entity)
        {
            try
            {
                _entities.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            { _logger.Error(Utilities.GetDetailedException(ex));
            }

        }

        public async virtual Task AddRange(IEnumerable<TEntity> entities)
        {
            _entities.AddRange(entities);
            await _context.SaveChangesAsync();
        }


        public async virtual Task Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async virtual Task UpdateRange(IEnumerable<TEntity> entities)
        {
            _entities.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }



        public async virtual Task Remove(TEntity entity)
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

        private IOrderedQueryable<TEntity> OrderBy(IQueryable<TEntity> source, string propertyName, bool descending = false, bool anotherLevel = false)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
            MemberExpression property = Expression.PropertyOrField(parameter, propertyName);
            LambdaExpression sort = Expression.Lambda(property, parameter);
            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(TEntity), property.Type },
                source.Expression,
                Expression.Quote(sort));
            return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(call);
        }


        public async virtual Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)

        {
            return _entities.Where(predicate);
        }

        public async virtual Task<TEntity> GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.SingleOrDefaultAsync(predicate);
        }

        public async virtual Task<TEntity> Get(int id)
        {
            var entity = await _entities.FindAsync(id);
            if (entity != null)
                this._context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async virtual Task<IEnumerable<TEntity>> GetAll()
        {
            return await _entities.AsNoTracking().ToListAsync();
        }
        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> match)
        {
            return await this._entities.Where(match).ToListAsync();
        }
        public async virtual Task<TEntity> Get(Expression<Func<TEntity, bool>> match)
        {
            return await this._entities.SingleOrDefaultAsync(match);
        }
        public async virtual Task AddRecord(TEntity entity)
        {
            this._context.Entry(entity).State = EntityState.Added;
            await this._context.SaveChangesAsync();
        }
        public async virtual Task UpdateRecord(TEntity entity)
        {
            this._context.Entry(entity).State = EntityState.Modified;
            await this._context.SaveChangesAsync();
        }
        public async virtual Task<TEntity> GetRecord(int id)
        {
            var entity = this._entities.FindAsync(id);
            this._context.Entry(entity.Result).State = EntityState.Detached;
            return await entity;
        }
        public async virtual Task AddRangeRecord(IEnumerable<TEntity> entities)
        {
            await _entities.AddRangeAsync(entities);
            await this._context.SaveChangesAsync();
        }

        public async virtual Task UpdateRangeRecord(IEnumerable<TEntity> entities)
        {
            _entities.UpdateRange(entities);
            await this._context.SaveChangesAsync();
        }

        public virtual IEnumerable<TEntity> GetAll(int skip, int count, string orderBy, Order? order)
        {
            skip = (skip - 1) * count;
            if (orderBy == "" || orderBy == null)
            {
                if (EagerLoading != null) return _set.Skip(skip).Take(count).Include(EagerLoading).ToList();
                else return _set.Skip(skip).Take(count).ToList();
            }
            else
            {
                if (EagerLoading != null) return OrderBy(_set, orderBy, (order == null || order == Order.ASC) ? false : true).Skip(skip).Take(count).Include(EagerLoading).ToList();
                else return OrderBy(_set, orderBy, (order == null || order == Order.ASC) ? false : true).Skip(skip).Take(count).ToList();
            }
        }

        public virtual IEnumerable<TEntity> GetAllAsync(int skip, int count, string orderBy, Order? order)
        {
            skip = (skip - 1) * count;

            if (orderBy == "" || orderBy == null)
            {
                if (EagerLoading != null)
                    return _set.Skip(skip).Take(count).Include(EagerLoading).ToList();
                else return _set.Skip(skip).Take(count).ToList();
            }
            else
            {
                if (EagerLoading != null) return OrderBy(_set, orderBy, (order == null || order == Order.ASC) ? false : true).Skip(skip).Take(count).Include(EagerLoading).ToList();
                else
                   // return OrderBy(_set, orderBy, (order) ? false : true).Skip(skip).Take(count).ToList();
                   return OrderBy(_set, orderBy, (order == null || order == Order.ASC) ? false : true).Skip(skip).Take(count).ToList();
            }
        }
        public virtual async Task<IEnumerable<TEntity>> FindByFieldAsync(string fieldName, string fieldValue, int skip, int count, string cmpOperator, string orderBy, Order? order)
        {
            if (orderBy == "" || orderBy == null)
            {
                if (EagerLoading != null) return await _set.Where(MakeEqualityExpression(fieldName, fieldValue, cmpOperator)).Skip(skip).Take(count).Include(EagerLoading).ToListAsync();
                else return await _set.Where(MakeEqualityExpression(fieldName, fieldValue, cmpOperator)).Skip(skip).Take(count).ToListAsync();
            }
            else
            {
                if (EagerLoading != null) return await OrderBy(_set.Where(MakeEqualityExpression(fieldName, fieldValue, cmpOperator)), orderBy, (order == null || order == Order.ASC) ? false : true).Skip(skip).Take(count).Include(EagerLoading).ToListAsync();
                else return await OrderBy(_set.Where(MakeEqualityExpression(fieldName, fieldValue, cmpOperator)), orderBy, (order == null || order == Order.ASC) ? false : true).Skip(skip).Take(count).ToListAsync();
            }
        }
        public virtual Task<int> CountAsync()
        {
            return _set.CountAsync();
        }

        public virtual Task<int> CountAsync(string fieldName, string fieldValue)
        {
            return _set.CountAsync(MakeEqualityExpression(fieldName, fieldValue));
        }
        Expression<Func<TEntity, bool>> MakeEqualityExpression(string fieldName, Object fieldValue, string cmpOperator = "")
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "entity");
            MemberExpression property = Expression.Property(parameter, fieldName);

            if (cmpOperator == "contains")
            {
                var stringContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var xpr = Expression.Call(property, stringContainsMethod, Expression.Constant(fieldValue, typeof(string)));
                return Expression.Lambda<Func<TEntity, bool>>(xpr, new[] { parameter });
            }

            var value = Expression.Constant(fieldValue);
            var equality = Expression.Equal(property, value);

            if (cmpOperator == "<") equality = Expression.LessThan(property, value);
            else if (cmpOperator == ">") equality = Expression.GreaterThan(property, value);
            else if (cmpOperator == "<=") equality = Expression.LessThanOrEqual(property, value);
            else if (cmpOperator == ">=") equality = Expression.GreaterThanOrEqual(property, value);
            else if (cmpOperator == "!=") equality = Expression.NotEqual(property, value);

            return Expression.Lambda<Func<TEntity, bool>>(equality, new[] { parameter });
        }
    }
}
