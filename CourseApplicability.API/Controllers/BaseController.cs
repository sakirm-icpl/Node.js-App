using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Mvc;
using System;
using log4net;
using CourseApplicability.API.Services;
using CourseApplicability.API.Helper;

namespace CourseApplicability.API.Controllers
{
    public class IdentityController : Controller
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IdentityController));
        private int _userId;
        private string _token;
        private string _orgCode;
        private string _connectionString;
        private readonly IIdentityService _identitySvc;
        private string _loginId;
        private string _roleCode;
        private string _IsInstitute;
        private bool _externalUser;
        public IdentityController(IIdentityService identitySvc)
        {
            this._identitySvc = identitySvc;
        }
        protected int UserId
        {
            get
            {
                _userId = Convert.ToInt32(Security.Decrypt(_identitySvc.GetUserIdentity()));
                return _userId;
            }
        }
        protected String OrgCode
        {
            get
            {
                _orgCode = Security.Decrypt(_identitySvc.GetOrgCode());
                return _orgCode;
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
        protected String OrganisationCode
        {
            get
            {
                _orgCode = Security.Decrypt(_identitySvc.GetOrgCode());
                return _orgCode;
            }
        }
        protected string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = Security.Decrypt(User.GetClaim(OpenIdConnectConstants.Claims.Address));
                }
                return _connectionString;
            }
        }
        protected String UserName
        {
            get
            {
                _orgCode = Security.Decrypt(_identitySvc.GetUserName());
                return _orgCode;
            }
        }

        protected String LoginId
        {
            get
            {
                _loginId = Security.Decrypt(_identitySvc.GetUserId());
                return _loginId;
            }
        }
        protected String RoleCode
        {
            get
            {
                _roleCode = _identitySvc.GetRoleCode();
                return _roleCode;
            }
        }

        protected bool IsiOS
        {
            get
            {
                if (!string.IsNullOrEmpty(_identitySvc.GetiOS()))
                {
                    if (String.Equals(_identitySvc.GetiOS(), "-"))
                        return false;

                    if (Convert.ToBoolean(Convert.ToInt32(_identitySvc.GetiOS())))
                        return true;
                    else
                        return false;
                }
                return false;
            }
        }

        protected bool externalUser
        {
            get
            {
                if (Convert.ToBoolean(Convert.ToInt32(_identitySvc.GetIsExternal())))
                    return true;
                else
                    return false;
            }
        }

        protected String IsInstitute
        {
            get
            {
                _IsInstitute = _identitySvc.GetIsInstitute();
                return _IsInstitute;
            }
        }
    }
}