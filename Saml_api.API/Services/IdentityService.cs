using Microsoft.AspNetCore.Http;
using System;

namespace Saml.API.Services
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
        public string GetToken()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.Request.Headers["Authorization"].ToString();
            else
                return null;
        }

        

        public string GetSubOrganization()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("subsidiary").Value;
            else
                return null;
        }

        public string GetTheme()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("theme").Value;
            else
                return null;
        }

        public string GetLogoname()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("logoname").Value;
            else
                return null;
        }

        public string GetLogonameDark()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("logonamedark").Value;
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

        

        public string GetStatusCode()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("statuscode").Value;
            else
                return null;
        }

        public string GetStatusCodeMessage()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("statuscodemessage").Value;
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

        public string GetLDAP()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("ldap").Value;
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

        public string GetExternalUser()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("ext_user").Value;
            else
                return null;
        }

        public string GetFCMToken()
        {
            if (_context.HttpContext != null)
                return _context.HttpContext.User.FindFirst("fcmtoken").Value;
            else
                return null;
        }

    }
}
