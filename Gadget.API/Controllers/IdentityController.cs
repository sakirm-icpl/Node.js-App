using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Gadget.API.Helper;
using Gadget.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;


namespace Gadget.API.Controllers
{
    public class IdentityController : Controller
    {
        private int _userId;
        private string _orgCode;
        private string _userRole;
        private string _userName;
        private string _connectionString;
        private string _customerCode;
        private string _subOrganization;
        private string _theme;
        private string _logoname;
        private string _statusCode;
        private string _statusCodeMessage;
        private string _token;
        private string _fcmToken;

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
                   //_orgCode = Security.Decrypt(User.GetClaim(OpenIdConnectConstants.Claims.Locality)); //User.GetClaim(OpenIdConnectConstants.Claims.);//"11";                
                _orgCode = Security.Decrypt(_identitySvc.GetOrgCode()); //User.GetClaim(OpenIdConnectConstants.Claims.);//"11";             
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

        protected String UserName
        {
            get
            {
                _userName = Security.Decrypt(_identitySvc.GetUserId());
                return _userName;
            }
        }

        protected String CustomerCode
        {
            get
            {
                _customerCode = Security.Decrypt(_identitySvc.GetCustomerCode());
                return _customerCode;
            }
        }

        protected String SubOrganization
        {
            get
            {
                _subOrganization = _identitySvc.GetSubOrganization();
                return _subOrganization;
            }
        }

        protected String Theme
        {
            get
            {
                _theme = _identitySvc.GetTheme();
                return _theme;
            }
        }

        protected String Logoname
        {
            get
            {
                _logoname = _identitySvc.GetLogoname();
                return _logoname;
            }
        }

        protected String LogonameDark
        {
            get
            {
                _logoname = _identitySvc.GetLogonameDark();
                return _logoname;
            }
        }

        protected String StatusCodeResult
        {
            get
            {
                _statusCode = _identitySvc.GetStatusCode();
                return _statusCode;
            }
        }

        protected String StatusCodeMessage
        {
            get
            {
                _statusCodeMessage = _identitySvc.GetStatusCodeMessage();
                return _statusCodeMessage;
            }
        }

        protected String Token
        {
            get
            {
                _token = _identitySvc.GetToken();
                return _token;
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

        protected bool IsLDAP
        {
            get
            {
                if (!string.IsNullOrEmpty(_identitySvc.GetLDAP()))
                {
                    if (Convert.ToBoolean(Convert.ToInt32(_identitySvc.GetLDAP())))
                        return true;
                    else
                        return false;
                }
                return false;
            }
        }

        protected bool IsExternalUser
        {
            get
            {
                if (!string.IsNullOrEmpty(_identitySvc.GetExternalUser()))
                {
                    if (Convert.ToBoolean(Convert.ToInt32(_identitySvc.GetExternalUser())))
                        return true;
                    else
                        return false;
                }
                return false;
            }
        }

        protected String FCMToken
        {
            get
            {
                if (!string.IsNullOrEmpty(_identitySvc.GetFCMToken()))
                {
                    if (String.Equals(_identitySvc.GetFCMToken(), "-"))
                        return "";
                    else
                        _fcmToken = _identitySvc.GetFCMToken();
                }
                return _fcmToken;
            }
        }

    }
}