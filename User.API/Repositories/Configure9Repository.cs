//======================================
// <copyright file="Configure9Repository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class Configure9Repository : Repository<Configure9>, IConfigure9Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure9Repository));

        private UserDbContext _db;
        public Configure9Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure9 in this._db.Configure9
                          where (Configure9.IsDeleted == 0)
                          select Configure9.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure9.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure9 in this._db.Configure9
                          orderby Configure9.Id descending
                          select Configure9.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure9NameById(int? configureId)
        {
            var result = await (from Configure9 in this._db.Configure9
                                where (Configure9.IsDeleted == Record.NotDeleted && Configure9.Id == configureId)
                                select Configure9.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure9>> GetAllConfiguration9(string search)
        {
            try
            {
                var result = (from configure9 in this._db.Configure9
                              where (configure9.Name.StartsWith(search) && configure9.IsDeleted == Record.NotDeleted)
                              select new Configure9
                              {
                                  Name = configure9.Name,
                                  Id = configure9.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return null;
        }

        public async Task<List<Configure9>> GetConfiguration9()
        {
            try
            {
                var result = (from configure9 in this._db.Configure9
                              where (configure9.IsDeleted == Record.NotDeleted)
                              select new Configure9
                              {
                                  Name = configure9.Name,
                                  Id = configure9.Id

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
