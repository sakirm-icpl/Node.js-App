using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;
using static User.API.Helper.EnumHelper;

namespace User.API.Repositories
{
    public class CustomerConnectionStringRepository : Repository<CustomerConnectionString>, ICustomerConnectionStringRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CustomerConnectionStringRepository));
        private UserDbContext _db;
        private IConfiguration _configuration;
        private IHttpContextAccessor _httpContext;
        public CustomerConnectionStringRepository(UserDbContext context, IConfiguration configuration, IHttpContextAccessor httpContext) : base(context)
        {
            this._db = context;
            this._configuration = configuration;
            this._httpContext = httpContext;
        }
        public void SetDbContext(UserDbContext db)
        {
            this._db = db;
        }
        public async Task<string> GetConnectionString(string clientId)
        {
            try
            {
                var result = (from cc in this._db.CustomerConnectionString
                              where String.Equals(cc.Code, clientId, StringComparison.CurrentCultureIgnoreCase)
                              select cc.ConnectionString);

                return await result.AsNoTracking().SingleAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return string.Empty;
        }
        public async Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode)
        {
            _logger.Debug("In Function GetConnectionStringByOrgnizationCode to fetch connectionstring for Org :-" + OrgnizationCode);
            if (string.IsNullOrEmpty(OrgnizationCode))
            {
                return ApiStatusCode.Unauthorized.ToString();
            }
            string OrgnizationConnectionString = null;
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultMasterConnection"))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            _logger.Debug("configuration.GetConnectionString(DefaultMasterConnection :-" + this._configuration.GetConnectionString("DefaultMasterConnection"));
            using (var context = new UserDbContext(optionsBuilder.Options))
            {

                if (context == null)
                {
                    return ApiStatusCode.BadRequest.ToString();
                }
               
                OrgnizationConnectionString = await GetConnectionStringAsync(OrgnizationCode);
                if (OrgnizationConnectionString == null)
                    return ApiStatusCode.Unauthorized.ToString();
            }
            optionsBuilder.UseSqlServer(OrgnizationConnectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            using (var context = new UserDbContext(optionsBuilder.Options))
            {
                optionsBuilder.UseSqlServer(OrgnizationConnectionString)
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                if (context == null)
                {
                    return ApiStatusCode.BadRequest.ToString();
                }
            }
            return OrgnizationConnectionString;
        }

        public async Task<String> GetConnectionStringAsync(string orgcode)
        {
            List<APIModel.ConnectionInfo> clientConnectionStrings = null;
            try
            {
                var cache = new CacheManager.CacheManager();

                if (cache.IsAdded(Constants.CacheKeyNames.CONNECTION_STRINGS))
                {
                    _logger.Debug("Getting Connection String Value for client :-" + orgcode + " from cache.");
                    clientConnectionStrings = cache.Get<List<User.API.APIModel.ConnectionInfo>>(Constants.CacheKeyNames.CONNECTION_STRINGS);
                }
                else
                {
                   _logger.Debug("Getting Connection String Value for client :-" + orgcode + " from database.");
                
                    #region "Fetch Connection Strings from Database"
                    var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
                    optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultMasterConnection"))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    using (var context = new UserDbContext(optionsBuilder.Options))
                    {
                        if (context == null)
                        {
                            return ApiStatusCode.BadRequest.ToString();
                        }
                        clientConnectionStrings = await (from cc in context.CustomerConnectionString
                                                         select new User.API.APIModel.ConnectionInfo { ConnectionString = cc.ConnectionString, ClientCode = cc.DatabaseCode }).AsNoTracking().ToListAsync();
                    }
                    #endregion
                
                    _logger.Debug("Connection strings count  :-" + clientConnectionStrings.Count);
                    cache.Add(Constants.CacheKeyNames.CONNECTION_STRINGS, clientConnectionStrings, System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                }
                return clientConnectionStrings.Where(A => A.ClientCode.ToUpper() == orgcode.ToUpper()).Select(A => A.ConnectionString).FirstOrDefault();
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));                
                _logger.Error(string.Format("Exception while getting connection string value for client {0}. Exception :- {1} :-", orgcode, ex.Message)); }
            return null;
        }

        public UserDbContext GetDbContext()
        {
            string ConnectionString = null;
            if (this._httpContext != null)
            {
                var httpContext = this._httpContext.HttpContext;
                string encryptedConnectionString = httpContext.User.FindFirst("address") == null ? null : httpContext.User.FindFirst("address").Value;
                if (!string.IsNullOrEmpty(encryptedConnectionString))
                    ConnectionString = Security.Decrypt(encryptedConnectionString);
            }
            else
            {
                ConnectionString = this._configuration.GetConnectionString("DefaultConnection");
            }
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            optionsBuilder.UseSqlServer(ConnectionString, option => option.UseRowNumberForPaging())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new UserDbContext(optionsBuilder.Options);
        }
        public UserDbContext GetDbContext(string ConnetionString)
        {
            if (ConnetionString != null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
                optionsBuilder.UseSqlServer(ConnetionString, option => option.UseRowNumberForPaging())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new UserDbContext(optionsBuilder.Options);
            }
            else
                return null;
        }

        public UserDbContext GetDbContextByOrgCode(string orgCode)
        {
            string ConnetionString = this.GetConnectionStringByOrgnizationCode(orgCode).Result;
            if (ConnetionString != null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
                optionsBuilder.UseSqlServer(ConnetionString, option => option.UseRowNumberForPaging())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new UserDbContext(optionsBuilder.Options);
            }
            else
                return null;
        }

    }
}
