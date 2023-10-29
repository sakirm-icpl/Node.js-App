//======================================
// <copyright file="Configure6Repository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Saml.API.Data;
using Saml.API.Helper;
using Saml.API.Models;
using Saml.API.Repositories.Interfaces;

namespace Saml.API.Repositories
{
    public class Configure6Repository : Repository<Configure6>, IConfigure6Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure6Repository));

        private UserDbContext _db;
        public Configure6Repository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from Configure6 in this._db.Configure6
                          where (Configure6.IsDeleted == 0)
                          select Configure6.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure6.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Configure6 in this._db.Configure6
                          orderby Configure6.Id descending
                          select Configure6.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure6NameById(int? configureId)
        {
            var result = await (from Configure6 in this._db.Configure6
                                where (Configure6.IsDeleted == Record.NotDeleted && Configure6.Id == configureId)
                                select Configure6.Name).AsNoTracking().SingleOrDefaultAsync();
            return result;
        }

        public async Task<IEnumerable<Configure6>> GetAllConfiguration6(string search)
        {
            try
            {
                var result = (from Configure6 in this._db.Configure6
                              where (Configure6.Name.StartsWith(search) && Configure6.IsDeleted == Record.NotDeleted)
                              select new Configure6
                              {
                                  Name = Configure6.Name,
                                  Id = Configure6.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure6>> GetConfiguration6()
        {
            try
            {
                var result = (from Configure6 in this._db.Configure6
                              where (Configure6.IsDeleted == Record.NotDeleted)
                              select new Configure6
                              {
                                  Name = Configure6.Name,
                                  Id = Configure6.Id

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
