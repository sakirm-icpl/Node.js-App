//======================================
// <copyright file="IRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace User.API.Repositories.Interfaces
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
