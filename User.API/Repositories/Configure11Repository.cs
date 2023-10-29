//======================================
// <copyright file="Configure11Repository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class Configure11Repository : Repository<Configure11>, IConfigure11Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure11Repository));
        private UserDbContext _db;
        public Configure11Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure11 in this._db.Configure11
                          where (Configure11.IsDeleted == 0)
                          select Configure11.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure11.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure11 in this._db.Configure11
                          orderby Configure11.Id descending
                          select Configure11.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure11NameById(int? configureId)
        {
            var result = await (from Configure11 in this._db.Configure11
                                where (Configure11.IsDeleted == Record.NotDeleted && Configure11.Id == configureId)
                                select Configure11.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure11>> GetAllConfiguration11(string search)
        {
            try
            {
                var result = (from configure11 in this._db.Configure11
                              where (configure11.Name.StartsWith(search) && configure11.IsDeleted == Record.NotDeleted)
                              select new Configure11
                              {
                                  Name = configure11.Name,
                                  Id = configure11.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure11>> GetConfiguration11()
        {
            try
            {
                var result = (from configure11 in this._db.Configure11
                              where (configure11.IsDeleted == Record.NotDeleted)
                              select new Configure11
                              {
                                  Name = configure11.Name,
                                  Id = configure11.Id

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
