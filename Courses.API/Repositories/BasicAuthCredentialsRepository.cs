using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Repositories
{
    public class BasicAuthCredentialsRepository: Repository<BasicAuthCredentials>, IBasicAuthRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BasicAuthCredentialsRepository));
        private CourseContext _db;
        public BasicAuthCredentialsRepository(CourseContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<BasicAuthCredentials> Authenticate(string userName, string password)
        {
            try
            {
                var result = await this._db.BasicAuthCredentials.Where(x => x.UserName == userName && x.Password == password).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
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
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
    }
}
