using Microsoft.EntityFrameworkCore;
using log4net;
using Assessment.API.Model.Log_API_Count;
using Assessment.API.Model;
using Assessment.API.Model.Competency;
using Assessment.API.Model.EdCastAPI;
using Assessment.API.Model.Assessment;
//using System.Reflection;
//using Assessment.API.Model;

namespace Assessment.API.Models
{
    public class AssessmentContext : DbContext
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentContext));
        IHttpContextAccessor _httpContext;
        public AssessmentContext(DbContextOptions<AssessmentContext> options) : base(options)
        {
        }
        public AssessmentContext(DbContextOptions<AssessmentContext> options, IHttpContextAccessor httpContext) : base(options)
        {
            this._httpContext = httpContext;
        }
        public DbSet<Assessment.API.Model.Course> Course { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<LCMS> LCMS { get; set; }
        public DbSet<CourseModuleAssociation> CourseModuleAssociation { get; set; }
        public DbSet<AssessmentQuestionOption> AssessmentQuestionOption { get; set; }
        public DbSet<NodalUserGroups> NodalUserGroups { get; set; }
        public DbSet<NodalCourseRequests> NodalCourseRequests { get; set; }
        public DbSet<CourseCompletionStatus> CourseCompletionStatus { get; set; }
        public DbSet<AuthoringMaster> AuthoringMaster { get; set; }
        public DbSet<UserMasterDetails> UserMasterDetails { get; set; }
        public DbSet<AssessmentQuestionRejected> AssessmentQuestionRejected { get; set; }
        public DbSet<AssessmentCompetenciesMapping> AssessmentCompetenciesMapping { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestion { get; set; }
        public DbSet<AssessmentSheetConfiguration> AssessmentSheetConfiguration { get; set; }
        public DbSet<CameraPhotos> CameraPhotos { get; set; }

        public DbSet<CompetenciesMaster> CompetenciesMaster { get; set; }
        public DbSet<AssessmentSheetConfigurationDetails> AssessmentSheetConfigurationDetails { get; set; }
        public DbSet<AssessmentAttemptManagement> AssessmentAttemptManagement { get; set; }
        public DbSet<ModuleCompletionStatus> ModuleCompletionStatus { get; set; }
        public DbSet<AccessibilityRule> AccessibilityRule { get; set; }
        public DbSet<CameraEvaluation> CameraEvaluation { get; set; }

        public DbSet<SubjectiveAssessmentStatus> SubjectiveAssessmentStatus { get; set; }
        public DbSet<PostAssessmentResult> PostAssessmentResult { get; set; }
        public DbSet<AssessmentQuestionDetails> AssessmentQuestionDetails { get; set; }
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<DarwinboxTransactionDetails> DarwinboxTransactionDetails { get; set; }
        public DbSet<ContentCompletionStatus> ContentCompletionStatus { get; set; }
        public DbSet<CourseAuthorAssociation> CourseAuthorAssociation { get; set; }
        public DbSet<AnswerSheetsEvaluation> AnswerSheetsEvaluation { get; set; }
        public DbSet<AssessmentConfiguration> AssessmentConfiguration { get; set; }
        public DbSet<GradingRules> GradingRules { get; set; }





        public DbSet<ClientUserApiCount> ClientUserApiCount { get; set; }
        public DbSet<CustomerConnectionString> CustomerConnectionString { get; set; }

        public static class DbContextFactory
        {
            public static Dictionary<string, string>? ConnectionStrings { get; set; }

            public static void SetConnectionString(Dictionary<string, string> connStrs)
            {
                ConnectionStrings = connStrs;
            }

            public static AssessmentContext Create(string connectionString)
            {
                var optionsBuilder = new DbContextOptionsBuilder<AssessmentContext>();
                optionsBuilder.UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new AssessmentContext(optionsBuilder.Options);
            }
        }

        [DbFunction(Schema = "dbo")]
        public static DateTime GetCourseAssignedDateForRewardPoints(int userId, int courseId)
        {
            throw new Exception();
        }
    }
}

