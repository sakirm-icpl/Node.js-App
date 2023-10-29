using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using Saml.API.APIModel;
using Saml.API.Data;
using Saml.API.Helper;
using Saml.API.Helper.Interface;
using Saml.API.Repositories;
using Saml.API.Repositories.Interfaces;
//using User.API.Repositories.Interfaces.MyLearningSystem;
//using User.API.Repositories.MyLearningSystem;
using Saml.API.Services;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
//using Saml.API.MiddleWare;
using AspNetCoreRateLimit;
//using AzureStorageLibrary.Repositories.Interfaces;
//using AzureStorageLibrary.Repositories;
using Saml.API.Helper.Log_API_Count;

// Todo
// License text information is missing, pls check all files


namespace Saml.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimit"));
            services.AddSingleton<IIpPolicyStore,MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore,MemoryCacheRateLimitCounterStore>();
            //services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

            services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
            //services.AddScoped<IUserHistoryRepository, UserHistoryRepository>();
            //services.AddScoped<IRolesRepository, RolesRepository>();
            //services.AddScoped<IRolesPermissionRepository, RolesPermissionRepository>();
            //services.AddScoped<IRoleAuthorityRepository, RoleAuthorityRepository>();
            //services.AddScoped<IPermissionRepository, PermissionRepository>();
            //services.AddScoped<ILocationRepository, LocationRepository>();
            //services.AddScoped<IAreaRepository, AreaRepository>();
            //services.AddScoped<IGroupRepository, GroupRepository>();
            //services.AddScoped<IBusinessRepository, BusinessRepository>();
            //services.AddScoped<IHRMSRepository, HRMSRepository>();
            //services.AddScoped<IConfigure1Repository, Configure1Repository>();
            //services.AddScoped<IConfigure2Repository, Configure2Repository>();
            //services.AddScoped<IConfigure3Repository, Configure3Repository>();
            //services.AddScoped<IConfigure4Repository, Configure4Repository>();
            //services.AddScoped<IConfigure5Repository, Configure5Repository>();
            //services.AddScoped<IConfigure6Repository, Configure6Repository>();
            //services.AddScoped<IConfigure7Repository, Configure7Repository>();
            //services.AddScoped<IConfigure8Repository, Configure8Repository>();
            //services.AddScoped<IConfigure9Repository, Configure9Repository>();
            //services.AddScoped<IConfigure10Repository, Configure10Repository>();
            //services.AddScoped<IConfigure11Repository, Configure11Repository>();
            //services.AddScoped<IConfigure12Repository, Configure12Repository>();
            //services.AddScoped<IConfigure13Repository, Configure13Repository>();
            //services.AddScoped<IConfigure14Repository, Configure14Repository>();
            //services.AddScoped<IConfigure15Repository, Configure15Repository>();
            //services.AddScoped<ISignUpRepository, SignUpRepository>();
            //services.AddScoped<IJobResponsibilityRepository, JobResponsibilityRepository>();
            //services.AddScoped<IKeyAreaSettingRepository, KeyAreaSettingRepository>();
            //services.AddScoped<IJobAidRepository, JobAidRepository>();
            //services.AddScoped<IMailTemplateDesignerRepository, MailTemplateDesignerRepository>();
            //services.AddScoped<IJobResponsibilityDetailRepository, JobResponsibilityDetailRepository>();
            //services.AddScoped<IUserMasterRejectedRepository, UserMasterRejectedRepository>();
            //services.AddScoped<IMyPreferencesRepository, MyPreferencesRepository>();
            services.AddScoped<IEmail, EmailRepository>();
            services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
            //services.AddScoped<IPasswordHistory, PasswordHistoryRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIdentityService, IdentityService>();
            //services.AddScoped<ILoggedInHistoryRepository, LoggedInHistoryRepository>();
            //services.AddScoped<IUserOtpRepository, UserOtpRpository>();
            //services.AddScoped<IRewardsPoint, RewardsPointRepository>();
            //services.AddScoped<IDemoRepository, DemoRepository>();
            //services.AddScoped<ITokenBlacklistRepository, TokenBlacklistRepository>();
            //services.AddScoped<ITokensRepository, TokensRepository>();
            //services.AddScoped<IUserOtpHistoryRepository, UserOtpHistoryRpository>();
            //services.AddScoped<IExportFileLog, ExportFileLogRpository>();
            services.AddScoped<ITLSHelper, TLSHelper>();
            //services.AddScoped<ISocialMediaRepository, SocialMediaRepository>();
            //services.AddScoped<IUserAuthOtpRepository, UserAuthOtpRpository>();

            services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
            //services.AddScoped<IDesignationRoleRepository, DesignationRoleRepository>();
            //services.AddScoped<IUserSignUpRepository, UserSignUpRepository>();
            //services.AddScoped<INodalGroupManagementRepository, NodalGroupManagementRepository>();
            //services.AddScoped<IUserTeamsRepository, UserTeamsRepository>();
            //services.AddScoped<IBasicAuthRepository,BasicAuthCredentialsRepository>();

            //services.AddScoped<IHRMSService, HRMSService>();
            //services.AddScoped<IHRMSBaseRepository, HRMSBaseRepository>();
            //services.AddScoped<IConfigurableParameterValuesRepository, ConfigurableParameterValuesRepository>();
            //services.AddSingleton<IAzureStorage, AzureStorage>();
            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection"));
            });
            ConfigureAuthService(services);
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
            });
            services.AddCors();
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddMvc(option => option.EnableEndpointRouting = false)
               .AddJsonOptions(options =>
               {
                   options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                   options.JsonSerializerOptions.DictionaryKeyPolicy = null;
               });
            services.AddScoped<APIRequestCount<ClientUserApiCount>>();
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
                    options.Audiences.Add("saml_api");
                    options.ClientId = "saml_api";
                    options.ClientSecret = "ClientSecret";
                    options.RequireHttpsMetadata = false;
                });
            }
            catch (Exception ex)
            { }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {

                app.UseDeveloperExceptionPage();
            }
            if(Configuration.GetValue<bool>("CheckRateLimit"))
				app.UseIpRateLimiting();

            app.UseCors(
                builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseAuthentication();
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API V1");
            });
            app.UseStaticFiles();
           /* app.UseMiddleware<RequestResponseLogging>();*/ //Will be added in future for Logging Requests

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

            app.UseMvc();
        }
    }
}
