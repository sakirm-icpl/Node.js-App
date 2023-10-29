using Feedback.API.Models;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Services;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

namespace Feedback.API.Repositories
{
    public class CourseRepository : Repository<Feedback.API.Model.Course>, ICourseRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRepository));
        private FeedbackContext _db;

        private IConfiguration _configuration;
        IIdentityService _identitySv;
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly IHostingEnvironment _hostingEnvironment;
       // IAzureStorage _azurestorage;
        public CourseRepository(FeedbackContext context,  IConfiguration configuration, ICustomerConnectionStringRepository customerConnection, IIdentityService identitySv,  IHttpContextAccessor httpContextAccessor) : base(context)
        {
            this._db = context;
            _httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
            this._identitySv = identitySv;
           // this._hostingEnvironment = hostingEnvironment;
            this._customerConnection = customerConnection;
           // this._azurestorage = azurestorage;
        }

        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configurationCode });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = dt.Rows[0]["Value"].ToString();
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return value;
        }
    }
}
