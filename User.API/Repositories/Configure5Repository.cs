//======================================
// <copyright file="Configure5Repository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class Configure5Repository : Repository<Configure5>, IConfigure5Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure5Repository));
        private UserDbContext _db;
        public Configure5Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure5 in this._db.Configure5
                          where (Configure5.IsDeleted == 0)
                          select Configure5.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure5.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure5 in this._db.Configure5
                          orderby Configure5.Id descending
                          select Configure5.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure5NameById(int? configureId)
        {
            var result = await (from Configure5 in this._db.Configure5
                                where (Configure5.IsDeleted == Record.NotDeleted && Configure5.Id == configureId)
                                select Configure5.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure5>> GetAllConfiguration5(string search)
        {
            try
            {
                var result = (from configure5 in this._db.Configure5
                              where (configure5.Name.StartsWith(search) && configure5.IsDeleted == Record.NotDeleted)
                              select new Configure5
                              {
                                  Name = configure5.Name,
                                  Id = configure5.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure5>> GetConfiguration5()
        {
            try
            {
                var result = (from configure5 in this._db.Configure5
                              where (configure5.IsDeleted == Record.NotDeleted)
                              select new Configure5
                              {
                                  Name = configure5.Name,
                                  Id = configure5.Id

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
