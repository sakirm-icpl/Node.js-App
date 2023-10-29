using Feedback.API.Controllers;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.VisualBasic;

namespace Feedback.API.Common
{
    public class TokenPermissions
    {

        public class TokenRequiredAttribute : TypeFilterAttribute
        {
            public TokenRequiredAttribute() : base(typeof(TokenRequiredFilter))
            {
            }
        }

        public class TokenRequiredFilter : IdentityController, IAsyncActionFilter
        {
            readonly ITokensRepository _tokensRepository;
            ICourseRepository _courseRepository;
            private readonly IIdentityService _identitySvc;
            private static readonly ILog _logger = LogManager.GetLogger(typeof(TokenRequiredFilter));
            public IConfiguration _configuration { get; }

            public TokenRequiredFilter(ITokensRepository tokensRepository, ICourseRepository courseRepository, IIdentityService _identitySvc, IConfiguration config) : base(_identitySvc)
            {
                this._tokensRepository = tokensRepository;
                _courseRepository = courseRepository;
                this._configuration = config;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                try
                {
                    bool IsCheckToken = Convert.ToBoolean(this._configuration["CheckTocken"]);
                    if (IsCheckToken == true)
                    {
                        string userToken = context.HttpContext.Request.Headers["Authorization"].ToString() == null ? null : context.HttpContext.Request.Headers["Authorization"].ToString();
                        bool expiredToken = false;

                        if (!string.IsNullOrEmpty(userToken))
                        {
                            if (await GetMultipleLoginConfigValueAsync(OrganisationCode) == false)
                            {
                                if (await this._tokensRepository.UserTokenExists(userToken))
                                    expiredToken = true;
                            }
                        }
                        if (expiredToken)
                        {
                            context.Result = new CustomUnauthorizedResult("Your account was accessed from a new location!");
                        }
                        else
                        {
                            await next();
                        }
                    }
                    else
                    {
                        await next();
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    await next();
                }
            }

            public async Task<bool> GetMultipleLoginConfigValueAsync(string orgcode)
            {
                bool isMultipleLoginEnabled;
                try
                {
                    var cache = new CacheManager.CacheManager();
                    string cacheKeyConfig = CacheKeyNames.IS_MULTIPLE_LOGIN_ENABLED + "-" + orgcode.ToUpper();

                    if (cache.IsAdded(cacheKeyConfig))
                        isMultipleLoginEnabled = cache.Get<string>(cacheKeyConfig).ToUpper() == "YES" ? true : false;
                    else
                    {
                        string configValue = await _courseRepository.GetMasterConfigurableParameterValue("MULTIPLE_LOGIN");
                        if (configValue == null || (configValue.ToUpper() != "YES" && configValue.ToUpper() != "NO"))
                            configValue = "YES";
                        cache.Add(cacheKeyConfig.ToUpper(), configValue, System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                        isMultipleLoginEnabled = configValue.ToUpper() == "YES" ? true : false;
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return true;
                }

                return isMultipleLoginEnabled;


            }

            public class CustomUnauthorizedResult : JsonResult
            {
                public CustomUnauthorizedResult(string message)
                    : base(new CustomError(message))
                {
                    StatusCode = StatusCodes.Status401Unauthorized;
                }
            }
            public class CustomError
            {
                public string Error { get; }
                public CustomError(string message)
                {
                    Error = message;
                }
            }
        }
    }
}
