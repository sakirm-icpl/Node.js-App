using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using CourseReport.API.Data;
using System;

namespace CourseReport.API.Helper.Log_API_Count
{
    public class APIRequestCount<T> : IActionFilter where T : class
    {
        private ReportDbContext _db;
        private IHttpContextAccessor _context;
        public APIRequestCount(ReportDbContext db, IHttpContextAccessor context)
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
                clientUserApiCount.ServiceName = "report";
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
