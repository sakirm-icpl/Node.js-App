using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Common
{    public class ApiToken : TypeFilterAttribute
    {
        public ApiToken() : base(typeof(ApiKeyFilter))
        {
        }
        

        public class ApiKeyFilter :  IAsyncActionFilter
        {
          
            private const string APIKEY = "apitoken";
            public IConfiguration _configuration { get; }
            public IBasicAuthRepository _basicAuthRepository;
            public ApiKeyFilter( IConfiguration config, IBasicAuthRepository basicAuthRepository)
            {
                this._configuration = config;
                this._basicAuthRepository = basicAuthRepository;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                try
                {
                    
                    BasicAuthCredentials authCredentials = null;
                    if (!context.HttpContext.Request.Headers.TryGetValue(APIKEY, out var extractedApiKey))
                    {
                        context.HttpContext.Response.StatusCode = 401;
                        await context.HttpContext.Response.WriteAsync("Api Key was not provided ");
                        return;
                    }

                    authCredentials = await _basicAuthRepository.AuthenticateApiToken(extractedApiKey);

                    if (authCredentials == null)
                    {
                        context.HttpContext.Response.StatusCode = 401;
                        await context.HttpContext.Response.WriteAsync("Unauthorized client");
                        return;
                    }
                    await next();
                }
                catch (Exception)
                {
                    await next();
                }
            }       
           
        }
    }
}
