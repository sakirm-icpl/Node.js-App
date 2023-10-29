//======================================
// <copyright file="Configure4Repository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class Configure4Repository : Repository<Configure4>, IConfigure4Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure4Repository));
        private UserDbContext _db;
        public Configure4Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure4 in this._db.Configure4
                          where (Configure4.IsDeleted == 0)
                          select Configure4.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure4.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure4 in this._db.Configure4
                          orderby Configure4.Id descending
                          select Configure4.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure4NameById(int? configureId)
        {
            var result = await (from Configure4 in this._db.Configure4
                                where (Configure4.IsDeleted == Record.NotDeleted && Configure4.Id == configureId)
                                select Configure4.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }
        public async Task<IEnumerable<Configure4>> GetAllConfiguration4(string search)
        {
            try
            {
                var result = (from configure4 in this._db.Configure4
                              where (configure4.Name.StartsWith(search) && configure4.IsDeleted == Record.NotDeleted)
                              select new Configure4
                              {
                                  Name = configure4.Name,
                                  Id = configure4.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure4>> GetConfiguration4()
        {
            try
            {
                var result = (from configure4 in this._db.Configure4
                              where (configure4.IsDeleted == Record.NotDeleted)
                              select new Configure4
                              {
                                  Name = configure4.Name,
                                  Id = configure4.Id

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
