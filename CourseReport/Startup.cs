using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CourseReport.API.Data;
using CourseReport.API.Helper;
using CourseReport.API.Repositories;
using CourseReport.API.Repositories.Interface;
using CourseReport.API.Service;
using CourseReport.API.Service.Interface;
using Swashbuckle.AspNetCore.Swagger;
using System;
using CourseReport.API.Service;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using CourseReport.API.Middleware;
using AspNetCoreRateLimit;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Repositories;
using CourseReport.API.Helper.Log_API_Count;
using CourseReport.API.Helper.Interfaces;

namespace CourseReport.API
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
            try
            {
                services.AddOptions();
                services.AddMemoryCache();
                services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimit"));
                services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
                services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
                services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

                ConfigureAuthService(services);
                string connection = Configuration.GetConnectionString("DefaultConnection");
                // services.AddDbContext<ReportDbContext>(options => options.UseSqlServer(connection, opt => opt.UseRowNumberForPaging())
                services.AddDbContext<ReportDbContext>(options => options.UseSqlServer(connection)
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

                //services.AddScoped<IILTDashboardReportExport, PostILTDashboardReportExport>();
                //services.AddScoped<ICourseModuleReportExport, PostCourseModuleReportExport>();
                //services.AddScoped<ICourseAssessmentReportExport, PostCourseAssessmentReportExport>();
                //services.AddScoped<IUserLoginHistoryReportExport, PostUserLoginHistoryReportExport>();
                //services.AddScoped<IILTUserDetailsReportExport, PostILTUserDetailsReportStatus>();
                //services.AddScoped<IUserWiseCourseCompletionReportStatus, PostUserWiseCourseCompletionReportStatus>();
                //services.AddScoped<IAllUsersActivityReportStatus, PostAllUsersActivityReportStatus>();
                //services.AddScoped<ISpecificReportTLSRepository, SpecificReportTLSRepository>();
                //services.AddScoped<ISpecificReportTLSService, SpecificReportTLSService>();
                //services.AddScoped<IStatisticsReportRepository, StatisticsReportRepository>();
                //services.AddScoped<IStatisticsReportService, StatisticsReportService>();
                //services.AddScoped<IILTRepository, ILTRepository>();
                //services.AddScoped<IILTService, ILTService>();
                //services.AddScoped<IUserWiseLoginDetailsRepository, UserWiseLoginDetailsRepository>();
                //services.AddScoped<IUserWiseLoginDetailsService, UserWiseLoginDetailsService>();
                //services.AddScoped<ISurveyReportRepository, SurveyReportRepository>();
                //services.AddScoped<ISurveyReportService, SurveyReportService>();
                //services.AddScoped<IFeedbackReportRepository, FeedbackReportRepository>();
                //services.AddScoped<IFeedbackReportService, FeedbackReportService>();
                //services.AddScoped<ICertificateReportRepository, CertificateReportRepository>();
                //services.AddScoped<ICertificateDownloadReportService, CertificateDownloadReportService>();
                //services.AddScoped<IReportRepository, ReportRepository>();
                //services.AddScoped<IReportService, ReportService>();
                //services.AddScoped<ICourseMasterAuditLogService, CourseMasterAuditLogService>();
                //services.AddScoped<ICourseMasterAuditLogRepository, CourseMasterAuditLogRepository>();
                //services.AddScoped<ICalendarService, CalendarService>();
                services.AddScoped<ICalendarRepository, CalendarRepository>();
                services.AddScoped<ITLSHelper, TLSHelper>();
                services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
               // services.AddScoped<IDataMigrationReport, DataMigrationReportRepository>();
                services.AddScoped<ITokensRepository, TokensRepository>();
                services.AddTransient<IIdentityService, IdentityService>();
                services.AddTransient<ICourseReportRepository, CourseReportRepository>();
                services.AddTransient<ICourseReportService, CourseReportService>();
                //services.AddTransient<ITNAReportRepository, TNAReportRepository>();
                //services.AddTransient<ITNAReportService, TNAReportService>();
                //services.AddTransient<IIntegrationRepository, IntegrationRepository>();
                //services.AddTransient<IIntegrationService, IntegrationService>();
                services.AddTransient<ISchedulerRepository, SchedulerRepository>();
                services.AddTransient<ISchedulerService, SchedulerService>();
                //services.AddTransient<IDealerRepository, DealerRepository>();
                //services.AddTransient<IDealerService, DealerService>();
                //services.AddTransient<IAnalyticalDashboardRepository, AnalyticalDashboardRepository>();
                //services.AddTransient<IAnalyticalDashboardService, AnalyticalDashboardService>();
                //services.AddTransient<IWNSReportRepository, WNSReportRepository>();
                //services.AddTransient<ICompetencyRepository, CompetencyRepository>();
                //services.AddTransient<IFileOperations, FileOperations>();
                //services.AddTransient<IWNSReportService, WNSReportService>();

                services.AddScoped<IToDataTableConverter, ToDataTableConverter>();
                services.AddSingleton<IAzureStorage, AzureStorage>();


                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CourseReport API", Version = "v1" });
                });
                services.AddCors();
                services.AddMvc(option => option.EnableEndpointRouting = false)
                     .AddJsonOptions(options =>
                     {
                         //options.SerializerSettings.Formatting = Formatting.None;
                         options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                         options.JsonSerializerOptions.DictionaryKeyPolicy = null;
                     });
                services.AddScoped<APIRequestCount<ClientUserApiCount>>();
            }
            catch (Exception)
            {

            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            if (Configuration.GetValue<bool>("CheckRateLimit"))
                app.UseIpRateLimiting();
            app.UseCors(
                builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseAuthentication();
            app.UseSwagger();
            app.UseMiddleware<RequestResponseLogging>(); //Will be added in future for Logging Requests
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CourseReport API V1");
            });
            app.UseStaticFiles();
            app.UseMvc();
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
                    options.Audiences.Add("coursereport_api");
                    options.ClientId = "coursereport_api";
                    options.ClientSecret = "ClientSecret";
                    options.RequireHttpsMetadata = false;
                });

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }
            catch (Exception)
            {

            }
        }
    }
}
