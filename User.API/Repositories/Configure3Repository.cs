//======================================
// <copyright file="Configure3Repository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class Configure3Repository : Repository<Configure3>, IConfigure3Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure3Repository));
        private UserDbContext _db;
        public Configure3Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure3 in this._db.Configure3
                          where (Configure3.IsDeleted == 0)
                          select Configure3.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure3.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure3 in this._db.Configure3
                          orderby Configure3.Id descending
                          select Configure3.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }
        public async Task<string> GetConfigure3NameById(int? configureId)
        {
            var result = await (from Configure3 in this._db.Configure3
                                where (Configure3.IsDeleted == Record.NotDeleted && Configure3.Id == configureId)
                                select Configure3.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure3>> GetAllConfiguration3(string search)
        {
            try
            {
                var result = (from configure3 in this._db.Configure3
                              where (configure3.Name.StartsWith(search) && configure3.IsDeleted == Record.NotDeleted)
                              select new Configure3
                              {
                                  Name = configure3.Name,
                                  Id = configure3.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure3>> GetConfiguration3()
        {
            try
            {
                var result = (from configure3 in this._db.Configure3
                              where (configure3.IsDeleted == Record.NotDeleted)
                              select new Configure3
                              {
                                  Name = configure3.Name,
                                  Id = configure3.Id

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
