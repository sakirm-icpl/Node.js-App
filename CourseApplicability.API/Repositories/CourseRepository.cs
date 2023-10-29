using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using log4net;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Models;

namespace CourseApplicability.API.Repositories
{
    public class CourseRepository : Repository<CourseApplicability.API.Model.Course>, ICourseRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRepository));
        private CoursesApplicabilityContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CourseRepository(CoursesApplicabilityContext context, ICustomerConnectionStringRepository customerConnection , IHttpContextAccessor httpContextAccessor) : base(context)
        {
            this._db = context;
            _httpContextAccessor = httpContextAccessor;
            this._customerConnection = customerConnection;
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