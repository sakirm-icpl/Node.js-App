//======================================
// <copyright file="Configure10Repository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class Configure10Repository : Repository<Configure10>, IConfigure10Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure10Repository));
        private UserDbContext _db;
        public Configure10Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure10 in this._db.Configure10
                          where (Configure10.IsDeleted == 0)
                          select Configure10.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure10.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure10 in this._db.Configure10
                          orderby Configure10.Id descending
                          select Configure10.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }
        public async Task<string> GetConfigure10NameById(int? configureId)
        {
            var result = await (from Configure10 in this._db.Configure10
                                where (Configure10.IsDeleted == Record.NotDeleted && Configure10.Id == configureId)
                                select Configure10.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure10>> GetAllConfiguration10(string search)
        {
            try
            {
                var result = (from configure10 in this._db.Configure10
                              where (configure10.Name.StartsWith(search) && configure10.IsDeleted == Record.NotDeleted)
                              select new Configure10
                              {
                                  Name = configure10.Name,
                                  Id = configure10.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure10>> GetConfiguration10()
        {
            try
            {
                var result = (from configure10 in this._db.Configure10
                              where (configure10.IsDeleted == Record.NotDeleted)
                              select new Configure10
                              {
                                  Name = configure10.Name,
                                  Id = configure10.Id

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
