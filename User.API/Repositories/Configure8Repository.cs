//======================================
// <copyright file="Configure8Repository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using log4net;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class Configure8Repository : Repository<Configure8>, IConfigure8Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure8Repository));

        private UserDbContext _db;
        public Configure8Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure8 in this._db.Configure8
                          where (Configure8.IsDeleted == 0)
                          select Configure8.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure8.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure8 in this._db.Configure8
                          orderby Configure8.Id descending
                          select Configure8.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }
        public async Task<string> GetConfigure8NameById(int? configureId)
        {
            var result = await (from Configure8 in this._db.Configure8
                                where (Configure8.IsDeleted == Record.NotDeleted && Configure8.Id == configureId)
                                select Configure8.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure8>> GetAllConfiguration8(string search)
        {
            try
            {
                var result = (from configure8 in this._db.Configure8
                              where (configure8.Name.StartsWith(search) && configure8.IsDeleted == Record.NotDeleted)
                              select new Configure8
                              {
                                  Name = configure8.Name,
                                  Id = configure8.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure8>> GetConfiguration8()
        {
            try
            {
                var result = (from configure8 in this._db.Configure8
                              where (configure8.IsDeleted == Record.NotDeleted)
                              select new Configure8
                              {
                                  Name = configure8.Name,
                                  Id = configure8.Id

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
