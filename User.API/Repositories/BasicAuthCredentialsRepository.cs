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
    public class BasicAuthCredentialsRepository : Repository<BasicAuthCredentials>, IBasicAuthRepository
    {
        //private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure11Repository));
        private UserDbContext _db;
        public BasicAuthCredentialsRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<BasicAuthCredentials> Authenticate(string userName, string password)      
        {
            try
            {
                var result = await this._db.BasicAuthCredentials.Where(x => x.UserName == userName && x.Password==password).FirstOrDefaultAsync();
                 return  result;
            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<BasicAuthCredentials> AuthenticateApiToken(string apiToken)
        {
            try
            {
                var result = await this._db.BasicAuthCredentials.Where(x => x.ApiToken == apiToken).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
               // _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

    }
}
