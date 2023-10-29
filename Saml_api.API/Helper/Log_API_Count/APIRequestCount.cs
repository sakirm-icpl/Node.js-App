using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using Saml.API.Data;

namespace Saml.API.Helper.Log_API_Count
{
    public class APIRequestCount<T> : IActionFilter where T : class
    {
        private UserDbContext _db;
        private IHttpContextAccessor _context;
        public APIRequestCount(UserDbContext db, IHttpContextAccessor context)
        {
            _db = db;
            _context = context;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

            try
            {
                ClientUserApiCount clientUserApiCount = new ClientUserApiCount();

                clientUserApiCount.CreatedDate = DateTime.Now;
                clientUserApiCount.CreatedBy = Convert.ToInt32(Security.Decrypt(_context.HttpContext.User.FindFirst("sub").Value));
                clientUserApiCount.Path = context.HttpContext.Request.Path;
                clientUserApiCount.Method = context.HttpContext.Request.Method;
                clientUserApiCount.ServiceName = "user";
                clientUserApiCount.OrgCode = Security.Decrypt(_context.HttpContext.User.FindFirst("locality").Value);

                _db.ClientUserApiCount.Add(clientUserApiCount);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Utilities.GetDetailedException(ex);
            }

        }
    }
}
