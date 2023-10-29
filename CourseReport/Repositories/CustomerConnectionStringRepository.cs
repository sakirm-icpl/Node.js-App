using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CourseReport.API.Data;
using CourseReport.API.Helper;
using CourseReport.API.Model;
using CourseReport.API.Repositories.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace CourseReport.API.Repositories
{
    public class CustomerConnectionStringRepository : Repository<CustomerConnectionString>, ICustomerConnectionStringRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CustomerConnectionStringRepository));
        private ReportDbContext _db;
        private IConfiguration _configuration;
        private IHttpContextAccessor _httpContext;
        public CustomerConnectionStringRepository(ReportDbContext context,
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
                IQueryable<string> result = (from cc in this._db.CustomerConnectionString
                                             where cc.Code.ToLower()==clientId.ToLower()
                                             select cc.ConnectionString);

                return await result.AsNoTracking().SingleAsync();
            }
            catch(Exception ex)
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
            DbContextOptionsBuilder<ReportDbContext> optionsBuilder = new DbContextOptionsBuilder<ReportDbContext>();
            optionsBuilder.UseSqlServer(this._configuration.GetConnectionString("DefaultMasterConnection"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            using (ReportDbContext context = new ReportDbContext(optionsBuilder.Options))
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
            using (ReportDbContext context = new ReportDbContext(optionsBuilder.Options))
            {
                optionsBuilder.UseSqlServer(OrgnizationConnectionString);
                if (context == null)
                {
                    return ApiStatusCode.BadRequest.ToString();
                }
            }
            return OrgnizationConnectionString;
        }
        public ReportDbContext GetDbContext()
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
            DbContextOptionsBuilder<ReportDbContext> optionsBuilder = new DbContextOptionsBuilder<ReportDbContext>();
            optionsBuilder
                .UseSqlServer(ConnectionString, option => option.UseRowNumberForPaging())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new ReportDbContext(optionsBuilder.Options);
        }
        public ReportDbContext GetDbContext(string ConnetionString)
        {
            if (ConnetionString != null)
            {
                DbContextOptionsBuilder<ReportDbContext> optionsBuilder = new DbContextOptionsBuilder<ReportDbContext>();
                optionsBuilder.UseSqlServer(ConnetionString, option => option.UseRowNumberForPaging())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new ReportDbContext(optionsBuilder.Options);
            }
            else
                return null;
        }
    }
}
