using Gadget.API.Controllers;
using Gadget.API.Helper;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Gadget.API.Common
{
    public class TokenPermissions
    {
        //public static class Claims
        //{
        //    public const string Permissions = "permissions";
        //}

        public class TokenRequiredAttribute : TypeFilterAttribute
        {
            public TokenRequiredAttribute() : base(typeof(TokenRequiredFilter))
            {
            }
        }

        public class TokenRequiredFilter : IdentityController, IAsyncActionFilter
        {
            private readonly ITokensRepository _tokensRepository;
            private static readonly ILog _logger = LogManager.GetLogger(typeof(TokenRequiredFilter));
            public IConfiguration _configuration { get; }
            public TokenRequiredFilter(ITokensRepository tokensRepository, IIdentityService _identitySvc, IConfiguration config) : base(_identitySvc)
            {
                this._tokensRepository = tokensRepository;
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
                            if (await GetMultipleLoginConfigValueAsync(OrgCode) == false)
                            {
                                if (await this._tokensRepository.UserTokenExists(userToken))
                                    expiredToken = true;
                            }
                        }
                        if (expiredToken)
                            context.Result = new CustomUnauthorizedResult("Your account was accessed from a new location!");
                        else
                            await next();
                    }
                    else
                        await next();

                }
                catch (Exception)
                {
                    await next();
                }
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

            public async Task<bool> GetMultipleLoginConfigValueAsync(string orgcode)
            {
                bool isMultipleLoginEnabled;
                try
                {
                    var cache = new CacheManager.CacheManager();
                    string cacheKeyConfig = Constants.CacheKeyNames.IS_MULTIPLE_LOGIN_ENABLED + "-" + orgcode.ToUpper();

                    if (cache.IsAdded(cacheKeyConfig))
                        isMultipleLoginEnabled = cache.Get<string>(cacheKeyConfig).ToUpper() == "YES" ? true : false;
                    else
                    {
                        string configValue = await _tokensRepository.GetMasterConfigurableParameterValue("MULTIPLE_LOGIN");
                        if (configValue == null || (configValue.ToUpper() != "YES" && configValue.ToUpper() != "NO"))
                            configValue = "YES";
                        cache.Add(cacheKeyConfig.ToUpper(), configValue, System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                        isMultipleLoginEnabled = configValue.ToUpper() == "YES" ? true : false;
                    }

                }
                catch (System.Exception ex)
                {
                    _logger.Error(string.Format("Exception in function GetMultipleLoginConfigValueAsync :- {0} for Organisation {1}", Utilities.GetDetailedException(ex), orgcode));
                    return true;
                }

                return isMultipleLoginEnabled;


            }
        }
    }
}
