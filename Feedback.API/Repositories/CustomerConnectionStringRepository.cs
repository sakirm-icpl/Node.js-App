using Microsoft.EntityFrameworkCore;
using log4net;
using Feedback.API.Helper;
using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Repositories;
using Feedback.API.Models;
using static Feedback.API.Common.EnumHelper;

namespace Feedback.API.Repositories
{
    public class CustomerConnectionStringRepository : Repository<CustomerConnectionString>, ICustomerConnectionStringRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CustomerConnectionStringRepository));
        private FeedbackContext _db;
        private IConfiguration _configuration;
        IHttpContextAccessor _httpContext;
        public CustomerConnectionStringRepository(FeedbackContext context,
            IConfiguration configuration,
             IHttpContextAccessor httpContext) : base(context)
        {
            this._db = context;
            this._configuration = configuration;
            this._httpContext = httpContext;
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
        public async Task<string> GetConnectionStringByCode(string orgCode)
        {
            try
            {
                var result = (from cc in this._db.CustomerConnectionString
                              where (cc.Code.ToLower() == orgCode.ToLower())
                              select cc.ConnectionString);

                return await result.SingleAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return string.Empty;
        }

        public async Task<string> GetConnectionStringByOrgnizationCodeForExternal(string OrgnizationCode)
        {
            _logger.Debug("In Function GetConnectionStringByOrgnizationCode to fetch connectionstring for Org :-" + OrgnizationCode);
            if (string.IsNullOrEmpty(OrgnizationCode))
            {
                return ApiStatusCode.Unauthorized.ToString();
            }
            string OrgnizationConnectionString = null;
            var optionsBuilder = new DbContextOptionsBuilder<FeedbackContext>();
            optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("EmpoweredMaster"))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            _logger.Debug("configuration.GetConnectionString(EmpoweredMaster :-" + this._configuration.GetConnectionString("EmpoweredMaster"));
            using (var context = new FeedbackContext(optionsBuilder.Options))
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
            using (var context = new FeedbackContext(optionsBuilder.Options))
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

        public async Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode)
        {
            if (string.IsNullOrEmpty(OrgnizationCode))
            {
                return ApiStatusCode.Unauthorized.ToString();
            }
            string OrgnizationConnectionString = null;
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<FeedbackContext>();
                optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultConnection"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                using (var context = new FeedbackContext(optionsBuilder.Options))
                {

                    if (context == null)
                    {
                        return ApiStatusCode.BadRequest.ToString();
                    }
                    OrgnizationCode = OrgnizationCode.ToLower();
                    OrgnizationConnectionString = await (from cc in context.CustomerConnectionString
                                                         where cc.Code.ToLower() == OrgnizationCode.ToLower()
                                                         select cc.ConnectionString).AsNoTracking().FirstOrDefaultAsync();
                    if (OrgnizationConnectionString == null)
                        return ApiStatusCode.Unauthorized.ToString();
                }
                optionsBuilder.UseSqlServer(OrgnizationConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                using (var context = new FeedbackContext(optionsBuilder.Options))
                {
                    optionsBuilder.UseSqlServer(OrgnizationConnectionString)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    if (context == null)
                    {
                        return ApiStatusCode.BadRequest.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return OrgnizationConnectionString;
        }
        public FeedbackContext GetDbContext()
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
            var optionsBuilder = new DbContextOptionsBuilder<FeedbackContext>();
            optionsBuilder.UseSqlServer(ConnectionString, option => option.UseRowNumberForPaging())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new FeedbackContext(optionsBuilder.Options);
        }
        public FeedbackContext GetDbContext(string ConnetionString)
        {
            if (ConnetionString != null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<FeedbackContext>();
                optionsBuilder.UseSqlServer(ConnetionString, option => option.UseRowNumberForPaging())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new FeedbackContext(optionsBuilder.Options);
            }
            else
                return null;
        }

        public async Task<string> GetConnectionStringByOrgnizationCodeNew(string OrgnizationCode)
        {
            _logger.Debug("In Function GetConnectionStringByOrgnizationCode to fetch connectionstring for Org :-" + OrgnizationCode);
            if (string.IsNullOrEmpty(OrgnizationCode))
            {
                return ApiStatusCode.Unauthorized.ToString();
            }
            string OrgnizationConnectionString = null;
            var optionsBuilder = new DbContextOptionsBuilder<FeedbackContext>();
            optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("EmpoweredMaster"))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            _logger.Debug("configuration.GetConnectionString(DefaultMasterConnection :-" + this._configuration.GetConnectionString("DefaultMasterConnection"));
            using (var context = new FeedbackContext(optionsBuilder.Options))
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
            using (var context = new FeedbackContext(optionsBuilder.Options))
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
            List<Feedback.API.APIModel.ThirdPartyIntegration.ConnectionInfo> clientConnectionStrings = null;
            try
            {
                var cache = new CacheManager.CacheManager();

                #region "Fetch Connection Strings from Database"
                var optionsBuilder = new DbContextOptionsBuilder<FeedbackContext>();
                optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("EmpoweredMaster"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                using (var context = new FeedbackContext(optionsBuilder.Options))
                {
                    if (context == null)
                    {
                        return ApiStatusCode.BadRequest.ToString();
                    }
                    clientConnectionStrings = await (from cc in context.CustomerConnectionString
                                                     select new Feedback.API.APIModel.ThirdPartyIntegration.ConnectionInfo { ConnectionString = cc.ConnectionString, ClientCode = cc.Code }).AsNoTracking().ToListAsync();
                }
                #endregion

                return clientConnectionStrings.Where(A => A.ClientCode.ToUpper() == orgcode.ToUpper()).Select(A => A.ConnectionString).FirstOrDefault();
            }
            catch (System.Exception ex)
            { _logger.Error(string.Format("Exception while getting connection string value for client {0}. Exception :- {1} :-", orgcode, ex.Message)); }
            return null;
        }
    }
}
