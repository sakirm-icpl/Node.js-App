using PollManagement.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;

namespace PollManagement.API.Helper.Log_API_Count
{
    public class APIRequestCount<T> : IActionFilter where T : class
    {
        private GadgetDbContext _db;
        private IHttpContextAccessor _context;
        public APIRequestCount(GadgetDbContext db, IHttpContextAccessor context)
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
                if(_context.HttpContext.User.FindFirst("sub") != null)
                {
                    clientUserApiCount.CreatedBy = Convert.ToInt32(Security.Decrypt(_context.HttpContext.User.FindFirst("sub").Value));
                }
                else
                {
                    clientUserApiCount.CreatedBy = 0;
                }
                clientUserApiCount.Path = context.HttpContext.Request.Path;
                clientUserApiCount.Method = context.HttpContext.Request.Method;
                clientUserApiCount.ServiceName = "gadget";
                if(_context.HttpContext.User.FindFirst("locality") != null)
                {
                    clientUserApiCount.OrgCode = Security.Decrypt(_context.HttpContext.User.FindFirst("locality").Value);
                }
                else
                {
                    clientUserApiCount.OrgCode = "anonymous";
                }

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