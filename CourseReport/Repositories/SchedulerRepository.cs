using Dapper;
using Microsoft.EntityFrameworkCore;
using CourseReport.API.APIModel;
using CourseReport.API.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using CourseReport.API.Helper;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CourseReport.API.Repositories.Interface
{
    public class SchedulerRepository : ISchedulerRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SchedulerRepository));
        protected ReportDbContext _db;
        protected ICustomerConnectionStringRepository _customerConnectionRepository;
        public static IConfigurationRoot Configuration { get; set; }
        static SchedulerRepository()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }
        public SchedulerRepository(ReportDbContext context, ICustomerConnectionStringRepository customerConnectionRepository)
        {
            this._db = context;
            _customerConnectionRepository = customerConnectionRepository;
        }

        public async Task<IEnumerable<APISchedulerReport>> GetAllCoursesCompletionReport(APISchedulerModule schedulermodule)
        {
            IEnumerable<APISchedulerReport> ApiSchedulerReport = null;
            string SHOW_CONFCOLUMNS_INREPORT = "No";
            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (DbConnection connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (DbCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetConfigurableParameterValue";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "SHOW_CONFCOLUMNS_INREPORT" });
                            //cmd.CommandTimeout = Convert.ToInt32(Configuration["MaxTimeOut"]);
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                SHOW_CONFCOLUMNS_INREPORT = string.IsNullOrEmpty(row["Value"].ToString()) ? null : row["Value"].ToString();
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
            if (SHOW_CONFCOLUMNS_INREPORT == "Yes")
            {
                try
                {
                    using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                    {
                        DbConnection connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        if (schedulermodule.StartIndex != -1 || schedulermodule.StartIndex != null)
                            schedulermodule.StartIndex = schedulermodule.StartIndex - 1;

                        DynamicParameters parameters = new DynamicParameters();

                        parameters.Add("@FromDate", schedulermodule.FromDate);
                        parameters.Add("@ToDate", schedulermodule.ToDate);
                        parameters.Add("@RegionID", schedulermodule.RegionID);
                        parameters.Add("@UserID", schedulermodule.UserID);
                        //There is no need to check the null parameter value for Configuration["MaxTimeOut"].Its set the 0 if its found null.
                        IEnumerable<APISchedulerReport> Result = await SqlMapper.QueryAsync<APISchedulerReport>((SqlConnection)connection, "dbo.GetCourseCompletionReportData_Configure", parameters, null, Convert.ToInt32(Configuration["MaxTimeOut"]), CommandType.StoredProcedure);
                        ApiSchedulerReport = Result.ToList();
                        connection.Close();
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error( Utilities.GetDetailedException(ex));


                }
            }
            else
            {
                try
                {
                    using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                    {
                        DbConnection connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        if (schedulermodule.StartIndex != -1 || schedulermodule.StartIndex != null)
                            schedulermodule.StartIndex = schedulermodule.StartIndex - 1;

                        DynamicParameters parameters = new DynamicParameters();

                        parameters.Add("@FromDate", schedulermodule.FromDate);
                        parameters.Add("@ToDate", schedulermodule.ToDate);
                        parameters.Add("@RegionID", schedulermodule.RegionID);
                        parameters.Add("@UserID", schedulermodule.UserID);
                        //There is no need to check the null parameter value for Configuration["MaxTimeOut"].Its set the 0 if its found null.
                        IEnumerable<APISchedulerReport> Result = await SqlMapper.QueryAsync<APISchedulerReport>((SqlConnection)connection, "dbo.GetCourseCompletionReportData_withoutConfigure", parameters, null, Convert.ToInt32(Configuration["MaxTimeOut"]), CommandType.StoredProcedure);
                        ApiSchedulerReport = Result.ToList();
                        connection.Close();
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error( Utilities.GetDetailedException(ex));


                }

            }
            return ApiSchedulerReport;
        }
        public async Task<IEnumerable<APIExportAllCoursesCompletionReport>> ExportAllCoursesCompletionReport()
        {
            IEnumerable<APIExportAllCoursesCompletionReport> ApiSchedulerReport = null;

            string SHOW_CONFCOLUMNS_INREPORT = "No";
            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (DbConnection connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (DbCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetConfigurableParameterValue";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "SHOW_CONFCOLUMNS_INREPORT" });
                           // cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                SHOW_CONFCOLUMNS_INREPORT = string.IsNullOrEmpty(row["Value"].ToString()) ? null : row["Value"].ToString();
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
            if (SHOW_CONFCOLUMNS_INREPORT == "Yes")
            {
                try
                {
                    using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                    {
                        DbConnection connection = dbContext.Database.GetDbConnection();

                        DynamicParameters parameters = new DynamicParameters();
                        //There is no need to check the null parameter value for Configuration["MaxTimeOut"].Its set the 0 if its found null.
                        IEnumerable<APIExportAllCoursesCompletionReport> Result = await SqlMapper.QueryAsync<APIExportAllCoursesCompletionReport>((SqlConnection)connection, "dbo.GetUserSetting", parameters, null, Convert.ToInt32(Configuration["MaxTimeOut"]), CommandType.StoredProcedure);
                        ApiSchedulerReport = Result.ToList();
                        connection.Close();
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error( Utilities.GetDetailedException(ex));


                }
            }
            return ApiSchedulerReport;
        }

    }
}
