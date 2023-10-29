using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Courses.API.Helper;
using log4net;
namespace Courses.API.Repositories
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SystemSettingRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IConfiguration _configuration;
        public SystemSettingRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection)
        {
            this._db = context;
            this._customerConnection = customerConnection;
        }

        public async Task<int> GetScormFileMaxSizeInMb()
        {
            int FileSize = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetScormMaxFileSize";
                        cmd.CommandType = CommandType.StoredProcedure;
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                FileSize = string.IsNullOrEmpty(row["MaxFileSize"].ToString()) ? 0 : Convert.ToInt32(row["MaxFileSize"].ToString());
                            }
                        }
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return FileSize;
        }
    }
}
