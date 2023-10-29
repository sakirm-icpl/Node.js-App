using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Saml.API.Data;
using Saml.API.Repositories.Interfaces;
using log4net;
using System;
using Saml.API.Helper;

namespace Saml.API.Repositories
{
    public class ConfigurationParameterRepository : IConfigurationParameterRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ConfigurationParameterRepository));
        private readonly UserDbContext _db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public ConfigurationParameterRepository(UserDbContext db, ICustomerConnectionStringRepository customerConnectionString)
        {
            this._db = db;
            this._customerConnectionString = customerConnectionString;
        }

        public async Task<bool> IsEmailConfigured()
        {
            bool IsEmailConfigured = false;
            string EmailConfigurationName = "EMAIL_NOTIFICATION";
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = EmailConfigurationName });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                string value = row["Value"].ToString();
                                if (value != null)
                                {
                                    if (value.ToLower().Equals("yes"))
                                    {
                                        IsEmailConfigured = true;
                                    }
                                    else
                                    {
                                        IsEmailConfigured = false;
                                    }
                                }
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return IsEmailConfigured;
        }
    }
}
