//using Assessment.API.Models;
using ILT.API.Model;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Model.EdCastAPI;
//using ILT.API.Model.ActivitiesManagement;
using ILT.API.Model.AdministrativeFunctions;
//using ILT.API.Model.Competency;
//using ILT.API.Model.CourseRating;
//using ILT.API.Model.DiscussionForum;
//using ILT.API.Model.Feedback;
using ILT.API.Model.ILT;
//using ILT.API.Model.MySupervisoryFunction;
//using ILT.API.Model.Process;
using ILT.API.Model.TNA;
//using ILT.API.Model.TrainersFunctions;
//using Feedback.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
//using Process.API.Models;
using System;
using System.Collections.Generic;
using log4net;
//using ILT.API.Model.EdCastAPI;
//using ILT.API.Model.ThirdPartyIntegration;
//using ILT.API.Model.CourseDetail;
using ILT.API.Model.Log_API_Count;
//using ILT.API.Model.Assessment;

namespace ILT.API.Models
{
    public class CourseContext : DbContext
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseContext));
        IHttpContextAccessor _httpContext;
        public CourseContext(DbContextOptions<CourseContext> options) : base(options)
        {
        }
        public CourseContext(DbContextOptions<CourseContext> options, IHttpContextAccessor httpContext) : base(options)
        {
            this._httpContext = httpContext;
        }

        public DbSet<ILT.API.Model.Course> Course { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Module> Module { get; set; }
        //public DbSet<AdditionalLearning> AdditionalLearning { get; set; }
        public DbSet<TrainingPlace> TrainingPlace { get; set; }
        //public DbSet<BatchAnnouncement> BatchAnnouncement { get; set; }
        //public DbSet<Attachment> Attachment { get; set; }
        //public DbSet<CentralBookLibrary> CentralBookLibrary { get; set; }
        //public DbSet<LCMS> LCMS { get; set; }
        //public DbSet<ScormVars> ScormVars { get; set; }
        //public DbSet<BookCategory> BookCategory { get; set; }
        //public DbSet<CourseRetrainingHistory> CourseRetrainingHistory { get; set; }
        public DbSet<CourseModuleAssociation> CourseModuleAssociation { get; set; }
        //public DbSet<ModuleLevelPlanning> ModuleLevelPlanning { get; set; }
        //public DbSet<ModuleLevelPlanningDetail> ModuleLevelPlanningDetail { get; set; }
        //public DbSet<BatchesFormation> BatchesFormation { get; set; }
        //public DbSet<BatchesFormationDetail> BatchesFormationDetail { get; set; }
        //public DbSet<TrainingExpenses> TrainingExpenses { get; set; }
        //public DbSet<TrainingExpensesDetail> TrainingExpensesDetail { get; set; }
        public DbSet<AccessibilityRule> AccessibilityRule { get; set; }
        public DbSet<ApplicabilityGroupTemplate> ApplicabilityGroupTemplate { get; set; }
        //public DbSet<Nomination> Nomination { get; set; }
        public DbSet<ModuleCompletionStatus> ModuleCompletionStatus { get; set; }
        //public DbSet<ScormVarResult> ScormVarResult { get; set; }
        //public DbSet<Assignments> Assignments { get; set; }
        //public DbSet<AssignmentsDetail> AssignmentsDetail { get; set; }
        //public DbSet<TargetSetting> TargetSetting { get; set; }
        //public DbSet<CourseCompletionStatus> CourseCompletionStatus { get; set; }
        //public DbSet<TnaEmployeeData> TnaEmployeeData { get; set; }
        //public DbSet<TnaNominateRequestData> TnaNominateRequestData { get; set; }
        //public DbSet<TrainingAttendance> TrainingAttendance { get; set; }
        //public DbSet<TrainingAttendanceDetail> TrainingAttendanceDetail { get; set; }
        //public DbSet<OfflineAssessmentScores> OfflineAssessmentScores { get; set; }
        //public DbSet<OfflineAssessmentScoresDetail> OfflineAssessmentScoresDetail { get; set; }
        //public DbSet<LcmsQuestionAssociation> LcmsQuestionAssociation { get; set; }
        //public DbSet<AssessmentConfiguration> AssessmentConfiguration { get; set; }
        //public DbSet<GradingRules> GradingRules { get; set; }
        //public DbSet<SubjectiveAssessmentStatus> SubjectiveAssessmentStatus { get; set; }
        //public DbSet<AnswerSheetsEvaluation> AnswerSheetsEvaluation { get; set; }
        //public DbSet<AssessmentQuestion> AssessmentQuestion { get; set; }
        //public DbSet<AssessmentQuestionOption> AssessmentQuestionOption { get; set; }
        //public DbSet<AssessmentQuestionRejected> AssessmentQuestionRejected { get; set; }
        //public DbSet<AssessmentSheetConfiguration> AssessmentSheetConfiguration { get; set; }
        //public DbSet<AssessmentSheetConfigurationDetails> AssessmentSheetConfigurationDetails { get; set; }
        //public DbSet<AssessmentQuestionDetails> AssessmentQuestionDetails { get; set; }
        //public DbSet<PostAssessmentResult> PostAssessmentResult { get; set; }
        //public DbSet<CommonSmileSheet> CommonSmileSheet { get; set; }
        //public DbSet<TrainerFeedback> TrainerFeedback { get; set; }
        //public DbSet<FeedbackQuestion> FeedbackQuestion { get; set; }
        //public DbSet<FeedbackOption> FeedbackOption { get; set; }
        //public DbSet<FeedbackStatus> FeedbackStatus { get; set; }
        //public DbSet<FeedbackStatusDetail> FeedbackStatusDetail { get; set; }
        //public DbSet<FeedbackQuestionRejected> FeedbackQuestionRejected { get; set; }
        //public DbSet<FeedbackSheetConfiguration> FeedbackSheetConfiguration { get; set; }
        //public DbSet<FeedbackSheetConfigurationDetails> FeedbackSheetConfigurationDetails { get; set; }
        //public DbSet<ContentCompletionStatus> ContentCompletionStatus { get; set; }
        //public DbSet<CompetencyCategory> CompetencyCategory { get; set; }
        //public DbSet<CompetencySubCategory> CompetencySubCategory { get; set; }
        //public DbSet<CompetencySubSubCategory> CompetencySubSubCategory { get; set; }
        //public DbSet<CompetencyReviewParameters> CompetencyReviewParameters { get; set; }
        //public DbSet<CompetencyReviewParametersOptions> CompetencyReviewParametersOptions { get; set; }
        //public DbSet<CompetencyReviewParametersAssessment> CompetencyReviewParametersResult { get; set; }
        //public DbSet<CompetenciesMaster> CompetenciesMaster { get; set; }
        //public DbSet<CompetencyLevels> CompetencyLevels { get; set; }
        //public DbSet<CompetenciesMapping> CompetenciesMapping { get; set; }
        //public DbSet<RolewiseCompetenciesMapping> RolewiseCompetenciesMapping { get; set; }
        //public DbSet<CourseReview> CourseReview { get; set; }
        //public DbSet<CourseRating> CourseRating { get; set; }
        public DbSet<CustomerConnectionString> CustomerConnectionString { get; set; }
        //public DbSet<DiscussionForum> DiscussionForum { get; set; }
        public DbSet<SubCategory> SubCategory { get; set; }
        //public DbSet<CourseCode> CourseCode { get; set; }
        public DbSet<ILTSchedule> ILTSchedule { get; set; }
        //public DbSet<Section> Section { get; set; }
        //public DbSet<Faq> Faq { get; set; }
        //public DbSet<UserMemo> UserMemo { get; set; }
        public DbSet<TrainingNomination> TrainingNomination { get; set; }
        public DbSet<AcademyAgencyMaster> AcademyAgencyMaster { get; set; }
        public DbSet<AgencyMaster> AgencyMaster { get; set; }
        public DbSet<ILTTrainingAttendance> ILTTrainingAttendance { get; set; }
        //public DbSet<TrendingCourse> TrendingCourse { get; set; }
        public DbSet<ILTRequestResponse> ILTRequestResponse { get; set; }
        public DbSet<ScheduleCode> ScheduleCode { get; set; }
        public DbSet<ILTScheduleTrainerBindings> ILTScheduleTrainerBindings { get; set; }
        //public DbSet<DegreedContent> DegreedContent { get; set; }
        //public DbSet<ToDoPriorityList> ToDoPriorityList { get; set; }
        //public DbSet<ExceptionLog> ExceptionLog { get; set; }
        //public DbSet<EBTDetails> EBTDetails { get; set; }
        //public DbSet<UserCoursesStatistics> UserCoursesStatistics { get; set; }
        public DbSet<AccessibilityRuleRejected> AccessibilityRuleRejected { get; set; }
        //public DbSet<UserCourseStatisticsDetails> UserCourseStatisticsDetails { get; set; }
        public DbSet<CourseRequest> CourseRequest { get; set; }
        //public DbSet<CoursesRequestDetails> CoursesRequestDetails { get; set; }
        //public DbSet<TNAYear> TNAYear { get; set; }
        //public DbSet<AuthoringMaster> AuthoringMaster { get; set; }
        //public DbSet<CompetencyJobRole> CompetencyJobRole { get; set; }
        //public DbSet<AuthoringMasterDetails> AuthoringMasterDetails { get; set; }
        //public DbSet<AuthoringInteractiveVideoPopups> AuthoringInteractiveVideoPopups { get; set; }
        //public DbSet<AuthoringInteractiveVideoPopupsOptions> AuthoringInteractiveVideoPopupsOptions { get; set; }
        //public DbSet<AuthoringInteractiveVideoPopupsHistory> AuthoringInteractiveVideoPopupsHistory { get; set; }
        //public DbSet<CourseScheduleEnrollmentRequest> CourseScheduleEnrollmentRequest { get; set; }
        //public DbSet<TempCourseScheduleEnrollmentRequest> TempCourseScheduleEnrollmentRequest { get; set; }
        //public DbSet<CourseScheduleEnrollmentRequestDetails> CourseScheduleEnrollmentRequestDetails { get; set; }
        //public DbSet<FinancialYear> FinancialYear { get; set; }
        //public DbSet<Lesson> Lesson { get; set; }
        public DbSet<Tokens> Tokens { get; set; }
        public DbSet<BBBMeeting> BBBMeeting { get; set; }
        public DbSet<GoToMeetingDetails> GoToMeetingDetails { get; set; }
        public DbSet<ZoomMeetingDetails> ZoomMeetingDetails { get; set; }
        public DbSet<ILTOnlineSetting> ILTOnlineSetting { get; set; }
        public DbSet<TeamsScheduleDetails> TeamsScheduleDetails { get; set; }
        //public DbSet<BespokeRequest> BespokeRequest { get; set; }
        //public DbSet<BespokeParticipants> BespokeParticipants { get; set; }
        //public DbSet<CertificateTemplates> CertificateTemplates { get; set; }
        //public DbSet<CertificateDownloadDetails> CertificateDownloadDetails { get; set; }
        public DbSet<UserOTPBindings> UserOTPBindings { get; set; }
        //public DbSet<CourseWiseEmailReminder> CourseWiseEmailReminder { get; set; }
        //public DbSet<UserWiseCourseEmailReminder> UserWiseCourseEmailReminder { get; set; }
        //public DbSet<CourseWiseSMSReminder> CourseWiseSMSReminder { get; set; }
        //public DbSet<UserWiseFeedbackAggregation> UserWiseFeedbackAggregation { get; set; }
        //public DbSet<CoursesEnrollRequest> CoursesEnrollRequest { get; set; }
        //public DbSet<CoursesEnrollRequestDetails> CoursesEnrollRequestDetails { get; set; }
        public DbSet<ILTTrainingAttendanceDetails> ILTTrainingAttendanceDetails { get; set; }
        public DbSet<ScheduleHolidayDetails> ScheduleHolidayDetails { get; set; }
        //public DbSet<AssignmentDetails> AssignmentDetails { get; set; }
        //public DbSet<AssignmentDetailsRejected> AssignmentDetailsRejected { get; set; }
        //public DbSet<AssessmentAttemptManagement> AssessmentAttemptManagement { get; set; }
        //public DbSet<CourseCertificateAssociation> CourseCertificateAssociation { get; set; }
        public DbSet<TopicMaster> TopicMaster { get; set; }
        public DbSet<ModuleTopicAssociation> ModuleTopicAssociation { get; set; }
        public DbSet<TeamsAccessToken> TeamsAccessToken { get; set; }
        public DbSet<ConfigurableValues> ConfigurableValues { get; set; }
        public DbSet<ApiConfigurableParameters> configurableParameters { get; set; }
        public DbSet<UserWebinarMaster> UserWebinarMasters { get; set; }
       // public DbSet<ModuleLcmsAssociation> ModuleLcmsAssociation { get; set; }
        public DbSet<AttendanceDataForCANH> AttendanceDataForCANH { get; set; }
        //public DbSet<RoleCompetency> RoleCompetency { get; set; }
        public DbSet<TrainingNominationRejected> TrainingNominationRejected { get; set; }
        //public DbSet<CareerJobRoles> CareerJobRoles { get; set; }
        //public DbSet<NextJobRoles> NextJobRoles { get; set; }
        //public DbSet<AssessmentCompetenciesMapping> AssessmentCompetenciesMapping { get; set; }
        //public DbSet<UserPrefferedCourseLanguage> UserPrefferedCourseLanguage { get; set; }
        //public DbSet<RolewiseCourseMapping> RolewiseCourseMapping { get; set; }
        //public DbSet<CourseCertificateAuthority> CourseCertificateAuthority { get; set; }
        //public DbSet<PMSEvaluationResult> PMSEvaluationResult { get; set; }

        //public DbSet<ProcessEvaluationQuestion> ProcessEvaluationQuestion { get; set; } 
        //public DbSet<PMSEvaluationQuestion> PMSEvaluationQuestion { get; set; }
        //public DbSet<PMSEvaluationSubmit> PMSEvaluationSubmit { get; set; }
        //public DbSet<PMSEvaluationPoint> PMSEvaluationPoints { get; set; }

        //public DbSet<OEREvaluationQuestion> OEREvaluationQuestion { get; set; }
        //public DbSet<ProcessEvaluationOption> ProcessEvaluationOption { get; set; }
        //public DbSet<OEREvaluationOption> OEREvaluationOption { get; set; }
        //public DbSet<KitchenAuditOption> KitchenAuditOption { get; set; }
        //public DbSet<KitchenAuditQuestion> KitchenAuditQuestion { get; set; }
        //public DbSet<ProcessEvaluationManagement> ProcessEvaluationManagement { get; set; }
        //public DbSet<ProcessConfiguration> ProcessConfiguration { get; set; }
        //public DbSet<ProcessResult> ProcessResult { get; set; }
        //public DbSet<OERProcessResult> OERProcessResult { get; set; }
        //public DbSet<KitchenAuditResult> KitchenAuditResult { get; set; }
        //public DbSet<ProcessResultDetails> ProcessResultDetails { get; set; }
        //public DbSet<OERProcessResultDetails> OERProcessResultDetails { get; set; }
        //public DbSet<KitchenAuditResultDetails> KitchenAuditResultDetails { get; set; }
        //public DbSet<CourseLog> CourseLog { get; set; }
        //public DbSet<Log_ClearBookMarking> Log_ClearBookMarking { get; set; }

        public DbSet<BatchCode> BatchCode { get; set; }
        public DbSet<ILTBatch> ILTBatch { get; set; }
        public DbSet<ILTBatchRegionBindings> ILTBatchRegionBindings { get; set; }
        public DbSet<Configure2> Configure2 { get; set; }
        public DbSet<Configure11> Configure11 { get; set; }
        public DbSet<ILTBatchRejected> ILTBatchRejected { get; set; }
        public DbSet<ILTScheduleRejected> ILTScheduleRejected { get; set; }
        //public DbSet<Model.ThirdPartyIntegration.AlisonCourseUserReport> AlisonCourseUserReport { get; set; }
        //public DbSet<CourseCompletionStatusHistory> CourseCompletionStatusHistory { get; set; }
        //public DbSet<CompetencyJdUpload> competencyJdUpload { get; set; }
        //public DbSet<NodalCourseRequests> NodalCourseRequests { get; set; }
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<UserMasterDetails> UserMasterDetails { get; set; }
        //public DbSet<NodalUserGroups> NodalUserGroups { get; set; }
        //public DbSet<CourseMasterAuditLog> CourseMasterAuditLog { get; set; }
        //public DbSet<CertificationUpload> CertificationUpload { get; set; }
        //public DbSet<TrainingDetailsCatalog> TrainingDetailsCatalog { get; set; }
        public DbSet<UserGroup> UserGroup { get; set; }
        //public DbSet<Group> Group { get; set; }
        //public DbSet<Business> Business { get; set; }
        public DbSet<AccebilityRuleUserGroup> AccebilityRuleUserGroup { get; set; }
        public DbSet<SkillSoft> SkillSoft { get; set; }
        //public DbSet<CriticalAuditQuestion> CriticalAuditQuestion { get; set; }
        //public DbSet<CriticalAuditOption> CriticalAuditOption { get; set; }
        //public DbSet<CriticalAuditProcessResult> CriticalAuditProcessResult { get; set; }
        //public DbSet<CriticalAuditProcessResultDetails> CriticalAuditProcessResultDetails { get; set; }
        //public DbSet<NightAuditQuestion> NightAuditQuestion { get; set; }
        //public DbSet<NightAuditOption> NightAuditOption { get; set; }
        //public DbSet<NightAuditProcessResult> NightAuditProcessResult { get; set; }
        //public DbSet<NightAuditProcessResultDetails> NightAuditProcessResultDetails { get; set; }

        //public DbSet<OpsAuditQuestion> OpsAuditQuestion { get; set; }
        //public DbSet<OpsAuditOption> OpsAuditOption { get; set; }
        //public DbSet<OpsAuditProcessResult> OpsAuditProcessResult { get; set; }
        //public DbSet<OpsAuditProcessResultDetails> OpsAuditProcessResultDetails { get; set; }

     
        public DbSet<EdCastTransactionDetails> EdCastTransactionDetails { get; set; }
        public DbSet<ScheduleVisibilityRule> ScheduleVisibilityRule { get;  set; }
        public DbSet<UserTeams> UserTeams { get; set; }
        public DbSet<UserTeamsMapping> UserTeamsMapping { get; set; }
        //public DbSet<DevelopmentPlanForCourse> DevelopmentPlanForCourse { get; set; }
        //public DbSet<CourseMappingToDevelopment> CourseMappingToDevelopment { get; set; }
        //public DbSet<UserDevelopmentPlanMapping> UserDevelopmentPlanMapping { get; set; }
        //public DbSet<CourseGroupMapping> CourseGroupMapping { get; set; }
        //public DbSet<CourseGroup> CourseGroup { get; set; }
        //public  DbSet<CourseVendorDetail> CourseVendorDetail { get; set; }
        //public DbSet<DarwinboxTransactionDetails> DarwinboxTransactionDetails { get; set; }
        //public DbSet<DarwinboxConfiguration> DarwinboxConfiguration { get; set; }
        //public DbSet<TrainingReommendationNeeds> TrainingReommendationNeeds { get; set; }
        //public DbSet<DevelopementPlanCode> DevelopementPlanCode { get; set; }
        //public DbSet<AdditionalResourceForCourse> AdditionalResourceForCourse { get; set; }
        public DbSet<EdCastConfiguration> EdCastConfiguration { get; set; }
        //public DbSet<DistributedAdminFieldAssociation> DistributedAdminFieldAssociation { get; set; }
        //public DbSet<VimeoConfiguration> VimeoConfiguration { get; set; }
        //public DbSet<CourseAuthorAssociation> CourseAuthorAssociation { get; set; }
        //public DbSet<ExternalCourseCategory> ExternalCourseCategory { get; set; }
        //public DbSet<ZobbleCourseDetails> ZobbleCourseDetails { get; set; }
        //public DbSet<SubSubCategory> SubSubCategory { get; set; }
 
        //public DbSet<ExternalCourseCategoryAssociation> ExternalCourseCategoryAssociation { get; set; }
        //public DbSet<ExternalCoursesConfiguration> ExternalCoursesConfiguration { get; set; }
        //public DbSet<ExternalTrainingRequest> ExternalTrainingRequest { get; set; }
        //public DbSet<Course_Details> CourseDetails { get; set; }
        //public DbSet<BasicAuthCredentials> BasicAuthCredentials { get; set; }
        //public DbSet<CourseInstructor> CourseInstructor { get; set; }
        //public DbSet<CourseOwner> CourseOwner { get; set; }
        //public DbSet<CourseCompletionMailReminder> CourseCompletionMailReminder { get; set; }
        //public DbSet<InvoicePaymentResponse> InvoicePaymentResponse { get; set; }

        public DbSet<ClientUserApiCount> ClientUserApiCount { get; set; }

        //public DbSet<CameraPhotos> CameraPhotos { get; set; }
        //public DbSet<CameraEvaluation> CameraEvaluation { get; set; }

        public DbSet<GoogleMeetDetails> GoogleMeetDetails { get; set; }
        //public DbSet<OpenAIQuestion> OpenAIQuestion { get; set; }
        //public DbSet<OpenAICourseQuestionAssociation> OpenAICourseQuestionAssociation { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                string ConnectionString = null;
                if (this._httpContext != null)
                {
                    var httpContext = this._httpContext.HttpContext;
                    if (httpContext != null)
                    {
                        string encryptedConnectionString = httpContext.User.FindFirst("address") == null ? null : httpContext.User.FindFirst("address").Value;
                        if (!string.IsNullOrEmpty(encryptedConnectionString))
                        {
                            ConnectionString = Security.Decrypt(encryptedConnectionString);
                            optionsBuilder
                            .UseSqlServer(ConnectionString, opt => opt.UseRowNumberForPaging())
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                            base.OnConfiguring(optionsBuilder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Course");

            modelBuilder.Entity<ILT.API.Model.Course>().Property(x => x.RowGuid).HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<ILT.API.Model.AdministrativeFunctions.ModuleLevelPlanningDetail>()
               .HasOne(p => p.ModuleLevelPlannings)
               .WithMany(b => b.ModuleLevelPlanningDetails)
               .HasForeignKey(p => p.ModuleLevelPlanningId);

            modelBuilder.Entity<ILT.API.Model.AdministrativeFunctions.BatchesFormationDetail>()
              .HasOne(p => p.BatchesFormations)
              .WithMany(b => b.BatchesFormationDetails)
              .HasForeignKey(p => p.BatchesFormationId);

            modelBuilder.Entity<ILT.API.Model.AdministrativeFunctions.TrainingExpensesDetail>()
             .HasOne(p => p.TrainingExpense)
             .WithMany(b => b.TrainingExpensesDetails)
             .HasForeignKey(p => p.TrainingExpenseId);

            modelBuilder.Entity<ILT.API.Model.ActivitiesManagement.AssignmentsDetail>()
             .HasOne(p => p.Assignment)
             .WithMany(b => b.AssignmentsDetails)
             .HasForeignKey(p => p.AssignmentId);

            //modelBuilder.Entity<ILT.API.Model.TrainersFunctions.TrainingAttendanceDetail>()
            //.HasOne(p => p.TrainingAttendances)
            //.WithMany(b => b.TrainingAttendanceDetails)
            //.HasForeignKey(p => p.TrainingAttendanceId);

            //modelBuilder.Entity<ILT.API.Model.TrainersFunctions.OfflineAssessmentScoresDetail>()
            //.HasOne(p => p.OfflineAssessmentScore)
            //.WithMany(b => b.OfflineAssessmentScoresDetails)
            //.HasForeignKey(p => p.OfflineAssessmentScoresId);

            modelBuilder.Entity<ConfigurableValues>().ToTable("ConfigurableValues", "Masters");
            modelBuilder.Entity<UserWebinarMaster>().ToTable("UserWebinarMaster", "User");
            //modelBuilder.Entity<ApiConfigurableParameters>().ToTable("ConfigurableParameter", "Masters");
            //modelBuilder.Entity<Group>().ToTable("Group", "User");
            //modelBuilder.Entity<Business>().ToTable("Business", "User");
            //modelBuilder.Entity<UserDevelopmentPlanMapping>().ToTable("UserDevelopmentPlanMapping", "User");
            //modelBuilder.Entity<BasicAuthCredentials>().ToTable("BasicAuthCredentials", "User");
            modelBuilder.Entity<ClientUserApiCount>().ToTable("ClientUserApiCount", "dbo");

        }

        public static class DbContextFactory
        {
            public static Dictionary<string, string> ConnectionStrings { get; set; }

            public static void SetConnectionString(Dictionary<string, string> connStrs)
            {
                ConnectionStrings = connStrs;
            }

            public static CourseContext Create(string connectionString)
            {
                var optionsBuilder = new DbContextOptionsBuilder<CourseContext>();
                optionsBuilder.UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new CourseContext(optionsBuilder.Options);
            }
        }
        [DbFunction(Schema = "dbo")]
        public static int func_IsCourseCompleted(int courseId, int userId)
        {
            throw new Exception();
        }

        [DbFunction(Schema = "dbo")]
        public static DateTime GetCourseAssignedDateForRewardPoints(int userId, int courseId)
        {
            throw new Exception();
        }

    }
}
