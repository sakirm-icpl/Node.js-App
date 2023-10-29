using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Gadget.API.APIModel;
using Gadget.API.Data;
using Gadget.API.Repositories;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using User.API.MiddleWare;
using Gadget.API.MiddleWare;
using AspNetCoreRateLimit;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Repositories;
using Gadget.API.Helper.Log_API_Count;

namespace Gadget.API
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
                  options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                  options.JsonSerializerOptions.DictionaryKeyPolicy = null;
              });
            string connection = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<GadgetDbContext>(options => options.UseSqlServer(connection));

            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IFeatureRepository, FeatureRepository>();
            services.AddScoped<IAdvantageRepository, AdvantageRepository>();
            services.AddScoped<IBenefitRepository, BenefitRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IProductAccessibilityRepository, ProductAccessibilityRepository>();

            services.AddScoped<IWorkDiaryRepository, WorkDiaryRepository>();
            //services.AddScoped<ISocialCheckHistoryRepository, SocialCheckHistoryRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();

            //services.AddScoped<IFeedRepository, FeedRepository>();
            //services.AddScoped<IFeedContentRepository, FeedContentRepository>();
            //services.AddScoped<IFeedCommentsRepository, FeedCommentsRepository>();
            //services.AddScoped<IFeedLikeRepository, FeedLikeRepository>();
            //services.AddScoped<IFeedCommentsLikeRepository, FeedCommentsLikeRepository>();

            //services.AddScoped<IThoughtForDayRepository, ThoughtForDayRepository>();
            services.AddScoped<IAnnouncementsRepository, AnnouncementsRepository>();
            //services.AddScoped<IPublicationsRepository, PublicationsRepository>();
            //services.AddScoped<INewsUpdatesRepository, NewsUpdatesRepository>();
            //services.AddScoped<IMediaLibraryRepository, MediaLibraryRepository>();
            //services.AddScoped<IInterestingArticlesRepository, InterestingArticlesRepository>();
            //services.AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>();
            //services.AddScoped<IPollsManagementRepository, PollsManagementRepository>();
            //services.AddScoped<ISuggestionsManagementRepository, SuggestionsManagementRepository>();
            //services.AddScoped<IQuizzesManagementRepository, QuizzesManagementRepository>();
            //services.AddScoped<IQuizQuestionMasterRepository, QuizQuestionMasterRepository>();
            //services.AddScoped<IQuizOptionMasterRepository, QuizOptionMasterRepository>();
            //services.AddScoped<ISurveyManagementRepository, SurveyManagementRepository>();
            //services.AddScoped<ISurveyConfigurationRepository, SurveyConfigurationRepository>();
            //services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
            //services.AddScoped<ISurveyOptionRepository, SurveyOptionRepository>();
            //services.AddScoped<ISurveyOptionNestedRepository, SurveyOptionNestedRepository>();
            //services.AddScoped<IPollsResultRepository, PollsResultRepository>();
            //services.AddScoped<ISurveyResultRepository, SurveyResultRepository>();
            //services.AddScoped<ISurveyResultDetailRepository, SurveyResultDetailRepository>();
            //services.AddScoped<IQuizResultRepository, QuizResultRepository>();
            //services.AddScoped<IQuizResultDetailRepository, QuizResultDetailRepository>();
            //services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<INotification, Notification>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddScoped<IMyAnnouncementRepository, MyAnnouncementRepository>();
            //services.AddScoped<IMySuggestionRepository, MySuggestionRepository>();
            //services.AddScoped<IMySuggestionDetailRepository, MySuggestionDetailRepository>();
            //services.AddScoped<IThoughtForDayCounterRepository, ThoughtForDayCounterRepository>();
            //services.AddScoped<IRewardsPointRepository, RewardsPointRepository>();
            //services.AddScoped<ILcmsRepository, LcmsRepository>();
            services.AddScoped<ICustomerConnectionStringRepository, CustomerConnectionStringRepository>();
            services.AddScoped<IOrganizationMessagesRepository, OrganizationMessagesRepository>();
            services.AddScoped<ITokensRepository, TokensRepository>();
            //services.AddScoped<ISurveyQuestionRejectedRepository, SurveyQuestionRejectedRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IIdeaApplicationApproverRepository, IdeaApplicationApproverRepository>();
            services.AddScoped<IUseCasesList, UseCasesListRepository>();
            //services.AddScoped<IEmployeeSuggestions, EmployeeSuggestionsRepository>();
            services.AddScoped<IDigitalRolesList, DigitalRolesListRepository>();
            services.AddScoped<IDigitalAdoptionReviewList, DigitalAdoptionReviewListRepository>();
            //services.AddScoped<ISuggestionCategories, SuggestionCategoriesRepository>();
            //services.AddScoped<IAwardList, AwardListRepository>();
            //services.AddScoped<IEmployeeAwards, EmployeeAwardsRepository>();
            services.AddSingleton<IAzureStorage, AzureStorage>();
            ConfigureAuthService(services);

            services.AddDistributedMemoryCache();

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            services.AddMvc();
            //Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gadget API", Version = "v1" });
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin();
                });
            });
            services.AddScoped<APIRequestCount<ClientUserApiCount>>();
        }

        private void AddJsonOptions(Func<object, object> p)
        {
            throw new NotImplementedException();
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
                    options.Audiences.Add("gadget_api");
                    options.ClientId = "gadget_api";
                    options.ClientSecret = "ClientSecret";
                    options.RequireHttpsMetadata = false;
                });

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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

            app.UseCors(builder =>
            builder.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
            );

            app.UseAuthentication();


            app.UseSwagger();
            //// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gadget API V1");
            });
            app.UseStaticFiles();

            app.UseRouting();
            app.UseResponseCaching();

            app.UseMiddleware<RequestResponseLogging>();


            string cachePeriod = env.IsDevelopment() ? "600" : "604800";
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
