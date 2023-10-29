//======================================
// <copyright file="Configure2Repository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class Configure2Repository : Repository<Configure2>, IConfigure2Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure2Repository));
        private UserDbContext _db;
        public Configure2Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure2 in this._db.Configure2
                          where (Configure2.IsDeleted == 0)
                          select Configure2.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure2
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure2 in this._db.Configure2
                          orderby Configure2.Id descending
                          select Configure2.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure2NameById(int? configureId)
        {
            var result = await (from Configure2 in this._db.Configure2
                                where (Configure2.IsDeleted == Record.NotDeleted && Configure2.Id == configureId)
                                select Configure2.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure2>> GetAllConfiguration2(string search)
        {
            try
            {
                var result = (from configure2 in this._db.Configure2
                              where (configure2.Name.StartsWith(search) && configure2.IsDeleted == Record.NotDeleted)
                              select new Configure2
                              {
                                  Name = configure2.Name,
                                  Id = configure2.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure2>> GetConfiguration2()
        {
            try
            {
                var result = (from configure2 in this._db.Configure2
                              where (configure2.IsDeleted == Record.NotDeleted)
                              select new Configure2
                              {
                                  Name = configure2.Name,
                                  Id = configure2.Id

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
