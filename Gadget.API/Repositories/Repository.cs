//======================================
// <copyright file="Repository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Gadget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Gadget.API.Helper;
using log4net;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gadget.API.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Repository<TEntity>));
        protected DbContext _context;
        protected DbSet<TEntity> _entities;

        public Repository(DbContext context)
        {
            this._context = context;
            this._entities = context.Set<TEntity>();
        }

        public virtual async Task Add(TEntity entity)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = this._context.Database.BeginTransaction())
            {
                try
                {
                    this._context.Entry(entity).State = EntityState.Added;
                    await this._context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error( Utilities.GetDetailedException(ex));
                    Console.WriteLine(ex);
                }
            }
        }

        public virtual async Task Update(TEntity entity)
        {
            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = this._context.Database.BeginTransaction())
            {
                try
                {
                    this._context.Entry(entity).State = EntityState.Modified;
                    await this._context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error( Utilities.GetDetailedException(ex));
                    string e = ex.Message;
                }
            }
        }

        public virtual async Task Remove(TEntity entity)
        {
            this._entities.Remove(entity);
            await this._context.SaveChangesAsync();
        }

        public virtual Task<int> Count()
        {
            return this._entities.CountAsync();
        }


        public virtual async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return await this._entities.Where(predicate).ToListAsync();
        }

        public virtual async Task<TEntity> GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return await this._entities.SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<TEntity> Get(Expression<Func<TEntity, bool>> match)
        {

            try
            {
                return await this._entities.SingleOrDefaultAsync(match);
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string e = ex.Message;
            }
            return null;

        }
        public virtual async Task<TEntity> Get(int id)
        {
            try
            {
                Task<TEntity> entity = _entities.FindAsync(id).AsTask();
                this._context.Entry(entity.Result).State = EntityState.Detached;
                return await entity;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string e = ex.Message;
            }
            return null;
        }
        public virtual async Task<List<TEntity>> GetAll()
        {
            try
            {
                return await this._entities.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string e = ex.Message;
            }
            return null;
        }
        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> match)
        {
            return await this._entities.Where(match).ToListAsync();
        }
        public virtual async Task AddRange(IEnumerable<TEntity> entities)
        {
            try
            {
                await _entities.AddRangeAsync(entities);
                await this._context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                Console.WriteLine(ex);
            }
        }
        public virtual async Task UpdateRange(IEnumerable<TEntity> entities)
        {
            try
            {
                _entities.UpdateRange(entities);
                await this._context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                Console.WriteLine(ex);
            }
        }

        public virtual async Task RemoveRange(IEnumerable<TEntity> entities)
        {
            try
            {
                _entities.RemoveRange(entities);
                await this._context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                Console.WriteLine(ex);
            }
        }


    }
}
