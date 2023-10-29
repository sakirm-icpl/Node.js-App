using AspNet.Security.OAuth.Introspection;

using AutoMapper;
using Feedback.API.Middleware;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using Feedback.API.Models;
using Feedback.API.Model.Log_API_Count;
using Feedback.API.APIModel;
using Feedback.API.Helper;
using Feedback.API.Services;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Repositories;
using Courses.API.Repositories.Interfaces;
using Courses.API.Repositories;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Repositories;
using Assessment.API.Repositories.Interface;
using Assessment.API.Repositories;

namespace Feedback.API
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

            services.AddDbContext<FeedbackContext>(options =>
            options.UseSqlServer(connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddTransient<IIdentityService, IdentityService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ITokensRepository, TokensRepository>();
            services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
            services.AddScoped<ICommonSmile, CommonSmileRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IFeedbackStatusDetail, FeedbackStatusDetailRepository>();
            services.AddScoped<IFeedbackOption, FeedbackOptionRepository>();
            services.AddScoped<IFeedbackQuestion, FeedbackQuestionRepository>();
            services.AddScoped<IFeedbackStatus, FeedbackStatusRepository>();
            services.AddScoped<IFeedbackQuestionRejectedRepository, FeedbackQuestionRejectedRepository>();
            services.AddScoped<IFeedbackSheetConfiguration, FeedbackSheetConfigurationRepository>();
            services.AddScoped<IFeedbackSheetConfigurationDetails, FeedbackSheetConfigurationDetailsRepository>();
            services.AddScoped<ICourseCompletionStatusRepository, CourseCompletionStatusRepository>();
            services.AddTransient<IEmail, EmailRepository>();
            services.AddScoped<IModuleCompletionStatusRepository, ModuleCompletionStatusRepository>();
            services.AddScoped<IMyCoursesRepository, MyCoursesRepository>();
            services.AddScoped<INodalCourseRequestsRepository, NodalCourseRequestsRepository>();
            services.AddScoped<IRewardsPointRepository, RewardsPointRepository>();
            services.AddScoped<IAccessibilityRule, AccessibiltyRuleRepository>();
            services.AddSingleton<IAzureStorage, AzureStorage>();
            services.AddScoped<IAsessmentQuestionOption, AsessmentQuestionOptionBankRepository>();
            services.AddScoped<IAssessmentQuestion, AssessmentQuestionRepository>();
            services.AddScoped<IAssessmentQuestionRejectedRepository, AssessmentQuestionRejectedRepository>();
            services.AddScoped<ITrainerFeedbackRepository, TrainerFeedbackRepository>();



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
                    options.Audiences.Add("feedback-api");
                    options.ClientId = "feedback-api";
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

