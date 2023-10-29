using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Helper;
using System;

namespace Payment.API.Helper.Log_API_Count
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
                clientUserApiCount.CreatedBy = Convert.ToInt32(_context.HttpContext.User.FindFirst("sub").Value.Decrypt());
                clientUserApiCount.Path = context.HttpContext.Request.Path;
                clientUserApiCount.Method = context.HttpContext.Request.Method;
                clientUserApiCount.ServiceName = "payment";
                clientUserApiCount.OrgCode = _context.HttpContext.User.FindFirst("locality").Value.Decrypt();

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
