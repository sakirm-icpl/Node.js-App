using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;

namespace Courses.API.Middleware
{
    public class CustomRateLimitConfiguration : RateLimitConfiguration
    {
        public CustomRateLimitConfiguration(
        IHttpContextAccessor httpContextAccessor,
        IOptions<IpRateLimitOptions> ipOptions,
        IOptions<ClientRateLimitOptions> clientOptions)
            : base(httpContextAccessor, ipOptions, clientOptions)
        {
        }

        protected override void RegisterResolvers()
        {
            ClientResolvers.Add(new ClientQueryStringResolveContributor(HttpContextAccessor));
        }
    }
    public class ClientQueryStringResolveContributor : IClientResolveContributor
    {
        private IHttpContextAccessor httpContextAccessor;

        public ClientQueryStringResolveContributor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string ResolveClient()
        {
            var request = httpContextAccessor.HttpContext?.Request;
            var queryDictionary =
                Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                    request.QueryString.ToString());
            if (queryDictionary.ContainsKey("api_key")
                && !string.IsNullOrWhiteSpace(queryDictionary["api_key"]))
            {
                return queryDictionary["api_key"];
            }

            return Guid.NewGuid().ToString();
        }
    }
}
