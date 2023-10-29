using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using User.API.Helper;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class ConfigurableParameterValuesRepository : IConfigurableParameterValuesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserRepository));
        private ICustomerConnectionStringRepository _customerConnectionString;
        public ConfigurableParameterValuesRepository(ICustomerConnectionStringRepository customerConnectionString)
        {
            _customerConnectionString = customerConnectionString;
        }

        public async Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string defaultValue = "")
        {
            DataTable dtConfigurationValues;
            string configValue;
            try
            {
                var cache = new CacheManager.CacheManager();
                string cacheKeyConfig = (Helper.Constants.CacheKeyNames.CONFIGURABLE_VALUES + "-" + orgCode).ToUpper();

                if (cache.IsAdded(cacheKeyConfig))
                    dtConfigurationValues = cache.Get<DataTable>(cacheKeyConfig);
                else
                {
                    dtConfigurationValues = this.GetAllConfigurableParameterValue();
                    cache.Add(cacheKeyConfig, dtConfigurationValues, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                }
                DataRow[] dr = dtConfigurationValues.Select("Code ='" + configurationCode + "'");
                if (dr.Length > 0)
                    configValue = Convert.ToString(dr[0]["Value"]);
                else
                    configValue = defaultValue;
                _logger.Debug(string.Format("Value for Config {0} for Organization {1} is {2} ", configurationCode, orgCode, configValue));
            }
            catch (System.Exception ex)
            {
                _logger.Error(string.Format("Exception in function GetConfigurationValueAsync :- {0} for Organisation {1}", Utilities.GetDetailedException(ex), orgCode));
                return null;
            }
            return configValue;
        }

        public DataTable GetAllConfigurableParameterValue()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetAllConfigurableParameterValues";
                        cmd.CommandType = CommandType.StoredProcedure;
                        DbDataReader reader = cmd.ExecuteReader();
                        dt.Load(reader);

                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (System.Exception ex)
            { _logger.Error("Exception in function GetAllConfigurableParameterValue :-" + Utilities.GetDetailedException(ex)); }

            return dt;
        }



    }
}
