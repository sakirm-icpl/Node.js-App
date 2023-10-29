using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using Assessment.API.Middleware;
using Assessment.API.Helper;
using Assessment.API.Models;
using Assessment.API.APIModel;
using Assessment.API.Model.Log_API_Count;
using Assessment.API.Services;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Repositories;
using Assessment.API.Repositories.Interface;
using Assessment.API.Repositories.Competency;
using Assessment.API.Repositories.Interfaces.Competency;
using Assessment.API.Repositories.EdCast;
using Assessment.API.Repositories.Interfaces.EdCast;
using Courses.API.Repositories;
//using AzureStorageLibrary.Repositories.Interfaces;
//using AzureStorageLibrary.Repositories;



namespace Assessment.API
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

            services.AddDbContext<AssessmentContext>(options =>
            options.UseSqlServer(connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));


            services.AddTransient<IIdentityService, IdentityService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ITokensRepository, TokensRepository>();
            services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
            services.AddSingleton<IAzureStorage, AzureStorage>();
            services.AddScoped<IAssessmentQuestion, AssessmentQuestionRepository>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddScoped<IPostAssessmentResult, PostAssessmentResultRepository>();
            services.AddScoped<IAssessmentQuestionDetails, AssessmentQuestionDetailsRepository>();
            services.AddScoped<IAssessmentQuestionRejectedRepository, AssessmentQuestionRejectedRepository>();
            services.AddScoped<IAssessmentSheetConfigurationDetails, AssessmentSheetConfigurationDetailsRepository>();
            services.AddScoped<IAssessmentConfigurationSheets, AssessmentSheetConfigurationRepository>();
            services.AddScoped<IAsessmentQuestionOption, AsessmentQuestionOptionBankRepository>();
            services.AddScoped<ICompetenciesAssessmentMappingRepository, CompetenciesAssessmentMappingRepository>();
            services.AddScoped<IDarwinboxConfiguration, DarwinboxConfigurationRepository>();
            services.AddScoped<IAccessibilityRule, AccessibiltyRuleRepository>();
            services.AddScoped<IContentCompletionStatus, ContentCompletionStatusRepository>();
            services.AddScoped<ICourseCompletionStatusRepository, CourseCompletionStatusRepository>();
            services.AddScoped<ICourseModuleAssociationRepository, CourseModuleAssociationRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddTransient<IEmail, EmailRepository>();
            services.AddScoped<ILCMSRepository, LCMSRepositrory>();
            services.AddScoped<IMyCoursesRepository, MyCoursesRepository>();
            services.AddScoped<IModuleCompletionStatusRepository, ModuleCompletionStatusRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<INodalCourseRequestsRepository, NodalCourseRequestsRepository>();
            services.AddScoped<INotification, NotificationRepository>();
            services.AddScoped<IRewardsPointRepository, RewardsPointRepository>();
            services.AddScoped<ICompetenciesMasterRepository, CompetenciesMasterRepository>();
            services.AddScoped<IAnswerSheetsEvaluationRepository, AnswerSheetsEvaluationRepository>();
            services.AddScoped<ISubjectiveAssessmentStatus, SubjectiveAssessmentStatusRepository>();
            services.AddScoped<IGradingRules, GradingRulesRepository>();


            //   services.AddSingleton<IAzureStorage, AzureStorage>();
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
                    options.Audiences.Add("courseapplicability-api");
                    options.ClientId = "courseapplicability-api";
                    options.ClientSecret = "ClientSecret";
                    options.RequireHttpsMetadata = false;
                });

                services.Configure<FormOptions>(Option =>
                {
                    Option.MultipartBodyLengthLimit = int.MaxValue;// 600000000;
                });

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }
            catch (Exception ex)
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


