using AspNet.Security.OAuth.Introspection;

using AutoMapper;

using CourseApplicability.API.Helper;
using CourseApplicability.API.Middleware;
using CourseApplicability.API.Models;
using CourseApplicability.API.APIModel;
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
using CourseApplicability.API.Model.Log_API_Count;
using CourseApplicability.API.Services;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Repositories;
//using AzureStorageLibrary.Repositories.Interfaces;
//using AzureStorageLibrary.Repositories;



namespace CourseApplicability.API
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

            services.AddDbContext<CoursesApplicabilityContext>(options =>
            options.UseSqlServer(connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
      
      
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ITokensRepository, TokensRepository>();
            services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
            services.AddScoped<IAccessibilityRule, AccessibiltyRuleRepository>();
            services.AddScoped<IApplicabilityGroupTemplate, ApplicabilityGroupTemplateRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<ICoursesEnrollRequestRepository, CoursesEnrollRequestRepository>();
            services.AddScoped<IDevelopmentPlanRepository, DevelopmentPlanRepository>();
            services.AddTransient<IEmail, EmailRepository>();
            services.AddScoped<INodalCourseRequestsRepository, NodalCourseRequestsRepository>();
            services.AddScoped<ITLSHelper, TLSHelper>();
            services.AddScoped<ICoursesEnrollRequestDetailsRepository, CoursesEnrollRequestDetailsRepository>();

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

