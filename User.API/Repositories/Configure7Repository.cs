//======================================
// <copyright file="Configure7Repository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.EntityFrameworkCore;
using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class Configure7Repository : Repository<Configure7>, IConfigure7Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure7Repository));
        private UserDbContext _db;
        public Configure7Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure7 in this._db.Configure7
                          where (Configure7.IsDeleted == 0)
                          select Configure7.Name).ToListAsync(); 
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure7.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure7 in this._db.Configure7
                          orderby Configure7.Id descending
                          select Configure7.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure7NameById(int? configureId)
        {
            var result = await (from Configure7 in this._db.Configure7
                                where (Configure7.IsDeleted == Record.NotDeleted && Configure7.Id == configureId)
                                select Configure7.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }

        public async Task<IEnumerable<Configure7>> GetAllConfiguration7(string search)
        {
            try
            {
                var result = (from configure7 in this._db.Configure7
                              where (configure7.Name.StartsWith(search) && configure7.IsDeleted == Record.NotDeleted)
                              select new Configure7
                              {
                                  Name = configure7.Name,
                                  Id = configure7.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async Task<List<Configure7>> GetConfiguration7()
        {
            try
            {
                var result = (from configure7 in this._db.Configure7
                              where (configure7.IsDeleted == Record.NotDeleted)
                              select new Configure7
                              {
                                  Name = configure7.Name,
                                  Id = configure7.Id

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
