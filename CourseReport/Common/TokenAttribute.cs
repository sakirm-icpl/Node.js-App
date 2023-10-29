using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using CourseReport.API.Repositories.Interface;
using System;
using System.Threading.Tasks;
using log4net;
using CourseReport.API.Helper;

namespace CourseReport.API.Common
{
    public class TokenPermissions
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TokenPermissions));

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

        public class TokenRequiredFilter : IAsyncActionFilter
        {
            private readonly ITokensRepository _tokensRepository;
            public IConfiguration _configuration { get; }
            public TokenRequiredFilter(ITokensRepository tokensRepository, IConfiguration config)
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
                    if (await this._tokensRepository.UserTokenExists(userToken))
                        expiredToken = true;
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
                        await next();

                }
                catch(Exception ex)
                {
                    _logger.Error( Utilities.GetDetailedException(ex));
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
        }
    }
}
