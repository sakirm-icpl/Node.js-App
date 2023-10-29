using Microsoft.AspNetCore.Http;
using System;

namespace Competency.API.Services
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
                return _context.HttpContext.User.FindFirst("address").Value;
            else
                return null;
        }

        public string GetCustomerCode()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("client_id").Value;
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

        public string GetUserId()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("username").Value;
            else
                return null;
        }
        public string GetUserName()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("name").Value;
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
        public string GetRoleCode()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("role").Value;
            else
                return null;
        }

        public string GetiOS()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("ios").Value;
            else
                return null;
        }
        public string GetIsExternal()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("ext_user").Value;
            else
                return null;
        }

        public string GetIsInstitute()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("isinstitute").Value;
            else
                return "false";
        }
    }
}
