using Microsoft.AspNetCore.Http;
using System;

namespace CourseReport.API.Service
{
    public class IdentityService : IIdentityService
    {
        private IHttpContextAccessor _context;

        public IdentityService(IHttpContextAccessor context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string GetUserIdentity()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("sub").Value;
            else
                return null;
        }

        public string GetConnectionString()
        {
            if (_context.HttpContext != null)
            {
                string connectionString = _context.HttpContext.User.FindFirst("address") == null ? null : _context.HttpContext.User.FindFirst("address").Value;
                return connectionString;
            }
            else
                return null;
        }

        public string GetCustomerCode()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("locality").Value;
            else
                return null;
        }

        public string GetUserId()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("username").Value;
            else
                return null;
        }
        public string GetUserRole()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("role").Value;
            else
                return null;
        }

        public string GetOrgCode()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("locality").Value;
            else
                return null;
        }

        public string GetToken()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.Request.Headers["Authorization"].ToString();
            else
                return null;
        }

    }
}
