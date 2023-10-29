using WallFeed.API.Data;
using WallFeed.API.Helper;
using WallFeed.API.Models;
using WallFeed.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using log4net;

namespace WallFeed.API.Repositories
{
    public class CustomerConnectionStringRepository : Repository<CustomerConnectionString>, ICustomerConnectionStringRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CustomerConnectionStringRepository));
        private GadgetDbContext _db;
        private IConfiguration _configuration;
        private IHttpContextAccessor _httpContext;
        public CustomerConnectionStringRepository(GadgetDbContext context, IConfiguration configuration, IHttpContextAccessor httpContext) : base(context)
        {
            this._db = context;
            this._configuration = configuration;
            this._httpContext = httpContext;
        }
        public void SetDbContext(GadgetDbContext db)
        {
            this._db = db;
        }
        public async Task<string> GetConnectionString(string clientId)
        {
            try
            {
                IQueryable<string> result = (from cc in this._db.CustomerConnectionString
                                             where (cc.Code.ToLower() == clientId.ToLower())
                                             select cc.ConnectionString);

                return await result.AsNoTracking().SingleAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                string Ex = ex.Message;
            }
            return string.Empty;
        }
        public async Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode)
        {
            if (string.IsNullOrEmpty(OrgnizationCode))
            {
                return ApiStatusCode.Unauthorized.ToString();
            }
            string OrgnizationConnectionString = null;
            DbContextOptionsBuilder<GadgetDbContext> optionsBuilder = new DbContextOptionsBuilder<GadgetDbContext>();
            optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultMasterConnection"))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            using (GadgetDbContext context = new GadgetDbContext(optionsBuilder.Options))
            {

                if (context == null)
                {
                    return ApiStatusCode.BadRequest.ToString();
                }
                OrgnizationCode = OrgnizationCode.ToLower();
                OrgnizationConnectionString = await (from cc in context.CustomerConnectionString
                                                     where (cc.Code.ToLower() == OrgnizationCode.ToLower())
                                                     select cc.ConnectionString).AsNoTracking().FirstOrDefaultAsync();
                if (OrgnizationConnectionString == null)
                    return ApiStatusCode.Unauthorized.ToString();
            }
            optionsBuilder.UseSqlServer(OrgnizationConnectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            using (GadgetDbContext context = new GadgetDbContext(optionsBuilder.Options))
            {
                if (context == null)
                {
                    return ApiStatusCode.BadRequest.ToString();
                }
            }
            return OrgnizationConnectionString;
        }
        public GadgetDbContext GetDbContext()
        {
            string ConnectionString = null;
            if (this._httpContext != null)
            {
                HttpContext httpContext = this._httpContext.HttpContext;
                string encryptedConnectionString = httpContext.User.FindFirst("address") == null ? null : httpContext.User.FindFirst("address").Value;
                if (!string.IsNullOrEmpty(encryptedConnectionString))
                    ConnectionString = Security.Decrypt(encryptedConnectionString);
            }
            else
            {
                ConnectionString = this._configuration.GetConnectionString("DefaultConnection");
            }
            DbContextOptionsBuilder<GadgetDbContext> optionsBuilder = new DbContextOptionsBuilder<GadgetDbContext>();
            optionsBuilder.UseSqlServer(ConnectionString, option => option.UseRowNumberForPaging())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new GadgetDbContext(optionsBuilder.Options);
        }
        public GadgetDbContext GetDbContext(string ConnetionString)
        {
            if (ConnetionString != null)
            {
                DbContextOptionsBuilder<GadgetDbContext> optionsBuilder = new DbContextOptionsBuilder<GadgetDbContext>();
                optionsBuilder.UseSqlServer(ConnetionString, option => option.UseRowNumberForPaging())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new GadgetDbContext(optionsBuilder.Options);
            }
            else
                return null;
        }
    }
}
