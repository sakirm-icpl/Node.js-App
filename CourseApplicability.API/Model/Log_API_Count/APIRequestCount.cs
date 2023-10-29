using CourseApplicability.API.Helper;
using CourseApplicability.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseApplicability.API.Model.Log_API_Count
{
    public class APIRequestCount<T> : IActionFilter where T : class
    {
        private CoursesApplicabilityContext _db;
        private IHttpContextAccessor _context;
        public APIRequestCount(CoursesApplicabilityContext db, IHttpContextAccessor context)
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
                clientUserApiCount.ServiceName = "course";
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
