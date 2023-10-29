//======================================
// <copyright file="Configure12Repository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class Configure12Repository : Repository<Configure12>, IConfigure12Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure12Repository));
        private UserDbContext _db;
        public Configure12Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure12 in this._db.Configure12
                          where (Configure12.IsDeleted == 0)
                          select Configure12.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure12.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure12 in this._db.Configure12
                          orderby Configure12.Id descending
                          select Configure12.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }
        public async Task<string> GetConfigure12NameById(int? configureId)
        {
            var result = await (from Configure12 in this._db.Configure12
                                where (Configure12.IsDeleted == Record.NotDeleted && Configure12.Id == configureId)
                                select Configure12.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure12>> GetAllConfiguration12(string search)
        {
            try
            {
                var result = (from configure12 in this._db.Configure12
                              where (configure12.Name.StartsWith(search) && configure12.IsDeleted == Record.NotDeleted)
                              select new Configure12
                              {
                                  Name = configure12.Name,
                                  Id = configure12.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure12>> GetConfiguration12()
        {
            try
            {
                var result = (from configure12 in this._db.Configure12
                              where (configure12.IsDeleted == Record.NotDeleted)
                              select new Configure12
                              {
                                  Name = configure12.Name,
                                  Id = configure12.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
    }
}
