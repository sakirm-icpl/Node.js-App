using Dapper;
using Microsoft.EntityFrameworkCore;
using CourseReport.API.APIModel;
using CourseReport.API.Data;
using CourseReport.API.Helper;
using CourseReport.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CourseReport.API.Service;
using log4net;
using Microsoft.Extensions.Configuration;
using System.IO;
using CourseReport.API.APIModel;
using CourseReport.API.Model;

namespace CourseReport.API.Repositories
{
    public class CourseReportRepository : ICourseReportRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseReportRepository));

        protected ReportDbContext _db;
        private readonly IIdentityService _identityService;
        protected ICustomerConnectionStringRepository _customerConnectionRepository;
        public static IConfigurationRoot Configuration { get; set; }
        static CourseReportRepository()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public CourseReportRepository(ReportDbContext context, IIdentityService identityService, ICustomerConnectionStringRepository customerConnectionRepository)
        {
            this._db = context;
            _identityService = identityService;
            this._customerConnectionRepository = customerConnectionRepository;
        }

        public async Task<string> GetConfigurableParameterValue(string Code)
        {
            try
            {
                string result = null;
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
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = Code });
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
                                result = string.IsNullOrEmpty(row["Value"].ToString()) ? null : row["Value"].ToString();
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<CourseWiseCompletionReports> GetCourseWiseCompletionReports(int page,int pageSize ,int UserId)
        {
            var Query = (from x in _db.ExportCourseCompletionDetailReport
                         join course in _db.Course on x.CourseId equals course.Id
                         join um in _db.UserMaster on x.CreatedBy equals um.Id
                         where x.CreatedBy == UserId && x.IsDownloaded == false
                         orderby x.CreatedDate descending
                         select new PostCourseWiseCompletionReport
                         {
                             Id = x.Id,
                             CourseId = x.CourseId,
                             CourseCode = course.Code,
                             CourseName = x.CourseName,
                             ExportPath = x.ExportPath,
                             Username = um.UserName,
                             RequestedDate = x.CreatedDate

                         }).AsNoTracking();

           
            CourseWiseCompletionReports ListandCount = new CourseWiseCompletionReports();
            ListandCount.Count = Query.Distinct().Count();

            ListandCount.ReportList = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
            return ListandCount;

        }

        public async Task<int> PostCourseWiseCompletionReport(APIPostCourseWiseCompletionReport data,int UserId)
        {
            ExportCourseCompletionDetailReport report = new ExportCourseCompletionDetailReport();
            report.CourseId = data.CourseId;
            report.CourseName = data.CourseName;
            report.ExportPath = null;
            report.IsDownloaded = false;
            report.ModifiedBy = UserId;
            report.CreatedBy = UserId;
            report.CreatedDate = DateTime.Now;
            report.ModifiedDate = DateTime.Now;

            await this._db.ExportCourseCompletionDetailReport.AddAsync(report);
            await this._db.SaveChangesAsync();

            ExportCourseCompletionDetailReport reportData= (from x in _db.ExportCourseCompletionDetailReport orderby x.CreatedDate descending
                                                    select x).FirstOrDefault();

            return reportData.Id;
        }

        public async Task<string> UpdateCourseWiseCompletionReport(APIUpdateCourseWiseCompletionReport data, int UserId)
        {
            ExportCourseCompletionDetailReport report = await this._db.ExportCourseCompletionDetailReport.Where(a=>a.Id==data.Id).FirstOrDefaultAsync();
            report.ExportPath = data.ExportPath;
            report.ModifiedDate = DateTime.Now;

             this._db.ExportCourseCompletionDetailReport.Update(report);
            await this._db.SaveChangesAsync();

            return report.ExportPath;
        }
        public async Task<ExportCourseCompletionDetailReport> UpdateonDownloadReport( int Id)
        {
            ExportCourseCompletionDetailReport report = await this._db.ExportCourseCompletionDetailReport.Where(a => a.Id == Id).FirstOrDefaultAsync();
            report.IsDownloaded = true;
            report.ModifiedDate = DateTime.Now;

            this._db.ExportCourseCompletionDetailReport.Update(report);
            await this._db.SaveChangesAsync();

            return report;
        }



        public async Task<IEnumerable<APICourseWiseCompletionReport>> GetCourseWiseCompletionReport(APICourseWiseCompletionModule courseWiseCompletionModule, string OrgCode)
        {
            IEnumerable<APICourseWiseCompletionReport> ObjAPICourseCompletionReport = null;

            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@CourseId", courseWiseCompletionModule.CourseId);
                    parameters.Add("@StartIndex", courseWiseCompletionModule.StartIndex);
                    parameters.Add("@PageSize", courseWiseCompletionModule.PageSize);
                    parameters.Add("@Search", courseWiseCompletionModule.Search);
                    parameters.Add("@SortOrder", courseWiseCompletionModule.SortOrder);
                    parameters.Add("@FromDate", courseWiseCompletionModule.FromDate);
                    parameters.Add("@Todate", courseWiseCompletionModule.ToDate);
                    parameters.Add("@UserStatus", courseWiseCompletionModule.UserStatus);
                    parameters.Add("@UserID", courseWiseCompletionModule.UserId);
                    parameters.Add("@OrgCode", OrgCode);
                    parameters.Add("@ModuleId", courseWiseCompletionModule.moduleId);
                    //There is no need to check the null parameter value for Configuration["MaxTimeOut"].Its set the 0 if its found null.
                    IEnumerable<APICourseWiseCompletionReport> Result = await SqlMapper.QueryAsync<APICourseWiseCompletionReport>((SqlConnection)connection, "dbo.Report_CoursewiseCompletionReport", parameters, null, Convert.ToInt32(Configuration["MaxTimeOut"]), CommandType.StoredProcedure);
                    ObjAPICourseCompletionReport = Result.ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return ObjAPICourseCompletionReport;
        }

        public async Task<IEnumerable<APICourseWiseCompletionReport>> GetCourseWiseCompletionReportExport(APICourseWiseCompletionModule courseWiseCompletionModule, string OrgCode)
        {
            IEnumerable<APICourseWiseCompletionReport> ObjAPICourseCompletionReport = null;

            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@CourseId", courseWiseCompletionModule.CourseId);
                    parameters.Add("@StartIndex", courseWiseCompletionModule.StartIndex);
                    parameters.Add("@PageSize", courseWiseCompletionModule.PageSize);
                    parameters.Add("@Search", courseWiseCompletionModule.Search);
                    parameters.Add("@SortOrder", courseWiseCompletionModule.SortOrder);
                    parameters.Add("@FromDate", courseWiseCompletionModule.FromDate);
                    parameters.Add("@Todate", courseWiseCompletionModule.ToDate);
                    parameters.Add("@UserStatus", courseWiseCompletionModule.UserStatus);
                    parameters.Add("@UserID", courseWiseCompletionModule.UserId);
                    parameters.Add("@OrgCode", OrgCode);
                    parameters.Add("@ModuleId", courseWiseCompletionModule.moduleId);
                    //There is no need to check the null parameter value for Configuration["MaxTimeOut"].Its set the 0 if its found null.
                    IEnumerable<APICourseWiseCompletionReport> Result = await SqlMapper.QueryAsync<APICourseWiseCompletionReport>((SqlConnection)connection, "dbo.Report_CoursewiseCompletionReportExport", parameters, null, Convert.ToInt32(Configuration["MaxTimeOut"]), CommandType.StoredProcedure);
                    ObjAPICourseCompletionReport = Result.ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return ObjAPICourseCompletionReport;
        }

        public async Task<IEnumerable<ApiModeratorwiseSubjectSummaryReport>> GetModeratorSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule subjectSummaryModule)
        {
            IEnumerable<ApiModeratorwiseSubjectSummaryReport> ObjAPISubjectSummarynReport = null;

            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    //parameters.Add("@CourseId", courseWiseCompletionModule.CourseId);


                    IEnumerable<ApiModeratorwiseSubjectSummaryReport> Result = await SqlMapper.QueryAsync<ApiModeratorwiseSubjectSummaryReport>((SqlConnection)connection, "dbo.Report_ModeratorwiseSubjectContentSummary", parameters, null, null, CommandType.StoredProcedure);
                    ObjAPISubjectSummarynReport = Result.ToList();
                    connection.Close();
                }
            }
            catch (Exception)
            {

            }
            return ObjAPISubjectSummarynReport;
        }
        public async Task<IEnumerable<ApiModeratorwiseSubjectDeailsReport>> GetModeratorSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule subjectDetailsModule)
        {
            IEnumerable<ApiModeratorwiseSubjectDeailsReport> ObjAPISubjectDetailsReport = null;

            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    //parameters.Add("@CourseId", courseWiseCompletionModule.CourseId);


                    IEnumerable<ApiModeratorwiseSubjectDeailsReport> Result = await SqlMapper.QueryAsync<ApiModeratorwiseSubjectDeailsReport>((SqlConnection)connection, "dbo.Report_ModeratorwiseSubjectContentDetails", parameters, null, null, CommandType.StoredProcedure);
                    ObjAPISubjectDetailsReport = Result.ToList();
                    connection.Close();
                }
            }
            catch (Exception)
            {

            }
            return ObjAPISubjectDetailsReport;
        }
        public async Task<DataTable> GetSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule objReport)
        {
            DataTable dt = new DataTable();
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
                            cmd.CommandText = "Report_ModeratorwiseSubjectContentSummary";
                            cmd.CommandType = CommandType.StoredProcedure;
                            //cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.Structured) { Value = objReport.Search });
                            //cmd.Parameters.Add(new SqlParameter("@StartIndex", SqlDbType.Int) { Value = objReport.StartIndex });
                            // cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.BigInt) { Value = objReport.PageSize });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            // DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                //return 0;
                            }
                            //foreach (DataRow row in dt.Rows)
                            //{
                            //    ConfigurableValue = (row["Value"].ToString());
                            //}

                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dt;
        }

        public async Task<DataTable> GetSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule objReport)
        {
            DataTable dt = new DataTable();
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
                            cmd.CommandText = "Report_ModeratorwiseSubjectContentDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            //cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.Structured) { Value = objReport.Search });
                            //cmd.Parameters.Add(new SqlParameter("@StartIndex", SqlDbType.Int) { Value = objReport.StartIndex });
                            // cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.BigInt) { Value = objReport.PageSize });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            // DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                //return 0;
                            }
                            //foreach (DataRow row in dt.Rows)
                            //{
                            //    ConfigurableValue = (row["Value"].ToString());
                            //}

                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dt;
        }


        #region User Learnig Report
        public async Task<IEnumerable<APIUserLearningReport>> GetUserLearningReport(APIUserLearningReportModule UserLearningReportModule, int UserID, string OrgCode)
        {
            IEnumerable<APIUserLearningReport> ObjAPIUserLearningReport = null;
                try
                {
                    using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                    {
                        DbConnection connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserId", UserLearningReportModule.UserId);
                        parameters.Add("@CourseId", UserLearningReportModule.CourseId);
                        parameters.Add("@Search", UserLearningReportModule.Search);
                        parameters.Add("@PageSize", UserLearningReportModule.PageSize);
                        parameters.Add("@StartIndex", UserLearningReportModule.StartIndex);
                        parameters.Add("@UserMasterID", UserID);
                        parameters.Add("@OrgCode", OrgCode);
                        //There is no need to check the null parameter value for Configuration["MaxTimeOut"].Its set the 0 if its found null.
                        IEnumerable<APIUserLearningReport> Result = await SqlMapper.QueryAsync<APIUserLearningReport>((SqlConnection)connection, "dbo.Report_UserLearningReport", parameters, null, Convert.ToInt32(Configuration["MaxTimeOut"]), CommandType.StoredProcedure);
                        ObjAPIUserLearningReport = Result.ToList();
                        foreach (APIUserLearningReport quizQuestionMergered in ObjAPIUserLearningReport)
                        {
                            quizQuestionMergered.UserID = Security.Decrypt(quizQuestionMergered.UserID);
                        }
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));

                }
            return ObjAPIUserLearningReport;
        }
        #endregion

        public async Task<IEnumerable<APICourseRatingReport>> GetCourseRatingReport(APICourseRatingReport aPICourseRatingReport)
        {
            IEnumerable<APICourseRatingReport> objAPICourseRatingReport = null;

            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();

                    parameters.Add("@CourseId", aPICourseRatingReport.CourseID);
                    parameters.Add("@PageSize", aPICourseRatingReport.PageSize);
                    parameters.Add("@Page", aPICourseRatingReport.StartIndex);

                    IEnumerable<APICourseRatingReport> Result = await SqlMapper.QueryAsync<APICourseRatingReport>((SqlConnection)connection, "dbo.Report_CourseRatingReport", parameters, null, null, CommandType.StoredProcedure);
                    objAPICourseRatingReport = Result.ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));


            }
            return objAPICourseRatingReport;
        }

        public async Task<int> GetCourseRatingReportCount(APICourseRatingReport aPICourseRatingReport)
        {
            int Count = 0;

            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[Report_CourseRatingReportCount]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPICourseRatingReport.CourseID });

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            connection.Close();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());

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
            return Count;
        }

        public async Task<IEnumerable<APIUserwiseCourseStatusReportResult>> GetUserwiseCourseStatusReport(APIUserwiseCourseStatusReport aPIUserwiseCourseStatusReport)
        {
            IEnumerable<APIUserwiseCourseStatusReportResult> aPIUserwiseCourseStatusReportResults = null;
            try
            {
                using (ReportDbContext context = this._customerConnectionRepository.GetDbContext())
                {
                    DbConnection connection = context.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserCategoryId", aPIUserwiseCourseStatusReport.UserCategoryId);
                    parameters.Add("@SalesOfficeId", aPIUserwiseCourseStatusReport.SalesOfficeId);
                    parameters.Add("@ControllingOfficeId", aPIUserwiseCourseStatusReport.ControllingOfficeId);
                    //There is no need to check the null parameter value for Configuration["MaxTimeOut"].Its set the 0 if its found null.
                    aPIUserwiseCourseStatusReportResults = await SqlMapper.QueryAsync<APIUserwiseCourseStatusReportResult>((SqlConnection)connection, "[dbo].[Report_UserwiseCourseStatus_IOCL]", parameters, null, Convert.ToInt32(Configuration["MaxTimeOut"]), CommandType.StoredProcedure);
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return aPIUserwiseCourseStatusReportResults;
        }

        public async Task<IEnumerable<ApiExportTcnsRetrainingReport>> GetTcnsRetrainingReport(APITcnsRetrainingReport tcnsRetrainingReport)
        {
            try
            {
                ApiExportTcnsRetrainingReport Report = new ApiExportTcnsRetrainingReport();
                List<ApiExportTcnsRetrainingReport> ReportList = new List<ApiExportTcnsRetrainingReport>();

                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (DbConnection connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (DbCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "Report_TcnsRetraining";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = tcnsRetrainingReport.CourseId });
                            cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = tcnsRetrainingReport.FromDate });
                            cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = tcnsRetrainingReport.ToDate });
                            await dbContext.Database.OpenConnectionAsync();
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    Report = new ApiExportTcnsRetrainingReport
                                    {
                                        UserId = Security.Decrypt(Convert.ToString(row["UserId"])),
                                        UserName = Convert.ToString(row["UserName"]),
                                        CourseCode = Convert.ToString(row["CourseCode"]),
                                        CourseTitle = Convert.ToString(row["CourseTitle"]),
                                        CourseStartDate = Convert.ToString(row["CourseStartDate"]),
                                        CourseCompletionDate = Security.Decrypt(Convert.ToString(row["CourseCompletionDate"])),
                                        CourseStatus = Security.Decrypt(Convert.ToString(row["CourseStatus"])),
                                        RetrainingDate = Convert.ToString(row["RetrainingDate"]),
                                        UserStatus = Convert.ToString(row["UserStatus"]),
                                        Department = Convert.ToString(row["Department"]),
                                        Designation = Convert.ToString(row["Designation"]),
                                        FunctionCode = Convert.ToString(row["FunctionCode"]),
                                        Group = Convert.ToString(row["Group"]),
                                        Region = Convert.ToString(row["Region"]),
                                        Score = Convert.ToString(row["Score"])
                                    };
                                    if (Report.UserStatus == "True")
                                    {
                                        Report.UserStatus = "Active";
                                    }
                                    else
                                    {
                                        Report.UserStatus = "Inactive";
                                    }
                                    ReportList.Add(Report);
                                }
                            }
                            reader.Dispose();
                            

                        }
                        connection.Close();
                    }
                }
                return ReportList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
    }
}
