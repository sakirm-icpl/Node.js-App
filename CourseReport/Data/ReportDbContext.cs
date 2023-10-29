using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CourseReport.API.APIModel;
using CourseReport.API.Helper;
using CourseReport.API.Helper.Log_API_Count;
using CourseReport.API.Model;
using System.Collections.Generic;


namespace CourseReport.API.Data
{
    public class ReportDbContext : DbContext
    {
        private IHttpContextAccessor _httpContext;
        public ReportDbContext(DbContextOptions<ReportDbContext> options)
            : base(options)
        {
        }
        public ReportDbContext(DbContextOptions<ReportDbContext> options, IHttpContextAccessor httpContext) : base(options)
        {
            this._httpContext = httpContext;
        }
        public DbSet<CustomerConnectionString> CustomerConnectionString { get; set; }
        //public DbSet<Tokens> Tokens { get; set; }
        //public DbSet<AllUsersActivityReportExport> AllUsersActivityReportExport { get; set; }
        //public DbSet<UserWiseCourseCompletionReportExport> UserWiseCourseCompletionReportExport { get; set; }
        //public DbSet<ILTUserDetailsReportExport> ILTUserDetailsReportExport { get; set; }
        //public DbSet<UserLoginHistoryReportExport> UserLoginHistoryReportExport { get; set; }
        //public DbSet<CourseAssessmentReportExport> CourseAssessmentReportExport { get; set; }
        //public DbSet<CourseModuleReportExport> CourseModuleReportExport { get; set; }
        //public DbSet<ILTDashboardReportExport> ILTDashboardReportExport { get; set; }
        public DbSet<ILTSchedule> ILTSchedule { get; set; }
        public DbSet<TeamsScheduleDetails> TeamsScheduleDetails { get; set; }
        public DbSet<ZoomMeetingDetails> ZoomMeetingDetails { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }

        public DbSet<ExportCourseCompletionDetailReport> ExportCourseCompletionDetailReport { get; set; }
        public DbSet<Course> Course { get; set; }
        //public DbSet<OpsAuditQuestion> opsAuditQuestions { get; set; }
        //public DbSet<ProcessEvaluationQuestion> ProcessEvaluationQuestion { get; set; }
        //public DbSet<NightAuditQuestion> NightAuditQuestion { get; set; }
        //public DbSet<CriticalAuditQuestion> CriticalAuditQuestion { get; set; }
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<ClientUserApiCount> ClientUserApiCount { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string ConnectionString = null;
            if (this._httpContext != null)
            {
                HttpContext httpContext = this._httpContext.HttpContext;
                if (httpContext != null)
                {
                    string encryptedConnectionString = httpContext.User.FindFirst("address") == null ? null : httpContext.User.FindFirst("address").Value;
                    if (!string.IsNullOrEmpty(encryptedConnectionString))
                        ConnectionString = Security.Decrypt(encryptedConnectionString);
                    optionsBuilder.UseSqlServer(ConnectionString, opt => opt.UseRowNumberForPaging())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    base.OnConfiguring(optionsBuilder);
                }
            }
        }
    }

    public static class DbContextFactory
    {
        public static Dictionary<string, string> ConnectionStrings { get; set; }

        public static void SetConnectionString(Dictionary<string, string> connStrs)
        {
            ConnectionStrings = connStrs;
        }

        public static ReportDbContext Create(string connectionString)
        {
            DbContextOptionsBuilder<ReportDbContext> optionsBuilder = new DbContextOptionsBuilder<ReportDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new ReportDbContext(optionsBuilder.Options);
        }
    }
}
