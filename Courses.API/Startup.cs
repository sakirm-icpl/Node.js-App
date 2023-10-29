using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Helper;
using Courses.API.Middleware;
using Courses.API.Models;
using Courses.API.Repositories;
using Courses.API.Repositories.Competency;
using Courses.API.Repositories.Interfaces;
using Courses.API.Repositories.Interfaces.Competency;
using Courses.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using Courses.API.Repositories.EdCast;
using Courses.API.Repositories.Interfaces.EdCast;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Repositories;
using Courses.API.Model.Log_API_Count;
using Courses.API.Controllers;
using Courses.API.Model;

namespace Courses.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimit"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddJsonOptions(options =>
                 {
                     //options.SerializerSettings.Formatting = Formatting.None;
                     //options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                     options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                     options.JsonSerializerOptions.DictionaryKeyPolicy = null;
                 });

            //services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
            //services.AddHttpClient();

            services.AddControllers()
            .AddNewtonsoftJson();

            var connection = (new ConnStringEncDec()).GetDefaultConnectionString();

            services.AddDbContext<CourseContext>(options =>
            options.UseSqlServer(connection) 
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
            //services.AddScoped<IDataMigration, DataMigrationRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<ICourseModuleAssociationRepository, CourseModuleAssociationRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            //services.AddScoped<IAdditionalLearning, AdditionalLearningRepository>();
            //services.AddScoped<ITrainingPlaceRepository, TrainingPlaceRepository>();
            //services.AddScoped<IBatchAnnouncementRepository, BatchAnnouncementRepository>();
            //services.AddScoped<ICentralBookLibraryRepository, CentralBookLibraryRepository>();
            services.AddScoped<IAttachmentRepository, AttacmentRepository>();
            //services.AddScoped<IScormVarRepository, ScormVarRepository>();
            services.AddScoped<ILCMSRepository, LCMSRepositrory>();
            services.AddScoped<IMyCoursesRepository, MyCoursesRepository>();
            //services.AddScoped<IBookCategoryRepository, BookCategoryRepository>();
            //services.AddScoped<IModuleLevelPlanningRepository, ModuleLevelPlanningRepository>();
            //services.AddScoped<IModuleLevelPlanningDetailRepository, ModuleLevelPlanningDetailRepository>();
            //services.AddScoped<IBatchesFormationRepository, BatchesFormationRepository>();
            //services.AddScoped<IBatchesFormationDetailRepository, BatchesFormationDetailRepository>();
            //services.AddScoped<ITrainingExpensesRepository, TrainingExpensesRepository>();
            //services.AddScoped<ITrainingExpensesDetailRepository, TrainingExpensesDetailRepository>();
            services.AddScoped<IAccessibilityRule, AccessibiltyRuleRepository>();
            //services.AddScoped<IAccebilityRuleUserGroup, AccebilityRuleUserGroupRepository>();
            //services.AddScoped<IUserGroup, UserGroupRepository>();
            //services.AddScoped<IAssignmentsRepository, AssignmentsRepository>();
            //services.AddScoped<IAssignmentsDetailRepository, AssignmentsDetailRepository>();
            //services.AddScoped<IModuleCompletionStatusRepository, ModuleCompletionStatusRepository>();
            //services.AddScoped<ITargetSettingRepository, TargetSettingRepository>();
            //services.AddScoped<IScormVarResultRepository, ScormVarResultRepository>();
            services.AddTransient<IIdentityService, IdentityService>();
            //services.AddScoped<ITrainingAttendanceRepository, TrainingAttendanceRepository>();
            //services.AddScoped<ITrainingAttendanceDetailRepository, TrainingAttendanceDetailRepository>();
            //services.AddScoped<IOfflineAssessmentScoresRepository, OfflineAssessmentScoresRepository>();
            //services.AddScoped<IOfflineAssessmentScoresDetailRepository, OfflineAssessmentScoresDetailRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddScoped<ICourseCompletionStatusRepository, CourseCompletionStatusRepository>();
            services.AddScoped<ILcmsQuestionAssociation, LcmsQuestionAssociationRepository>();
            //services.AddScoped<IAssessmentConfiguration, AssessmentConfigurationRepositories>();
            //services.AddScoped<IGradingRules, GradingRulesRepository>();
            //services.AddScoped<IPostAssessmentResult, PostAssessmentResultRepository>();
            //services.AddScoped<IAssessmentQuestionDetails, AssessmentQuestionDetailsRepository>();
            //services.AddScoped<ISubjectiveAssessmentStatus, SubjectiveAssessmentStatusRepository>();
            //services.AddScoped<IAnswerSheetsEvaluationRepository, AnswerSheetsEvaluationRepository>();
            //services.AddScoped<IAssessmentQuestion, AssessmentQuestionRepository>();
            //services.AddScoped<IAsessmentQuestionOption, AsessmentQuestionOptionBankRepository>();
            //services.AddScoped<IAssessmentConfigurationSheets, AssessmentSheetConfigurationRepository>();
            //services.AddScoped<IAssessmentQuestionRejectedRepository, AssessmentQuestionRejectedRepository>();
            //services.AddScoped<IAssessmentSheetConfigurationDetails, AssessmentSheetConfigurationDetailsRepository>();
            //services.AddScoped<ITrainerFeedbackRepository, TrainerFeedbackRepository>();
            //services.AddScoped<IFeedbackStatusDetail, FeedbackStatusDetailRepository>();
            //services.AddScoped<IFeedbackOption, FeedbackOptionRepository>();
            //services.AddScoped<IFeedbackQuestion, FeedbackQuestionRepository>();
            //services.AddScoped<IFeedbackStatus, FeedbackStatusRepository>();
            //services.AddScoped<IFeedbackQuestionRejectedRepository, FeedbackQuestionRejectedRepository>();
            //services.AddScoped<IFeedbackSheetConfiguration, FeedbackSheetConfigurationRepository>();
            //services.AddScoped<IFeedbackSheetConfigurationDetails, FeedbackSheetConfigurationDetailsRepository>();
            //services.AddScoped<IContentCompletionStatus, ContentCompletionStatusRepository>();
            //services.AddScoped<ICompetencyCategoryRepository, CompetencyCategoryRepository>();
            services.AddScoped<ICompetenciesMasterRepository, CompetenciesMasterRepository>();
            services.AddScoped<ICompetencyLevelsRepository, CompetencyLevelsRepository>();
            services.AddScoped<ICompetenciesMappingRepository, CompetenciesMappingRepository>();
            //services.AddScoped<IRolewiseCompetenciesMappingRepository, RolewiseCompetenciesMappingRepository>();
            //services.AddScoped<ICommonSmile, CommonSmileRepository>();
            //services.AddScoped<ICourseReviewRepository, CourseReviewRepository>();
            //services.AddScoped<ICourseRatingRepository, CourseRatingRepository>();
            //services.AddScoped<ICommonSmile, CommonSmileRepository>();
            //services.AddScoped<IDiscussionForumRepository, DiscussionForumRepository>();
            services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
            services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
            services.AddScoped<ISubSubCategoryRepository, SubSubCategoryRepository>();
            services.AddScoped<INotification, NotificationRepository>();
            //services.AddScoped<IILTSchedule, ILTScheduleRepository>();
            services.AddScoped<ISectionRepository, SectionRepository>();
            services.AddScoped<IFaqRepository, FaqRepository>();
            //services.AddScoped<IMemoRepository, MemoRepository>();
            //services.AddScoped<ITrainingNomination, TrainingNominationRepository>();
            services.AddScoped<IRewardsPointRepository, RewardsPointRepository>();
            //services.AddScoped<IILTTrainingAttendance, ILTTrainingAttendanceRepository>();
            //services.AddScoped<IILTRequestResponse, ILTRequestResponseRepository>();
            services.AddScoped<IGamificationRepository, GamificationRepository>();
            //services.AddScoped<IToDoPriorityList, ToDoPriorityListRepository>();
            services.AddTransient<IEmail, EmailRepository>();
            services.AddTransient<ISMS, SMSRepository>();
            //services.AddTransient<IExceptionRepository, ExceptionRepository>();
            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<IAccessibilityRuleRejectedRepository, AccessibilityRuleRejectedRepository>();
            //services.AddScoped<ICourseRequest, CourseRequestRepository>();
            services.AddScoped<ITLSHelper, TLSHelper>();
            //services.AddScoped<ITNACourseRequest, TNACourseRequestRepository>();
            //services.AddScoped<IApplicabilityGroupTemplate, ApplicabilityGroupTemplateRepository>();
            //services.AddScoped<ILessonRepository, LessonRepository>();
            services.AddScoped<IAuthoringMaster, AuthoringMasterRepository>();
            //services.AddScoped<IJobRoleRepository, JobRoleRepository>();
            //services.AddScoped<ICourseScheduleEnrollmentRequest, CourseScheduleEnrollmentRequestRepository>();
            //services.AddScoped<IFinancialYearRepository, FinancialYearRepository>();
            //services.AddScoped<ICourseApplicability, CheckApplicabilityRepository>();
            services.AddScoped<ITokensRepository, TokensRepository>();
            //services.AddScoped<IBespokeTrainingRequestRepository, BespokeTrainingRequestRepository>();
            //services.AddScoped<ICertificateTemplatesRepository, CertificateTemplatesRepository>();
            //services.AddScoped<IAssessmentAttemptManagement, AssessmentAttemptManagementRepository>();
            //services.AddScoped<ICoursesEnrollRequestRepository, CoursesEnrollRequestRepository>();
            //services.AddScoped<ICoursesEnrollRequestDetailsRepository, CoursesEnrollRequestDetailsRepository>();
            //services.AddScoped<IAssignmentDetailsRepository, AssignmentDetailsRepository>();
            //services.AddScoped<ITopicMaster, TopicMasterRepository>();
            //services.AddScoped<IModuleTopicAssociation, ModuleTopicAssociationRepository>();
            services.AddScoped<ICourseCertificateAssociationRepository, CourseCertificateAssociationRepository>();
            //services.AddScoped<IBespokeEnrollmentRequest, BespokeEnrollmentRequestRepository>();
            //services.AddScoped<IRoleCompetenciesRepository, RoleCompetenciesRepository>();
            //services.AddScoped<ICompetenciesAssessmentMappingRepository, CompetenciesAssessmentMappingRepository>();
            //services.AddScoped<IRolewiseCoursesMapping, RolewiseCoursesMappingRepository>();
            //services.AddScoped<IProcessEvaluationQuestion, ProcessEvaluationRepository>();
            //services.AddScoped<IILTOnlineSetting, ILTOnlineSettingRepository>();
            //services.AddScoped<IILTBatchRepository, ILTBatchRepository>();
            //services.AddScoped<IAlisonRepository, AlisonRepository>();
            //services.AddScoped<IJdUploadRepository, JdUploadRepository>();
            services.AddScoped<INodalCourseRequestsRepository, NodalCourseRequestsRepository>();
            //services.AddScoped<ISkillSoftRepository, SoftSkillRepository>();
            services.AddScoped<IEdCastTransactionDetails, EdCastTransactionDetailsRepository>();
            //services.AddScoped<IScheduleVisibilityRule, ScheduleVisibilityRuleRepository>();
            services.AddScoped<IDevelopmentPlanRepository, DevelopmentPlanRepository>();
            services.AddScoped<ICourseGroupMappingRepository, CourseGroupMappingRepository>();
            //services.AddScoped<ICourseVendor, CourseVendorRepository>();
            services.AddScoped<IDarwinboxTransactionDetails, DarwinboxTransactionDetailsRepository>();
            services.AddScoped<IDarwinboxConfiguration, DarwinboxConfigurationRepository>();
            //services.AddScoped<ILinkedinRepository, LinkedinRepository>();
            //services.AddScoped<IOnlineWebinarRepository, OnlineWebinarRepository>();
            services.AddScoped<ICourseAuthorAssociation, CourseAuthorAssociationRepository>();
            //services.AddScoped<IExternalCoursesRepository, ExternalCoursesRepository>();
            services.AddScoped<IBasicAuthRepository, BasicAuthCredentialsRepository>();
            //services.AddScoped<IEnthrallRepository, EnthrallRepository>();
            //services.AddScoped<ICompetencySubCategoryRepository, CompetencySubCategoryRepository>();
            //services.AddScoped<ICompetencySubSubCategoryRepository, CompetencySubSubCategoryRepository>();
            //services.AddScoped<ICompetencyReviewParametersRepository, CompetencyReviewParametersRepository>();
            services.AddScoped<IExternalTrainingRequest, ExternalTrainingRequestRepository>();
            services.AddScoped<ICourseCompletionMailReminder, CourseCompletionMailReminderRepository>();
            services.AddScoped<IOpenAIRepository, OpenAIRepository>();
            services.AddScoped<IOpenAICourseQuestionAssociationRepository, OpenAICourseQuestionAssociationRepository>();
            services.AddSingleton<IAzureStorage, AzureStorage>();
            services.Configure<FormOptions>(x => x.MultipartBodyLengthLimit = 200000000);
            ConfigureAuthService(services);

            services.AddDistributedMemoryCache();

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Courses API",
                    Description = "Courses API give access to all Courses  Web APIs"

                });
            });

            services.AddCors();
            services.AddMvc();
            services.AddScoped<APIRequestCount<ClientUserApiCount>>();
    
            //services.AddControllers(config =>
            //{
            //    config.Filters.Add(new APIRequestCount());
            //});
        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            try
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = OAuthIntrospectionDefaults.AuthenticationScheme;
                })
                .AddOAuthIntrospection(options =>
                {
                    options.Authority = new Uri(Configuration.GetValue<string>("IdentityUrl"));
                    options.Audiences.Add("course-api");
                    options.ClientId = "course-api";
                    options.ClientSecret = "ClientSecret";
                    options.RequireHttpsMetadata = false;
                });

                services.Configure<FormOptions>(Option =>
                {
                    Option.MultipartBodyLengthLimit = int.MaxValue;// 600000000;
                });

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }
            catch (Exception ex )
            {
                throw ex;
            }
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            if (Configuration.GetValue<bool>("CheckRateLimit"))
                app.UseIpRateLimiting();

            app.UseStaticFiles();
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //     Path.Combine(Directory.GetCurrentDirectory(), "LXPFiles")),
            //    RequestPath = "/ContentFiles"
            //});


            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //    Path.Combine("C:/Publish/ApiGateway", "LXPFiles")),
            //    RequestPath = "/ContentFiles"
            //});
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //    Path.Combine("C:/Publish/ApiGateway", "wwwroot")),
            //    RequestPath = "/ContentFiles"
            //});

            var cachePeriod = env.IsDevelopment() ? "600" : "604800";
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Requires the following import:
                    // using Microsoft.AspNetCore.Http;
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                }
            });

            app.UseCors(
                builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());


            app.UseRouting();
            app.UseAuthentication();
            app.UseMiddleware<RequestResponseLogging>(); //Will be added in future for Logging Requests
            app.UseSwagger();
                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Course API V1");
                });
                app.UseStaticFiles();
            app.UseMvc();
  
}
    }
}
