using Microsoft.EntityFrameworkCore;
using log4net;
using CourseApplicability.API.Model.Log_API_Count;
using CourseApplicability.API.Model;
using CourseApplicability.API.APIModel;

namespace CourseApplicability.API.Models
{
    public class CoursesApplicabilityContext : DbContext
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CoursesApplicabilityContext));
        IHttpContextAccessor _httpContext;
        public CoursesApplicabilityContext(DbContextOptions<CoursesApplicabilityContext> options) : base(options)
        {
        }
        public CoursesApplicabilityContext(DbContextOptions<CoursesApplicabilityContext> options, IHttpContextAccessor httpContext) : base(options)
        {
            this._httpContext = httpContext;
        }

        public DbSet<ClientUserApiCount> ClientUserApiCount { get; set; }
        public DbSet<CustomerConnectionString> CustomerConnectionString { get; set; }
        public DbSet<DevelopmentPlanForCourse> DevelopmentPlanForCourse { get; set; }
        public DbSet<UserDevelopmentPlanMapping> UserDevelopmentPlanMapping { get; set; }
        public DbSet<AccessibilityRule> AccessibilityRule { get; set; }
        public DbSet<UserTeams> UserTeams { get; set; }
        public DbSet<CoursesEnrollRequest> CoursesEnrollRequest { get; set; }
        public DbSet<NodalCourseRequests> NodalCourseRequests { get; set; }
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<UserMasterDetails> UserMasterDetails { get; set; }
        public DbSet<CourseApplicability.API.Model.Course> Course { get; set; }
        public DbSet<InvoicePaymentResponse> InvoicePaymentResponse { get; set; }
        public DbSet<NodalUserGroups> NodalUserGroups { get; set; }
        public DbSet<CourseCompletionStatus> CourseCompletionStatus { get; set; }
        public DbSet<CourseModuleAssociation> CourseModuleAssociation { get; set; }
        public DbSet<ContentCompletionStatus> ContentCompletionStatus { get; set; }
        public DbSet<ModuleCompletionStatus> ModuleCompletionStatus { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<ScormVars> ScormVars { get; set; }
        public DbSet<ScormVarResult> ScormVarResult { get; set; }
        public DbSet<ApplicabilityGroupTemplate> ApplicabilityGroupTemplate { get; set; }
        public DbSet<UserTeamsMapping> UserTeamsMapping { get; set; }
        public DbSet<ScheduleVisibilityRule> ScheduleVisibilityRule { get; set; }

        public static class DbContextFactory
        {
            public static Dictionary<string, string> ConnectionStrings { get; set; }

            public static void SetConnectionString(Dictionary<string, string> connStrs)
            {
                ConnectionStrings = connStrs;
            }

            public static CoursesApplicabilityContext Create(string connectionString)
            {
                var optionsBuilder = new DbContextOptionsBuilder<CoursesApplicabilityContext>();
                optionsBuilder.UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new CoursesApplicabilityContext(optionsBuilder.Options);
            }
        }
    }
}
