using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Mvc;
using CourseReport.API.Helper;
using System;
using CourseReport.API.Service;

namespace CourseReport.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Identity")]
    public class IdentityController : Controller
    {
        private int _userId;
        private string _orgCode;
        private string _userRole;
        private string _connectionString;
        private string _token;
        private readonly IIdentityService _identitySvc;
        public IdentityController(IIdentityService identitySvc)
        {
            this._identitySvc = identitySvc;
        }
        protected int UserId
        {
            get
            {
                _userId = Convert.ToInt32(Security.Decrypt(_identitySvc.GetUserIdentity())); //User.GetClaim(OpenIdConnectConstants.Claims.);//"11";                
                return _userId;
            }
        }
        protected String OrgCode
        {
            get
            {
                _orgCode = Security.Decrypt(User.GetClaim(OpenIdConnectConstants.Claims.Locality)); //User.GetClaim(OpenIdConnectConstants.Claims.);//"11";                
                return _orgCode;
            }
        }
        protected string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = Security.Decrypt(User.GetClaim(OpenIdConnectConstants.Claims.Address));//string.Empty;//
                }
                return _connectionString;
            }
        }
        protected string UserRole
        {
            get
            {
                if (_userRole == null)
                {
                    _userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
                }
                return _userRole;
            }
        }

        protected string Token
        {
            get
            {
                _token = _identitySvc.GetToken();
                return _token;
            }
        }
    }
}