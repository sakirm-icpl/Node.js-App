using log4net;
using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Repositories;
using static Feedback.API.Models.FeedbackContext;
using Feedback.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

namespace Courses.API.Repositories
{
    public class AccessibiltyRuleRepository : Repository<AccessibilityRule>, IAccessibilityRule
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleRepository));
        private FeedbackContext _db;
        private readonly IConfiguration _configuration;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
        
       

        public AccessibiltyRuleRepository(FeedbackContext context, IConfiguration configuration,    ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
           
           

        }
        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<AccessibilityRule>();
        }

        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnectionRepository.GetDbContext())
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
