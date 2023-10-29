using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using log4net;

namespace MyCourse.API.Common
{
    public class AuthorizePermissions
    {
        public static class Claims
        {
            private static readonly ILog _logger = LogManager.GetLogger(typeof(Claims));
            public const string Permissions = "permissions";
        }

        public class PermissionRequiredAttribute : TypeFilterAttribute
        {
            public PermissionRequiredAttribute(string claimValue = "") : base(typeof(PermissionRequiredFilter))
            {
                Arguments = new object[] { new Claim(Claims.Permissions, claimValue) };
            }
        }

        public class PermissionRequiredFilter : IAsyncActionFilter
        {
            readonly Claim _claim;
            public PermissionRequiredFilter(Claim claim)
            {
                _claim = claim;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var rd = context.ActionDescriptor;
                string action = rd.RouteValues["action"];
                string controller = rd.RouteValues["controller"];

                string permission;
                permission = _claim.Value;

                var permissions = permission.Split(" ");
                var hasClaim = false;
                hasClaim = context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value.Split(" ").Any(e => permissions.Contains(e)));
              
                if (!hasClaim)
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    await next();
                }
            }
        }
    }
}
