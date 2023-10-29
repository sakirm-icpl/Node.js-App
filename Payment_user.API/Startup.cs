using AspNet.Security.OAuth.Introspection;
//using Payment.API.MiddleWare;
using AspNetCoreRateLimit;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Payment.API.APIModel;
using Payment.API.Data;
using Payment.API.Helper;
using Payment.API.Helper.Interface;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Repositories;
using Payment.API.Helper.Log_API_Count;
//using Pay.API.MiddleWare;
using Payment.API.MiddleWare;
using Payment.API.Repositories;
using Payment.API.Repositories.Interfaces;
//using User.API.Repositories.Interfaces;

using Payment.API.Services;
using System;
using System.Linq;
using System.Text.Json;

// Todo
// License text information is missing, pls check all files


namespace Payment.API
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
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            //services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();


            //services.AddScoped<IUserRepository,UserR>();
            //services.AddScoped<IU, usersettingsrepository>();
            //services.addscoped<iuserhistoryrepository, userhistoryrepository>();
            //services.addscoped<irolesrepository, rolesrepository>();
            //services.addscoped<irolespermissionrepository, rolespermissionrepository>();
            //services.addscoped<iroleauthorityrepository, roleauthorityrepository>();
            //services.addscoped<ipermissionrepository, permissionrepository>();
            //services.addscoped<ilocationrepository, locationrepository>();
            //services.addscoped<iarearepository, arearepository>();
            //services.addscoped<igrouprepository, grouprepository>();
            //services.addscoped<ibusinessrepository, businessrepository>();
            //services.addscoped<ihrmsrepository, hrmsrepository>();
            //services.addscoped<iconfigure1repository, configure1repository>();
            //services.addscoped<iconfigure2repository, configure2repository>();
            //services.addscoped<iconfigure3repository, configure3repository>();
            //services.addscoped<iconfigure4repository, configure4repository>();
            //services.addscoped<iconfigure5repository, configure5repository>();
            //services.addscoped<iconfigure6repository, configure6repository>();
            //services.addscoped<iconfigure7repository, configure7repository>();
            //services.addscoped<iconfigure8repository, configure8repository>();
            //services.addscoped<iconfigure9repository, configure9repository>();
            //services.addscoped<iconfigure10repository, configure10repository>();
            //services.addscoped<iconfigure11repository, configure11repository>();
            //services.addscoped<iconfigure12repository, configure12repository>();
            //services.addscoped<iconfigure13repository, configure13repository>();
            //services.addscoped<iconfigure14repository, configure14repository>();
            //services.addscoped<iconfigure15repository, configure15repository>();
            //services.addscoped<isignuprepository, signuprepository>();
            //services.addscoped<ijobresponsibilityrepository, jobresponsibilityrepository>();
            //services.addscoped<ikeyareasettingrepository, keyareasettingrepository>();
            //services.addscoped<ijobaidrepository, jobaidrepository>();
            //services.addscoped<imailtemplatedesignerrepository, mailtemplatedesignerrepository>();
            //services.addscoped<ijobresponsibilitydetailrepository, jobresponsibilitydetailrepository>();
            //services.addscoped<iusermasterrejectedrepository, usermasterrejectedrepository>();
            //services.addscoped<imypreferencesrepository, mypreferencesrepository>();
            services.AddScoped<IEmail, EmailRepository>();
            services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
            //services.addscoped<ipasswordhistory, passwordhistoryrepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIdentityService, IdentityService>();
            //services.addscoped<iloggedinhistoryrepository, loggedinhistoryrepository>();
            //services.addscoped<iuserotprepository, userotprpository>();
            //services.addscoped<irewardspoint, rewardspointrepository>();
            //services.addscoped<idemorepository, demorepository>();
            //services.addscoped<itokenblacklistrepository, tokenblacklistrepository>();
            //services.AddScoped<ITokensRepository, TokensRepository>();
            //services.addscoped<iuserotphistoryrepository, userotphistoryrpository>();
            ////services.addscoped<iexportfilelog, exportfilelogrpository>();
            //services.AddScoped<ITLSHelper, TLSHelper>();
            //services.addscoped<isocialmediarepository, socialmediarepository>();
            //services.addscoped<iuserauthotprepository, userauthotprpository>();

            //services.addscoped<iconfigurationparameterrepository, configurationparameterrepository>();
            //services.addscoped<idesignationrolerepository, designationrolerepository>();
            services.AddScoped<IUserSignUpRepository, UserSignUpRepository>();
            //services.addscoped<inodalgroupmanagementrepository, nodalgroupmanagementrepository>();
            //services.addscoped<iuserteamsrepository, userteamsrepository>();
            //services.addscoped<ibasicauthrepository, basicauthcredentialsrepository>();

            services.AddScoped<IHRMSService, HRMSService>();
            services.AddScoped<IHRMSBaseRepository, HRMSBaseRepository>();

            //services.addscoped<iconfigurableparametervaluesrepository, configurableparametervaluesrepository>();
            //services.addsingleton<iazurestorage, azurestorage>();
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
                    options.Audiences.Add("payment-api"); 
                    options.ClientId = "payment-api";
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
            if (Configuration.GetValue<bool>("CheckRateLimit"))
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
            app.UseMiddleware<RequestResponseLogging>(); //Will be added in future for Logging Requests

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
