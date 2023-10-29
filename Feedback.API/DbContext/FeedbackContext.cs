

using Feedback.API.Model;
using Microsoft.EntityFrameworkCore;
using log4net;
using Feedback.API.Model.Log_API_Count;
using System.Reflection;
using Feedback.API.Model.Feedback;
using Feedback.API.APIModel;
using Assessment.API.Models;

namespace Feedback.API.Models
{
    public class FeedbackContext : DbContext
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedbackContext));
        IHttpContextAccessor _httpContext;
        public FeedbackContext(DbContextOptions<FeedbackContext> options) : base(options)
        {
        }
        public FeedbackContext(DbContextOptions<FeedbackContext> options, IHttpContextAccessor httpContext) : base(options)
        {
            this._httpContext = httpContext;
        }

        public DbSet<Model.Module> Module { get; set; }
        public DbSet<LCMS> LCMS { get; set; }
        public DbSet<ClientUserApiCount> ClientUserApiCount { get; set; }
        public DbSet<CustomerConnectionString> CustomerConnectionString { get; set; }
        public DbSet<CommonSmileSheet> CommonSmileSheet { get; set; }
        public DbSet<FeedbackOption> FeedbackOption { get; set; }
        public DbSet<FeedbackQuestion> FeedbackQuestion { get; set; }
        public DbSet<FeedbackQuestionRejected> FeedbackQuestionRejected { get; set; }
        public DbSet<FeedbackSheetConfigurationDetails> FeedbackSheetConfigurationDetails { get; set; }
        public DbSet<Feedback.API.Model.Course> Course { get; set; }
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<UserMasterDetails> UserMasterDetails { get; set; }
        public DbSet<FeedbackStatus> FeedbackStatus { get; set; }
        public DbSet<FeedbackStatusDetail> FeedbackStatusDetail { get; set; }
        public DbSet<CourseAuthorAssociation> CourseAuthorAssociation { get; set; }
        public DbSet<CourseModuleAssociation> CourseModuleAssociation { get; set; }
        public DbSet<UserWiseFeedbackAggregation> UserWiseFeedbackAggregation { get; set; }

        public DbSet<FeedbackSheetConfiguration> FeedbackSheetConfiguration { get; set; }
        public DbSet<AccessibilityRule> AccessibilityRule { get; set; }

        public DbSet<CourseCompletionStatus> CourseCompletionStatus { get; set; }
        public DbSet<ModuleCompletionStatus> ModuleCompletionStatus { get; set; }
        public DbSet<NodalUserGroups> NodalUserGroups { get; set; }
        public DbSet<NodalCourseRequests> NodalCourseRequests { get; set; }
        public DbSet<AuthoringMaster> AuthoringMaster { get; set; }
        public DbSet<DevelopmentPlanForCourse> DevelopmentPlanForCourse { get; set; }
        public DbSet<CourseMappingToDevelopment> CourseMappingToDevelopment { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestion { get; set; }
        public DbSet<AssessmentQuestionOption> AssessmentQuestionOption { get; set; }
        public DbSet<AssessmentQuestionRejected> AssessmentQuestionRejected { get; set; }
        public DbSet<TrainerFeedback> TrainerFeedback { get; set; }





        public static class DbContextFactory
        {
            public static Dictionary<string, string>? ConnectionStrings { get; set; }

            public static void SetConnectionString(Dictionary<string, string> connStrs)
            {
                ConnectionStrings = connStrs;
            }

            public static FeedbackContext Create(string connectionString)
            {
                var optionsBuilder = new DbContextOptionsBuilder<FeedbackContext>();
                optionsBuilder.UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new FeedbackContext(optionsBuilder.Options);
            }



        }

        [DbFunction(Schema = "dbo")]
        public static DateTime GetCourseAssignedDateForRewardPoints(int userId, int courseId)
        {
            throw new Exception();
        }
    }

}
