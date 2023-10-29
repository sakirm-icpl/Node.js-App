//======================================
// <copyright file="UserController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using AspNet.Security.OAuth.Introspection;
using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using com.pakhee.common;
using LDAPService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Common;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.AuthorizePermissions;
using static User.API.Common.TokenPermissions;
using static User.API.Helper.EnumHelper;
using static User.API.Models.UserMaster;
using Exception = System.Exception;
using log4net;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using User.API.Helper.Log_API_Count;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class UserController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserController));
        private string url;
        private string SMSURL;
        private IUserRepository _userRepository;
        private IUserSettingsRepository _userSettingsRepository;
        private IUserMasterRejectedRepository _userMasterRejectedRepository;
        private IUserHistoryRepository _userHistoryRepository;
        private IHRMSRepository _hrmsRepository;
        private IPasswordHistory _passwordHistory;
        private readonly IIdentityService _identitySvc;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly ILoggedInHistoryRepository _loggedInHistoryRepository;
        private readonly IUserOtpRepository _userMasterOtp;
        private readonly IUserOtpHistoryRepository _userMasterOtpHistory;
        private readonly IUserAuthOtpRepository _userAuthOtp;
        private readonly IExportFileLog _exportFileLog;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration configuration;
        private IRewardsPoint _rewardsPoint;
        private IRolesRepository _rolesRepository;
        private IBusinessRepository _businessRepository;
        private readonly ITokenBlacklistRepository _tokenBlacklistRepository;
        private readonly ITokensRepository _tokensRepository;
        IAzureStorage _azurestorage;

        public UserController
            (IUserRepository userReposirotry,
            IUserSettingsRepository userSettingsRepository,
            IUserMasterRejectedRepository userMasterRejectedReposirotry,
            IUserHistoryRepository userHistoryRepository,
            IHRMSRepository hrmsRepository,
            IWebHostEnvironment environment,
            IConfiguration configure,
            IIdentityService identityService,
            IHttpContextAccessor httpContextAccessor,
            ICustomerConnectionStringRepository customerConnectionStringRepository,
            IConfiguration configurationCon,
            IPasswordHistory passwordHistory,
            ILoggedInHistoryRepository loggedInHistoryRepository,
            IUserOtpRepository userMasterOtp,
            IUserOtpHistoryRepository userMasterOtpHistory,
            IExportFileLog exportFileLog,
            IRewardsPoint rewardsPoint,
            IRolesRepository rolesRepository,
            IBusinessRepository businessRepository,
            ITokenBlacklistRepository tokenBlacklistRepository,
            ITokensRepository tokensRepository,
            IUserAuthOtpRepository AuthOtp,
            IAzureStorage azurestorage
            ) : base(identityService)
        {
            this._azurestorage = azurestorage;
            this._userRepository = userReposirotry;
            this._userMasterRejectedRepository = userMasterRejectedReposirotry;
            this._userHistoryRepository = userHistoryRepository;
            this._hrmsRepository = hrmsRepository;
            this._passwordHistory = passwordHistory;
            this._userSettingsRepository = userSettingsRepository;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            this._loggedInHistoryRepository = loggedInHistoryRepository;
            this._userMasterOtp = userMasterOtp;
            this._userMasterOtpHistory = userMasterOtpHistory;
            this._exportFileLog = exportFileLog;
            this._configuration = configure;
            this.configuration = configurationCon;
            this.hostingEnvironment = environment;
            this._identitySvc = identityService;
            this._httpContextAccessor = httpContextAccessor;
            this._rewardsPoint = rewardsPoint;
            this._rolesRepository = rolesRepository;
            this._businessRepository = businessRepository;
            this._tokenBlacklistRepository = tokenBlacklistRepository;
            this._tokensRepository = tokensRepository;
            this._userAuthOtp = AuthOtp;
        }

        [AllowAnonymous]
        [HttpPost("GetAuthenticationOTP")]
        public async Task<IActionResult> Get([FromBody] ApiTwoWayAuthentication auth)
        {
            try
            {
                if (auth.display.ToLower() == "hdfc")
                {
                    string requestUsername;
                    requestUsername = Security.DecryptForUI(auth.username.Replace(" ", "+"));
                    requestUsername = requestUsername.ToLower();

                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(auth.display);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                        });

                    }

                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    this._userAuthOtp.ChangeDbContext(OrgnizationConnectionString);


                    if (await _userRepository.CheckForAuthOTP(requestUsername))
                    {


                        this.url = this._configuration[Configuration.NotificationApi];

                        var users = await this._userRepository.ForgotPassword(requestUsername);

                        if (users == null)
                        {
                            return this.BadRequest(new ResponseMessage
                            {
                                Message = EnumHelper.GetEnumName(MessageType.Fail),
                                Description = EnumHelper.GetEnumDescription(MessageType.NotExist)
                            });
                        }
                        string Otp = RandomPassword.GenerateRandomPassword();
                        bool UpdateRecord = true;
                        UserAuthOtp UserAuthOtp = await _userAuthOtp.Get(o => o.UserMasterId == users.ID);
                        if (UserAuthOtp == null)
                        {
                            UpdateRecord = false;
                            UserAuthOtp = new UserAuthOtp();
                        }
                        UserAuthOtp.CreatedDate = DateTime.UtcNow;
                        UserAuthOtp.Otp = Security.Encrypt(Otp);
                        UserAuthOtp.UserMasterId = users.ID;
                        int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                        UserAuthOtp.OtpExpiryDate = UserAuthOtp.CreatedDate.AddMinutes(OtpValidityTime);
                        if (UpdateRecord)
                            await _userAuthOtp.Update(UserAuthOtp);
                        else
                            await _userAuthOtp.Add(UserAuthOtp);

                        this.url += "/UserAuthOtp";

                        JObject oJsonObject = new JObject
                            {
                                { "toEmail", users.ToEmail },
                                { "organizationCode", auth.display.ToLower() },
                                { "userName", users.UserName },
                                { "UserID", users.UserId },
                                { "Otp", Otp }
                            };

                        try
                        {
                            Task.Run(() => CallAPI(url, oJsonObject).Result);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        return Ok(true);
                    }
                    return Ok(false);
                }

               else if (auth.display.ToLower() == "hdbfs" || auth.display.ToLower() == "hdfcrpm" || auth.display.ToLower() == "hdb" || auth.display.ToLower() == "compass")
                {
                    string abc = Security.Encrypt("lmsadmin");
                    string requestUsername;
                    bool IsIOS = Convert.ToBoolean(auth.isIos); 
                    if (IsIOS)
                    {
                        string Key = "a1b2c3d4e5f6g7h8";
                        string _initVector = "1234123412341234";
                        requestUsername = CryptLib.decrypt(auth.username, Key, _initVector);
                    }
                    else
                    {
                        requestUsername = Security.DecryptForUI(auth.username.Replace(" ", "+"));
                        requestUsername = requestUsername.ToLower();
                    }

                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(auth.display);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                        });

                    }

                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    this._userAuthOtp.ChangeDbContext(OrgnizationConnectionString);


                         this.url = this._configuration[Configuration.NotificationApi];

                        var users = await this._userRepository.ForgotPassword(requestUsername);

                        if (users == null)
                        {
                            return this.BadRequest(new ResponseMessage
                            {
                                Message = EnumHelper.GetEnumName(MessageType.Fail),
                                Description = EnumHelper.GetEnumDescription(MessageType.NotExist)
                            });
                        }

                        string Otp = RandomPassword.GenerateRandomPassword();
                        bool UpdateRecord = true;
                        UserAuthOtp UserAuthOtp = await _userAuthOtp.Get(o => o.UserMasterId == users.ID);
                        if (UserAuthOtp == null)
                        {
                            UpdateRecord = false;
                            UserAuthOtp = new UserAuthOtp();
                        }
                        UserAuthOtp.CreatedDate = DateTime.UtcNow;
                        UserAuthOtp.Otp = Security.Encrypt(Otp);
                        UserAuthOtp.UserMasterId = users.ID;
                        int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                        UserAuthOtp.OtpExpiryDate = UserAuthOtp.CreatedDate.AddMinutes(OtpValidityTime);
                        if (UpdateRecord)
                            await _userAuthOtp.Update(UserAuthOtp);
                        else
                            await _userAuthOtp.Add(UserAuthOtp);

                        this.url += "/UserAuthSMSOtp";

                        JObject oJsonObject = new JObject
                            {
                                { "mobileNumber", users.MobileNumber },
                                { "organizationCode", auth.display.ToLower() },
                                { "userName", users.UserName },
                                { "UserID", users.UserId },
                                { "Otp", Otp }
                            };

                        try
                        {
                            Task.Run(() => CallAPI(url, oJsonObject).Result);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        return Ok(true);
                }

                else if (auth.display.ToLower().Contains("sembcorp"))
                {
                    string requestUsername;
                    requestUsername = Security.DecryptForUI(auth.username.Replace(" ", "+"));
                    requestUsername = requestUsername.ToLower();


                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(auth.display);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                        });

                    }

                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    this._userAuthOtp.ChangeDbContext(OrgnizationConnectionString);

                    if (requestUsername.ToLower() != "lmsadmin" && requestUsername.ToLower() != "sammir")
                    {
                        if (await _userRepository.CheckUserTypeForAuthOTP(requestUsername))
                        {
                            this.url = this._configuration[Configuration.NotificationApi];
                            var users = await this._userRepository.ForgotPassword(requestUsername);

                            if (users == null)
                            {
                                return this.BadRequest(new ResponseMessage
                                {
                                    Message = EnumHelper.GetEnumName(MessageType.Fail),
                                    Description = EnumHelper.GetEnumDescription(MessageType.NotExist)
                                });
                            }
                            string Otp = RandomPassword.GenerateRandomPassword();
                            bool UpdateRecord = true;
                            UserAuthOtp UserAuthOtp = await _userAuthOtp.Get(o => o.UserMasterId == users.ID);
                            if (UserAuthOtp == null)
                            {
                                UpdateRecord = false;
                                UserAuthOtp = new UserAuthOtp();
                            }
                            UserAuthOtp.CreatedDate = DateTime.UtcNow;
                            UserAuthOtp.Otp = Security.Encrypt(Otp);
                            UserAuthOtp.UserMasterId = users.ID;
                            int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                            UserAuthOtp.OtpExpiryDate = UserAuthOtp.CreatedDate.AddMinutes(OtpValidityTime);
                            if (UpdateRecord)
                                await _userAuthOtp.Update(UserAuthOtp);
                            else
                                await _userAuthOtp.Add(UserAuthOtp);

                            this.url += "/UserAuthOtp";

                            JObject oJsonObject = new JObject
                            {
                                { "toEmail", users.ToEmail },
                                { "organizationCode", auth.display.ToLower() },
                                { "userName", users.UserName },
                                { "UserID", users.UserId },
                                { "Otp", Otp }
                            };

                            try
                            {
                                Task.Run(() => CallAPI(url, oJsonObject).Result);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            return Ok(true);
                        }
                    }
                    return Ok(false);
                }
                return this.Ok(false);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUserInfo")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Get([FromBody] ApiGetUserDetailsById getuserbyid)
        {
            try
            {
                getuserbyid.ID = Security.DecryptForUI(getuserbyid.ID);
                APIUserMaster user = await this._userRepository.GetUser(Convert.ToInt32(getuserbyid.ID));
                user.Password = null;
                return this.Ok(user);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetUserReject")]
        
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IEnumerable<UserMasterRejected>> Get()
        {
            try
            {
                return await this._userMasterRejectedRepository.GetAll(e => e.IsDeleted == Record.NotDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpGet("GetTeam")]
       
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> GetTeam()
        {

            try
            {
                APIUserTeam user = await _userRepository.GetUserData(UserId);
                string email = Convert.ToString(user.ReportsTo);
                return this.Ok(await this._userRepository.GetTeam(email));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserData")]
        public async Task<IActionResult> GetUserData()
        {
            //Access For End User 
            try
            {

                return this.Ok(await this._userRepository.GetUserData(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserGrade")]
        public async Task<IActionResult> GetUserGrade()
        {
            try
            {

                return this.Ok(await this._userRepository.GetUserGrade(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetProfilePicture")]
        public async Task<IActionResult> GetProfilePicture([FromBody] ApiGetUserDetailsById getuserbyid)
        {

                try
            {
                //Access For End User 
                if (!string.IsNullOrEmpty(getuserbyid.ID))
                    getuserbyid.ID = Security.DecryptForUI(getuserbyid.ID);
                else
                    getuserbyid.ID = UserId.ToString();

                APIUserMaster user = await this._userRepository.GetUser(Convert.ToInt32(getuserbyid.ID));
                  
                var EnableBlobStorage = await _userRepository.GetConfigurationValueAsync("Enable_BlobStorage", OrgCode);

                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                {
                    var file = Path.Combine(this._configuration["ApiGatewayLXPFiles"],
                   user.ProfilePicture);

                    //Check whether file is exists or not at particular location
                    bool isFileExists = System.IO.File.Exists(file);
                    if (isFileExists)
                    {
                        return PhysicalFile(file, "image/png");
                    }
                    else
                    {
                        return PhysicalFile(this._configuration["DefaultProfilePicture"], "image/png");
                    }
                }
                else
                {
                    try
                    {

                        //  return await BlobFile(user.ProfilePicture);
                        return null; // need to update
                   
                    }
                    catch(Exception ex)
                    {
                        return PhysicalFile(this._configuration["DefaultProfilePicture"], "image/png");
                    }  
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        //private async Task<IActionResult> BlobFile(string file)
        //{
        //    BlobDto imgres = await _azurestorage.DownloadAsync(file);
        //    if (imgres != null)
        //    {
        //        if (!string.IsNullOrEmpty(imgres.Name))
        //        {
        //            using (var stream = new MemoryStream())
        //            {
        //                return File(imgres.Content, "image/png");
        //            }
        //        }
        //        else
        //        {

        //            _logger.Error(imgres.ToString());
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        _logger.Error("File not exists");
        //        return null;
        //    }
        //}

        [HttpGet("GetMyProfilePicture")]
        public async Task<IActionResult> GetMyProfilePicture()
        {
            try
            {
                APIUserMaster user = await this._userRepository.GetUser(Convert.ToInt32(UserId.ToString()));
                var EnableBlobStorage = await _userRepository.GetConfigurationValueAsync("Enable_BlobStorage", OrgCode);

                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                {
                    var file = Path.Combine(this._configuration["ApiGatewayLXPFiles"]
                    , user.ProfilePicture);

                    return PhysicalFile(file, "image/png");
                }
                else
                {
                    try
                    {
                        // return await BlobFile(user.ProfilePicture);
                        return null; // need to update
                    }
                    catch (Exception ex)
                    {
                        return PhysicalFile(this._configuration["DefaultProfilePicture"], "image/png");
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetUserSupervisor")]
        public async Task<IActionResult> GetUserSupervisor()
        {
            //Access For End User 
            try
            {
                string email = await _userRepository.GetReportToEmail(UserId);
                return this.Ok(await this._userRepository.GetUserSupervisor(Security.Decrypt(email)));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfigurationGroups/{UserId}/{search}")]

        public async Task<IActionResult> GetAllConfigurationGroups(int UserId, string search)
        {
            try
            {
                var group = await this._userRepository.GetAllConfigurationGroups(UserId, search);
                return this.Ok(group);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserSupervisorId/{userId?}")]
        public async Task<IActionResult> GetUserSupervisorId(string userId = null)
        {
            //Access For End User 
            try
            {
                string email;
                int urid;
                if (userId != null)
                {
                    urid = Convert.ToInt32(Security.DecryptForUI(userId));
                    email = await _userRepository.GetEmail(urid);
                }
                else
                    email = await _userRepository.GetReportToEmail(UserId);

                return this.Ok(await this._userRepository.GetUserSupervisorId(email));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetUserTeam")]
        public async Task<IActionResult> GetUserTeam()
        {
            //Access For End User 
            try
            {
                string email = await _userRepository.GetEmail(UserId);
                return this.Ok(await this._userRepository.GetTeam(Security.Decrypt(email)));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserMyTeam/{userId?}")]
        public async Task<IActionResult> GetUserMyTeam(string userId = null)
        {
            //Access For End User 
            try
            {
                string email;
                int urid;
                if (userId != null)
                {
                    urid = Convert.ToInt32(Security.DecryptForUI(userId));
                    email = await _userRepository.GetEmail(urid);
                }
                else
                    email = await _userRepository.GetEmail(UserId);

                return this.Ok(await this._userRepository.GetMyTeam(email, OrgCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("GetUserByUserName")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Get([FromBody] ApiGetUserName apiGetUserName)
        {
            try
            {
                return this.Ok(await this._userRepository.GetUserNameAndId(apiGetUserName.Name));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUserMasterId")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> GetUserMasterId([FromBody] ApiGetUserMasterId apiGetUserMasterId )
        {
            try
            {
                return this.Ok(await this._userRepository.GetUserMasterId(apiGetUserMasterId.UserID));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetAccountManagers")]
        [PermissionRequired(Permissions.product_manager_Account_Conf + " " + Permissions.product_manager_Customer_Management + " " + Permissions.product_manager_Support_Managment)]
        public async Task<IActionResult> GetAccountManagers([FromBody] APISearch apiSearch)
        {
            try
            {
                return this.Ok(await this._userRepository.GetAccountManagers(apiSearch.Search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("searchUser")]
        public async Task<IActionResult> SearchUser([FromBody] ApiSearchUser searchUser)
        {
            try
            {
                if (searchUser.UserId != null)
                    searchUser.UserId = Security.DecryptForUI(searchUser.UserId);
                if (searchUser.UserType != null)
                    searchUser.UserType = Security.DecryptForUI(searchUser.UserType);

                searchUser.UserType = searchUser.UserType.ToLower().Equals("null") ? null : searchUser.UserType;


                return this.Ok(await this._userRepository.SearchUserType(searchUser.UserId, searchUser.UserType));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("searchTrainer")]
        public async Task<IActionResult> searchTrainer([FromBody] ApiSearchUser searchUser)
        {
            try
            {
                if (searchUser.UserId != null)
                    searchUser.UserId = Security.DecryptForUI(searchUser.UserId);
                if (searchUser.UserType != null)
                    searchUser.UserType = Security.DecryptForUI(searchUser.UserType);

                searchUser.UserType = searchUser.UserType.ToLower().Equals("null") ? null : searchUser.UserType;


                return this.Ok(await this._userRepository.SearchTrainer(searchUser.UserId, searchUser.UserType));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("searchUserdwtc")]
        public async Task<IActionResult> SearchUserdwtc([FromBody] ApiSearchUser searchUser)
        {
            try
            {
                if (!OrgCode.ToLower().Contains("dwtc"))
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

                if (searchUser.UserId != null)
                    searchUser.UserId = Security.DecryptForUI(searchUser.UserId);
                if (searchUser.UserType != null)
                    searchUser.UserType = Security.DecryptForUI(searchUser.UserType);

                searchUser.UserType = searchUser.UserType.ToLower().Equals("null") ? null : searchUser.UserType;
                return this.Ok(await this._userRepository.SearchUserType(searchUser.UserId, searchUser.UserType));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("searchUserByUserRole")]
        [PermissionRequired(Permissions.JobRoleResponsbilities + " " + Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> SearchUserByUserRole([FromBody] ApiName apiName)
        {

            try
            {
                //get userid from token
                string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                if (identity == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }

                if (apiName.Name.Contains('%'))
                    return StatusCode(401, Record.EnterValidUserName);

                int tokenId = Convert.ToInt32(identity);

                return this.Ok(await this._userRepository.SearchByUserRole(tokenId, apiName.Name));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("GetUserCustomerCode")]
        public async Task<IActionResult> GetUserCustomerCode()
        {
            try
            {
                string CustomerCode = Security.Decrypt(_identitySvc.GetCustomerCode());
                if (CustomerCode == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }

                APISectionAdminDetails aPISectionAdminDetails = await this._userRepository.GetSectionAdminDetails(UserId);
                APISectionDetails aPISectionDetails = new APISectionDetails
                {
                    CustomerCode = CustomerCode
                };

                if (UserRole.ToLower().Equals("ba"))
                {
                    aPISectionDetails.SectionDetail = "business";
                    aPISectionDetails.SectionDetailValue = aPISectionAdminDetails.Business;
                }
                else if (UserRole.ToLower().Equals("ga"))
                {
                    aPISectionDetails.SectionDetail = "group";
                    aPISectionDetails.SectionDetailValue = aPISectionAdminDetails.Group;
                }
                else if (UserRole.ToLower().Equals("aa"))
                {
                    aPISectionDetails.SectionDetail = "area";
                    aPISectionDetails.SectionDetailValue = aPISectionAdminDetails.Area;
                }
                else if (UserRole.ToLower().Equals("la"))
                {
                    aPISectionDetails.SectionDetail = "location";
                    aPISectionDetails.SectionDetailValue = aPISectionAdminDetails.Location;
                }

                return Ok(aPISectionDetails);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("GetAllUser")]
       
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.iltnominate)]
        public async Task<IActionResult> Get([FromBody] APIUserSearch apiUserSearch)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (apiUserSearch.Search != null)
                        apiUserSearch.Search = apiUserSearch.Search.ToLower().Equals("null") ? null : apiUserSearch.Search;
                    if (apiUserSearch.ColumnName != null)
                        apiUserSearch.ColumnName = apiUserSearch.ColumnName.ToLower().Equals("null") ? null : apiUserSearch.ColumnName;
                    if (apiUserSearch.Status != null)
                        apiUserSearch.Status = apiUserSearch.Status.ToLower().Equals("null") ? null : apiUserSearch.Status;

                    if (apiUserSearch.Search != null)
                    {
                        string Key = "a1b2c3d4e5f6g7h8";
                        string _initVector = "1234123412341234";

                        if (!IsiOS)
                            apiUserSearch.Search = Security.DecryptForUI(apiUserSearch.Search);
                        else
                            apiUserSearch.Search = CryptLib.decrypt((apiUserSearch.Search), Key, _initVector);
                    }


                    if (apiUserSearch.ColumnName != null)
                    {
                        string Key = "a1b2c3d4e5f6g7h8";
                        string _initVector = "1234123412341234";

                        if (!IsiOS)
                            apiUserSearch.ColumnName = Security.DecryptForUI(apiUserSearch.ColumnName);

                        else
                            apiUserSearch.ColumnName = CryptLib.decrypt((apiUserSearch.ColumnName), Key, _initVector);
                    }

                    IEnumerable<APIUserMaster> users = await this._userRepository.GetAllUser(apiUserSearch.Page, apiUserSearch.PageSize, apiUserSearch.Search, apiUserSearch.ColumnName, apiUserSearch.Status, UserId, Security.Encrypt(UserName), OrgCode);
                    return this.Ok(users);
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest();
            }
        }



        [HttpPost("GetAllUserReject")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.iltnominate)]
        public async Task<IActionResult> GetUserReject([FromBody] APIUserSearch apiUserSearch)
        {
            try
            {
                var userReject = await this._userMasterRejectedRepository.GetAllUserReject(apiUserSearch.Page, apiUserSearch.PageSize, apiUserSearch.Search);
                return Ok(userReject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("UserReject/GetTotalRecords")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> GetCount([FromBody] APISearch apiSearch)
        {
            try
            {
                var userReject = await this._userMasterRejectedRepository.Count(apiSearch.Search);
                return Ok(userReject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

            }
        }




        [HttpPost("GetTotalRecords")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.iltnominate)]
        public async Task<IActionResult> GetCount([FromBody] APIUserSearch apiUserSearch)
        {

            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });


                if (apiUserSearch.Search != null)
                    apiUserSearch.Search = apiUserSearch.Search.ToLower().Equals("null") ? null : apiUserSearch.Search;
                if (apiUserSearch.ColumnName != null)
                    apiUserSearch.ColumnName = apiUserSearch.ColumnName.ToLower().Equals("null") ? null : apiUserSearch.ColumnName;
                if (apiUserSearch.Status != null)
                    apiUserSearch.Status = apiUserSearch.Status.ToLower().Equals("null") ? null : apiUserSearch.Status;


                if (apiUserSearch.Search != null)
                    apiUserSearch.Search = Security.DecryptForUI(apiUserSearch.Search);
                if (apiUserSearch.ColumnName != null)
                    apiUserSearch.ColumnName = Security.DecryptForUI(apiUserSearch.ColumnName);



                var count = await this._userRepository.GetUserCount(apiUserSearch.Search, apiUserSearch.ColumnName, apiUserSearch.Status, UserId, Security.Encrypt(UserName));
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




        [HttpPost("Exist")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Exist([FromBody] UserSearch apiUserSearch)
        {
            try
            {
                apiUserSearch.SearchByColumn = Security.DecryptForUI(apiUserSearch.SearchByColumn);
                apiUserSearch.SearchText = Security.DecryptForUI(apiUserSearch.SearchText);

                if (apiUserSearch.SearchByColumn.ToLower().Equals("email"))
                {
                    if (await this._userRepository.EmailExists(apiUserSearch.SearchText))
                        return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.EmailIdExist) });
                }
                else if (apiUserSearch.SearchByColumn.ToLower().Equals("mobile"))
                {
                    if (await this._userRepository.MobileExists(apiUserSearch.SearchText))
                        return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.MobileExist) });
                }
                else if (apiUserSearch.SearchByColumn.ToLower().Equals("userid"))
                {
                    if (await this._userRepository.IsExists(apiUserSearch.SearchText))
                        return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.UserIdExist) });
                }

                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

            }
        }



        [HttpPost("ExistsForUpdate")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> ExistsForUpdate([FromBody] ApiUserSearchById userSearchById)
        {
            try
            {
                userSearchById.SearchByColumn = Security.DecryptForUI(userSearchById.SearchByColumn);
                userSearchById.SearchText = Security.DecryptForUI(userSearchById.SearchText);
                userSearchById.Id = Security.DecryptForUI(userSearchById.Id);

                return Ok(await this._userRepository.ExistsForUpdate(userSearchById.SearchByColumn, userSearchById.SearchText, Convert.ToInt32(userSearchById.Id)));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("TermsConditionsAccepted")]
        public async Task<IActionResult> TermsConditionsAccepted()
        {
            try
            {
                int? loginReminderDays;
                int? daysRemainedtoExpirepwd;
                PasswordChangeAlert passwordChangeAlert = new PasswordChangeAlert();
                bool termsConditionsAccepted;
                bool isPasswordModified;

                var allowPasswordReminder = await _userRepository.GetConfigurationValueAsync("ALLOW_PASSWORD_REMINDER", OrgCode);
                if (allowPasswordReminder.ToUpper().ToString() == "YES")
                {
                    if (IsLDAP && !IsExternalUser)
                    {
                        loginReminderDays = 29;
                        termsConditionsAccepted = true;
                        isPasswordModified = true;
                    }
                    else
                    {
                        loginReminderDays = await this._userRepository.LoginReminderDays(UserId, OrgCode);
                        if (OrgCode == "bandhan" || OrgCode=="ltfs" || OrgCode == "ltfsuat" || OrgCode == "bandhanbank" )
                        {
                            daysRemainedtoExpirepwd = await _userRepository.LoginPasswordChangeReminderDays(UserId, OrgCode);
                            passwordChangeAlert.daysRemainedtoExpirePassword = daysRemainedtoExpirepwd;
                            if (daysRemainedtoExpirepwd <= 7)
                            {
                                passwordChangeAlert.displayPasswordChangeAlert = true;
                                if (daysRemainedtoExpirepwd > 0)
                                    passwordChangeAlert.alertMessage = "Please Change Login Password within " + daysRemainedtoExpirepwd + " days.";
                                else
                                    passwordChangeAlert.alertMessage = "Please Change Your Password.It has Expired.";
                            }
                        }
                        var enableTermsAndConditions = await _userRepository.GetConfigurationValueAsync("ENABLE_TANDC_ACCPTED", OrgCode);
                        if (enableTermsAndConditions.ToUpper().ToString() == "NO")
                            termsConditionsAccepted = true;
                        else
                            termsConditionsAccepted = await this._userRepository.TermsConditionsAccepted(UserId);

                        isPasswordModified = await this._userRepository.IsPasswordModified(UserId);
                    }
                }
                else
                {
                    loginReminderDays = 29;
                    termsConditionsAccepted = true;
                    isPasswordModified = true;
                }

                return Ok(new { termsConditionsAccepted, isPasswordModified, loginReminderDays, passwordChangeAlert });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("TermsConditionsAccepted")]
        public async Task<IActionResult> UserTermsConditionsAccepted()
        {

            if (ModelState.IsValid)
            {

                APIUserMaster oldUser = await this._userRepository.GetUser(UserId);
                if (oldUser == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                }
                oldUser.UserId = Security.Encrypt(oldUser.UserId);
                oldUser.MobileNumber = Security.Encrypt(oldUser.MobileNumber);
                oldUser.EmailId = Security.Encrypt(oldUser.EmailId);
                oldUser.ReportsTo = Security.Encrypt(oldUser.ReportsTo == null ? null : oldUser.ReportsTo.ToLower());
                oldUser.ModifiedDate = DateTime.UtcNow;
                oldUser.AcceptanceDate = DateTime.UtcNow;
                oldUser.TermsCondintionsAccepted = true;
                await this._userRepository.UpdateUser(oldUser);
                bool TermsConditionsAccepted = true;
                return this.Ok(new { TermsConditionsAccepted });
            }
            return this.BadRequest(this.ModelState);
        }

        [HttpDelete]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                if(DecryptedId == UserId)
                   return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.CannotDeleteYourself) });
                UserMasterLogs _userMasterLogs = new UserMasterLogs();

                if (OrgCode.ToLower() == "tcl" && UserRole == "UAM")
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NoPermission) });
                }

                int Result = await this._userRepository.Delete(DecryptedId, UserId);
                if (Result == -1)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = "You are trying to delete an Active user. Please change the user status to Inactive and try again." });

                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });

                if (Result > 0)
                {
                    _userMasterLogs.ModifiedBy = UserId;
                    _userMasterLogs.IsDeleted = 1;
                    _userMasterLogs.UserId = DecryptedId;
                    await this._userRepository.AddUserMasterLogs(_userMasterLogs);
                    return this.Ok();
                }
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("DeleteUser")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Delete([FromBody] ApiUserId apiUserId)
        {
            try
            {
                apiUserId.UserId = Security.DecryptForUI(apiUserId.UserId);
                if (Convert.ToInt32(apiUserId.UserId) == UserId)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.CannotDeleteYourself) });

                int Result = await this._userRepository.Delete(Convert.ToInt32(apiUserId.UserId), UserId);
                if (Result == -1)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = "You are trying to delete an Active user. Please change the user status to Inactive and try again." });

                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                if (Result > 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependencyExist) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Export")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Export()
        {
            try
            {
                var userSetting = await this._userSettingsRepository.GetUserSetting(OrgCode);
                List<APIUserSetting> aPIUserSettings = new List<APIUserSetting>();
                aPIUserSettings = Mapper.Map<List<APIUserSetting>>(userSetting);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @UserMasterImportField.UserMasterxlsx;
                string URL = string.Concat(DomainName, OrgCode, "/", sFileName);
                if (!Directory.Exists(sWebRootFolder))
                {
                    Directory.CreateDirectory(sWebRootFolder);
                }
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(UserMasterImportField.UserMaster);
                    //First add the headers
                    worksheet.Cells[1, 1].Value = UserMasterImportField.UserId + Record.Star;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    if (IsInstitute.ToLower() == "false")
                    {
                        if (OrgCode.ToLower() == "bandhan" || OrgCode.ToLower() == "bandhanbank" )
                        {
                            worksheet.Cells[1, 2].Value = UserMasterImportField.EmailId;
                           
                        }
                        else
                        {
                            worksheet.Cells[1, 2].Value = UserMasterImportField.EmailId + Record.Star;
                            worksheet.Cells[1, 2].Style.Font.Bold = true;
                        }
                    }
                    else
                    {
                        worksheet.Cells[1, 2].Value = UserMasterImportField.EmailId;


                    }
                    worksheet.Cells[1, 3].Value = UserMasterImportField.UserName + Record.Star;
                    worksheet.Cells[1, 3].Style.Font.Bold = true;
                    if (IsInstitute.ToLower() == "false")
                    {
                        if(OrgCode.ToLower() == "tcl")
                        {
                        worksheet.Cells[1, 4].Value = UserMasterImportField.MobileNumber;

                        }
                        else
                        {
                            worksheet.Cells[1, 4].Value = UserMasterImportField.MobileNumber + Record.Star;
                            worksheet.Cells[1, 4].Style.Font.Bold = true;
                        }
                    }
                    else

                    {
                        worksheet.Cells[1, 4].Value = UserMasterImportField.MobileNumber;

                    }

                    int i = 5;
                    foreach (APIUserSetting opt in aPIUserSettings)
                    {
                        UserSettings userSettings = new UserSettings
                        {
                            ChangedColumnName = opt.ChangedColumnName
                        };
                        string ChangedColumnName = userSettings.ChangedColumnName.ToString();
                        bool ismandatory = opt.IsMandatory;
                        if (ismandatory == true)
                        {
                            worksheet.Cells[1, i].Value = ChangedColumnName + Record.Star;
                            worksheet.Cells[1, i].Style.Font.Bold = true;
                        }
                        else
                        {
                            worksheet.Cells[1, i].Value = ChangedColumnName;
                        }
                        i++;
                    }
                    worksheet.Cells[1, i++].Value = UserMasterImportField.Language;
                    worksheet.Cells[1, i++].Value = UserMasterImportField.Currency;
                    if (OrgCode.ToLower() == "bandhan" || OrgCode.ToLower() == "bandhanbank")
                    {
                        int ii = i;
                        worksheet.Cells[1, ii].Value = UserMasterImportField.DateOfBirth + Record.Star;
                        worksheet.Cells[1, ii].Style.Font.Bold = true;
                        i++;
                    }
                    else
                    {
                        worksheet.Cells[1, i++].Value = UserMasterImportField.DateOfBirth;
                    }
                       
                    for (int row = 1;row<10000;row++)
                    {
                        worksheet.Cells[row, i - 1].Style.Numberformat.Format = "@";
                    }
                    worksheet.Cells[1, i++].Value = UserMasterImportField.DateOfJoining;
                    for (int row = 1; row < 10000; row++)
                    {
                        worksheet.Cells[row, i - 1].Style.Numberformat.Format = "@";
                    }
                    worksheet.Cells[1, i++].Value = UserMasterImportField.AccountExpiryDate;
                    for (int row = 1; row < 10000; row++)
                    {
                        worksheet.Cells[row, i - 1].Style.Numberformat.Format = "@";
                    }
                    worksheet.Cells[1, i++].Value = UserMasterImportField.Gender;
                    worksheet.Cells[1, i++].Value = UserMasterImportField.ReportsTo;
                    worksheet.Cells[1, i++].Value = UserMasterImportField.UserType;
                    worksheet.Cells[1, i++].Value = UserMasterImportField.UserRole;


                    var FIXED_JOB_ROLE = await _userRepository.GetConfigurationValueAsync("FIXED_JOB_ROLE", OrgCode);
                    if (FIXED_JOB_ROLE.ToString().ToUpper() == "YES")
                    {
                        worksheet.Cells[1, i++].Value = UserMasterImportField.JobRole;
                        worksheet.Cells[1, i++].Value = UserMasterImportField.DateIntoRole;
                    }
                    string[] configvalue = { "HRBP_Rename", "Mentor_Rename", "Companion_Rename" };
                    List<AppConfiguration> renameConfigValues = await _userRepository.GetUserConfigurationValueAsync(configvalue,OrgCode,"");
                    worksheet.Cells[1, i++].Value = renameConfigValues.Where(a=>a.Code== "Companion_Rename").Select(a=>a.value).FirstOrDefault();
                    worksheet.Cells[1, i++].Value = renameConfigValues.Where(a => a.Code == "Mentor_Rename").Select(a => a.value).FirstOrDefault();
                    worksheet.Cells[1, i++].Value = renameConfigValues.Where(a => a.Code == "HRBP_Rename").Select(a => a.value).FirstOrDefault();



                    package.Save(); //Save the workbook.
                }
                //return this.Ok(URL);
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel, @UserMasterImportField.UserMasterWithData);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("ExportSearch")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> ExportSearch([FromBody] APIUserSearch apiUserSearch)
        {
            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string sFileName = @UserMasterImportField.UserMasterWithData;
            string DomainName = this._configuration["ApiGatewayUrl"];
            string URL = string.Concat(DomainName, OrgCode, "/", sFileName);
            try
            {

                int downloadcount = 0;

                List<ExportFileLog> exportFileLog = await _exportFileLog.GetAll(o => o.Token == Token && o.ServiceName == "User");
                if (exportFileLog != null)
                {
                    downloadcount = exportFileLog.Count;
                }

                if (downloadcount < 5)
                {
                    ExportFileLog exportFileLogNew = new ExportFileLog
                    {
                        ServiceName = "User",
                        Token = Token,
                        Count = downloadcount + 1
                    };
                    await _exportFileLog.Add(exportFileLogNew);


                    if (apiUserSearch.ColumnName != null)
                        apiUserSearch.ColumnName = apiUserSearch.ColumnName.ToLower().Equals("null") ? null : apiUserSearch.ColumnName;
                    if (apiUserSearch.Status != null)
                        apiUserSearch.Status = apiUserSearch.Status.ToLower().Equals("null") ? null : apiUserSearch.Status;
                    if (apiUserSearch.Search != null)
                        apiUserSearch.Search = apiUserSearch.Search.ToLower().Equals("null") ? null : apiUserSearch.Search;

                    IEnumerable<APIUserMaster> users = await this._userRepository.GetAllUser(1, 65536, apiUserSearch.Search, apiUserSearch.ColumnName, apiUserSearch.Status, UserId, Security.Encrypt(UserName));


                    FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                    if (file.Exists)
                    {
                        file.Delete();
                        file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                    }
                    using (ExcelPackage package = new ExcelPackage(file))
                    {
                        // add a new worksheet to the empty workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(UserMasterImportField.UserMasterData);
                        //First add the headers

                        var userSettings = await this._userSettingsRepository.GetUserSetting(OrgCode);
                        var roles = await this._rolesRepository.GetAll(e => e.IsDeleted == Record.NotDeleted);
                        bool business = false;
                        bool group = false;
                        bool area = false;
                        bool location = false;
                        bool configurationColumn1 = false, configurationColumn2 = false, configurationColumn3 = false, configurationColumn4 = false, configurationColumn5 = false, configurationColumn6 = false, configurationColumn7 = false, configurationColumn8 = false, configurationColumn9 = false, configurationColumn10 = false, configurationColumn11 = false, configurationColumn12 = false;
                        int headingColumn = 1;

                        worksheet.Cells[1, headingColumn++].Value = "User Id";
                        worksheet.Cells[1, headingColumn++].Value = "User Name";
                        worksheet.Cells[1, headingColumn++].Value = "Email Id";
                        worksheet.Cells[1, headingColumn++].Value = "Mobile Number";
                        worksheet.Cells[1, headingColumn++].Value = "Gender";
                        worksheet.Cells[1, headingColumn++].Value = "User Role";
                        worksheet.Cells[1, headingColumn++].Value = "Account Created Date";
                        worksheet.Cells[1, headingColumn++].Value = "Account Expiry Date";
                        worksheet.Cells[1, headingColumn++].Value = "Status";
                        worksheet.Cells[1, headingColumn++].Value = "TimeZone";
                        worksheet.Cells[1, headingColumn++].Value = "Currency";
                        worksheet.Cells[1, headingColumn++].Value = "Language";
                        worksheet.Cells[1, headingColumn++].Value = "Reports To";
                        worksheet.Cells[1, headingColumn++].Value = "Date Of Birth";
                        worksheet.Cells[1, headingColumn++].Value = "Date Of Joining";
                        worksheet.Cells[1, headingColumn++].Value = "User Type";


                        foreach (var userSetting in userSettings)
                        {
                            switch (userSetting.ConfiguredColumnName)
                            {
                                case "Business":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    business = true;
                                    break;
                                case "Group":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    group = true;
                                    break;
                                case "Area":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    area = true;
                                    break;
                                case "Location":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    location = true;
                                    break;
                                case "ConfigurationColumn1":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn1 = true;
                                    break;
                                case "ConfigurationColumn2":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn2 = true;
                                    break;
                                case "ConfigurationColumn3":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn3 = true;
                                    break;
                                case "ConfigurationColumn4":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn4 = true;
                                    break;
                                case "ConfigurationColumn5":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn5 = true;
                                    break;
                                case "ConfigurationColumn6":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn6 = true;
                                    break;
                                case "ConfigurationColumn7":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn7 = true;
                                    break;
                                case "ConfigurationColumn8":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn8 = true;
                                    break;
                                case "ConfigurationColumn9":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn9 = true;
                                    break;
                                case "ConfigurationColumn10":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn10 = true;
                                    break;
                                case "ConfigurationColumn11":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn11 = true;
                                    break;
                                case "ConfigurationColumn12":
                                    worksheet.Cells[1, headingColumn++].Value = userSetting.ChangedColumnName;
                                    configurationColumn12 = true;
                                    break;
                                default:
                                    break;
                            }
                        }



                        int row = 2, column = 1;
                        DateTime dateValue = new DateTime();
                        foreach (APIUserMaster user in users)
                        {
                            worksheet.Cells[row, column++].Value = user.UserId;
                            worksheet.Cells[row, column++].Value = user.UserName;
                            worksheet.Cells[row, column++].Value = user.EmailId;
                            worksheet.Cells[row, column++].Value = user.MobileNumber;
                            worksheet.Cells[row, column++].Value = user.Gender;
                            var role = roles.Find(x => x.RoleCode == user.UserRole);
                            if (role != null)
                            {
                                worksheet.Cells[row, column++].Value = role.RoleName;
                            }
                            else
                            {
                                worksheet.Cells[row, column++].Value = user.UserRole;
                            }
                            if (DateTime.TryParse(user.AccountCreatedDate.ToString(), out DateTime outputDateTimeValue))
                            {
                                dateValue = outputDateTimeValue;

                                if (dateValue == DateTime.MinValue)
                                    worksheet.Cells[row, column++].Value = string.Empty;
                                else
                                    worksheet.Cells[row, column++].Value = dateValue.ToString("MMM dd, yyyy").ToString();
                            }
                            else
                            {
                                worksheet.Cells[row, column++].Value = string.Empty;
                            }
                            worksheet.Cells[row, column++].Value = this.DateValidation(user.AccountExpiryDate);
                            worksheet.Cells[row, column++].Value = user.IsActive == true ? "Active" : "Inactive";
                            worksheet.Cells[row, column++].Value = user.TimeZone;
                            worksheet.Cells[row, column++].Value = user.Currency;
                            worksheet.Cells[row, column++].Value = user.Language;
                            worksheet.Cells[row, column++].Value = user.ReportsTo;
                            worksheet.Cells[row, column++].Value = this.DateValidation(user.DateOfBirth);
                            worksheet.Cells[row, column++].Value = this.DateValidation(user.DateOfJoining);
                            worksheet.Cells[row, column++].Value = user.UserType;

                            if (business)
                            {
                                worksheet.Cells[row, column++].Value = user.Business;
                            }

                            if (group)
                            {
                                worksheet.Cells[row, column++].Value = user.Group;
                            }

                            if (area)
                            {
                                worksheet.Cells[row, column++].Value = user.Area;
                            }
                            if (location)
                            {
                                worksheet.Cells[row, column++].Value = user.Location;
                            }
                            if (configurationColumn1)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn1;
                            }

                            if (configurationColumn2)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn2;
                            }
                            if (configurationColumn3)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn3;
                            }
                            if (configurationColumn4)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn4;
                            }
                            if (configurationColumn5)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn5;
                            }
                            if (configurationColumn6)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn6;
                            }
                            if (configurationColumn7)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn7;
                            }
                            if (configurationColumn8)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn8;
                            }
                            if (configurationColumn9)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn9;
                            }
                            if (configurationColumn10)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn10;
                            }
                            if (configurationColumn11)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn11;
                            }
                            if (configurationColumn12)
                            {
                                worksheet.Cells[row, column++].Value = user.ConfigurationColumn12;
                            }

                            row++;
                            column = 1;
                        }

                        package.Save(); //Save the workbook.
                    }
                }
                else
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.LimitExtended), Description = EnumHelper.GetEnumDescription(MessageType.LimitExtended) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return this.Ok(URL);
        }



        [HttpPost("ExportData")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> ExportData([FromBody] APIUserSearch apiUserSearch)
        {
            try
            {
                FileInfo ExcelFile;
                apiUserSearch.ColumnName = Security.DecryptForUI(apiUserSearch.ColumnName);
                apiUserSearch.Search = Security.DecryptForUI(apiUserSearch.Search);
                var FIXED_JOB_ROLE = await _userRepository.GetConfigurationValueAsync("FIXED_JOB_ROLE", OrgCode);

                ExcelFile = await this._userRepository.ExportUserData(apiUserSearch.ColumnName, apiUserSearch.Status, apiUserSearch.Search, UserId, Security.Encrypt(UserName), FIXED_JOB_ROLE, OrgCode);

                var Fs = ExcelFile.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                if (ExcelFile.Exists)
                {
                    ExcelFile.Delete();

                }
                //return File(fileData, FileContentType.Excel, @UserMasterImportField.UserMasterWithData);
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        private string DateValidation(DateTime? date)
        {
            DateTime dateValue = new DateTime();
            if (DateTime.TryParse(date.ToString(), out DateTime outputDateTimeValue))
            {
                dateValue = outputDateTimeValue;

                if (dateValue == DateTime.MinValue)
                    return string.Empty;
                else
                    return dateValue.ToString("MMM dd, yyyy").ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Post([FromBody] APIUserMaster apiUser)
        {
            try
            {
                UserMasterLogs _userMasterLogs = new UserMasterLogs();

                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);
                if(OrgCode.ToLower() != "tcl")
                {
                    if (IsInstitute.ToLower() == "false" && (apiUser.EmailId == "" || apiUser.MobileNumber == ""))
                        return this.BadRequest(new ResponseMessage { Message = "Email/Mobile No is required", Description = "Please check Email/Mobile. It  is required field" });
                }
                else
                {
                    if (IsInstitute.ToLower() == "false" && (apiUser.EmailId == ""))
                        return this.BadRequest(new ResponseMessage { Message = "Email is required", Description = "Please check Email. It  is required field" });
                }
                
                var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");

                var allowUserNameValidation = await _userRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode);
                if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                {
                    if (!UserNameRegExpression.IsMatch(apiUser.UserName))
                    {
                        return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                    }
                }
                apiUser.CreatedBy = UserId;
                apiUser.ModifiedBy = UserId;
                apiUser.OrganizationCode = OrgCode;

                if (apiUser.EmailId != "")
                {
                    if (apiUser.EmailId == apiUser.ReportsTo)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.SelfReportToNotAllow), Description = EnumHelper.GetEnumDescription(MessageType.SelfReportToNotAllow) });
                }
                if (await this._userRepository.IsUserRoleExist(apiUser.UserRole))
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                if (apiUser.AccountExpiryDate != null)
                {
                    if (apiUser.AccountExpiryDate <= DateTime.UtcNow.Date)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.AccountExpiryDate), Description = EnumHelper.GetEnumDescription(MessageType.AccountExpiryDate) });
                }

                string[] configvalue = { "HRBP_Rename", "Mentor_Rename", "Companion_Rename" };
                List<AppConfiguration> renameConfigValues = await _userRepository.GetUserConfigurationValueAsync(configvalue, OrgCode, "");
                string companion = renameConfigValues.Where(a => a.Code == "Companion_Rename").Select(a => a.value).FirstOrDefault();
                string mentor = renameConfigValues.Where(a => a.Code == "Mentor_Rename").Select(a => a.value).FirstOrDefault();
                string hrbp = renameConfigValues.Where(a => a.Code == "HRBP_Rename").Select(a => a.value).FirstOrDefault();
                if (apiUser.BuddyTrainerId != null || apiUser.MentorId != null || apiUser.HRBPId != null)
                {
                    if (apiUser.BuddyTrainerId == apiUser.MentorId && (apiUser.BuddyTrainerId != null && apiUser.MentorId != null) && (apiUser.BuddyTrainerId != 0 && apiUser.MentorId != 0))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumDescription(MessageType.DataNotValid), Description = "The " + companion + " you're trying to map is already mapped as " + mentor + " to the selected user. Please select another user for your intended purpose." });

                    else if (apiUser.BuddyTrainerId == apiUser.HRBPId && (apiUser.BuddyTrainerId != null && apiUser.HRBPId !=null) && (apiUser.BuddyTrainerId != 0 && apiUser.HRBPId != 0))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumDescription(MessageType.DataNotValid), Description = "The " + companion + " you're trying to map is already mapped as " + hrbp + " to the selected user. Please select another user for your intended purpose." });

                    else if (apiUser.HRBPId == apiUser.MentorId && (apiUser.HRBPId != null && apiUser.MentorId != null) && (apiUser.HRBPId != 0 && apiUser.MentorId != 0))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumDescription(MessageType.DataNotValid), Description = "The " + hrbp + " you're trying to map is already mapped as " + mentor + " to the selected user. Please select another user for your intended purpose." });
                }

                if (apiUser.TermsCondintionsAccepted != false || apiUser.AcceptanceDate != null || apiUser.HouseId != null || (!string.IsNullOrEmpty(apiUser.House)))
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                // string IsInstitute =_identitySvc.GetIsInstitute();
                int Result = await this._userRepository.AddUser(apiUser, UserRole, OrgCode, IsInstitute);
                if (Result == 0)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                if (Result == 3)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });

                if (Result == 4)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.BirthdateNotValid), Description = EnumHelper.GetEnumDescription(MessageType.BirthdateNotValid) });

                if (Result == 2)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                if (Result == 5)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.MissingBirthDate) });

                apiUser.Password = null;
                if (Result == 1)
                {
                    _userMasterLogs.ModifiedBy = UserId;
                    _userMasterLogs.IsInserted = 1;
                    _userMasterLogs.UserId = apiUser.Id;
                    await this._userRepository.AddUserMasterLogs(_userMasterLogs);
                }
                return this.Ok(apiUser);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("SaveFileData")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> PostFile([FromBody] APIFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                //get customerCode from token
                if (OrgCode == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }

                string result = await this._userRepository.ProcessImportFile(aPIFilePath, UserId, OrgCode);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpPost]
        [Route("ProfilePictureUpload")]
        public async Task<IActionResult> ProfilePictureUpload([FromBody] PictureProfile pictureProfile)
        {
            try
            {


                if (string.IsNullOrEmpty(pictureProfile.Base64String) && string.IsNullOrEmpty(pictureProfile.url))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (!string.IsNullOrEmpty(pictureProfile.Base64String))
                {
                    string[] str = pictureProfile.Base64String.Split(',');

                    if (str[0] == "data:image/jpeg;base64" || str[0] == "data:image/png;base64" || str[0] == "data:image/jpg;base64" || str[0] == "data:image/bmp;base64")
                    {
                        var bytes = Convert.FromBase64String(str[1]);
                        var EnableBlobStorage = await _userRepository.GetConfigurationValueAsync("Enable_BlobStorage", OrgCode);
                        _logger.Info("pdf EnableBlobStorage : " + EnableBlobStorage);

                        if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                        {
                            string fileDir = this._configuration["ApiGatewayLXPFiles"];
                            fileDir = Path.Combine(fileDir, OrgCode);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            //if (str[0] == "data:image/jpeg;base64" || str[0] == "data:image/png;base64" || str[0] == "data:image/jpg;base64" || str[0] == "data:image/bmp;base64")
                            //{
                            //    var bytes = Convert.FromBase64String(str[1]);
                            //    string fileDir = this._configuration["ApiGatewayLXPFiles"];
                            //    fileDir = Path.Combine(fileDir, OrgCode);
                            //    if (!Directory.Exists(fileDir))
                            //    {
                            //        Directory.CreateDirectory(fileDir);
                            //    }

                            string file = Path.Combine(fileDir, DateTime.UtcNow.Ticks + ".png");
                            if (bytes.Length > 0)
                            {

                                string fileclass = bytes[0].ToString();
                                fileclass += bytes[1].ToString();
                                if (fileclass == "255216" || fileclass == "7173" || fileclass == "13780" || fileclass == "6677")//3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png
                                {
                                    using (var stream = new FileStream(file, FileMode.Create))
                                    {

                                        stream.Write(bytes, 0, bytes.Length);
                                        stream.Flush();
                                    }
                                }
                                else
                                {
                                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                                }
                            }

                            if (string.IsNullOrEmpty(file))
                            {
                                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                            }

                            string fileName = file.Substring(file.LastIndexOf(OrgCode)).Replace(@"\", "/");
                            await this._userRepository.UpdateProfilePicture(fileName, UserId);

                            return Ok();
                        }
                        else
                        {
                            BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode, null, null);
                            if (res != null)
                            {
                                if (res.Error == false)
                                {
                                    string filePath = res.Blob.Name.ToString();
                                    await this._userRepository.UpdateProfilePicture(filePath.Replace(@"\", "/"), UserId);
                                    return Ok();

                                }
                                else
                                {
                                    _logger.Error(res.ToString());
                                }
                            }
                            return null;
                        }
                    }
                    else
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                    }

                }
                else
                {
                    await this._userRepository.UpdateProfilePicture(pictureProfile.url, UserId);
                    return Ok();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost]
        [Route("UserMasterProfilePictureUpload")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> UserMasterProfilePictureUpload([FromBody] PictureProfile pictureProfile)
        {
            try
            {
                pictureProfile.UserId = Security.DecryptForUI(pictureProfile.UserId);

                if (string.IsNullOrEmpty(pictureProfile.Base64String))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                string[] str = pictureProfile.Base64String.Split(',');

                if (str[0] == "data:image/jpeg;base64" || str[0] == "data:image/png;base64" || str[0] == "data:image/jpg;base64" || str[0] == "data:image/bmp;base64")
                {
                    var bytes = Convert.FromBase64String(str[1]);
                    var EnableBlobStorage = await _userRepository.GetConfigurationValueAsync("Enable_BlobStorage", OrgCode);
                    _logger.Info("pdf EnableBlobStorage : " + EnableBlobStorage);

                    if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                    {
                        string fileDir = this._configuration["ApiGatewayLXPFiles"];
                        fileDir = Path.Combine(fileDir, OrgCode);
                        if (!Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }

                        string file = Path.Combine(fileDir, DateTime.UtcNow.Ticks + ".png");
                        if (bytes.Length > 0)
                        {

                            string fileclass = bytes[0].ToString();
                            fileclass += bytes[1].ToString();
                            // allowed .pdf,video,images
                            if (fileclass == "255216" || fileclass == "7173" || fileclass == "13780" || fileclass == "6677")//3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png
                            {
                                using (var stream = new FileStream(file, FileMode.Create))
                                {

                                    stream.Write(bytes, 0, bytes.Length);
                                    stream.Flush();
                                }
                            }
                            else
                            {
                                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                            }
                        }

                        if (string.IsNullOrEmpty(file))
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }

                        string fileName = file.Substring(file.LastIndexOf(OrgCode)).Replace(@"\", "/");
                        APIUserMaster apiUser = await this._userRepository.GetUserByUserId(pictureProfile.UserId);

                        if (apiUser != null)
                        {
                            await this._userRepository.UpdateProfilePicture(fileName, apiUser.Id);
                        }

                        return this.Ok(file.Substring(file.LastIndexOf(OrgCode)).Replace(@"\", "/"));
                    }
                    else
                    {
                        BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode, null, null);
                        if (res != null)
                        {
                            if (res.Error == false)
                            {
                                string filePath = res.Blob.Name.ToString();
                                APIUserMaster apiUser = await this._userRepository.GetUserByUserId(pictureProfile.UserId);

                                if (apiUser != null)
                                {
                                    await this._userRepository.UpdateProfilePicture(filePath.Replace(@"\", "/"), apiUser.Id);
                                }
                                return this.Ok(filePath.Replace(@"\", "/"));
                            }
                            else
                            {
                                _logger.Error(res.ToString());
                            }
                        }
                        return null;
                    }
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                }

            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                //Truncate UserMasterRejected Record
                _userMasterRejectedRepository.delete();
                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.XLSX : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        string filename = fileUpload.FileName;
                        string[] fileaary = filename.Split('.');
                        string fileextention = fileaary[1].ToLower();
                        string filex = Record.XLSX;
                        if (fileextention != filex)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });
                        }
                        string fileDir = this._configuration["ApiGatewayWwwroot"];
                        fileDir = Path.Combine(fileDir, OrgCode);
                        fileDir = Path.Combine(fileDir, FileType);
                        if (!Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                        using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                        {
                            await fileUpload.CopyToAsync(fs);
                        }
                        if (String.IsNullOrEmpty(file))
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                        }
                        return Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }




        [HttpPost("UserEdit")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> UserEdit([FromBody] APIUserMaster user)
        {

            try
            {

                if (ModelState.IsValid)
                {

                    var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");


                    var allowUserNameValidation = await _userRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode);
                    if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                    {
                        if (!UserNameRegExpression.IsMatch(user.UserName))
                        {
                            return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                        }
                    }
                    string[] configvalue = { "HRBP_Rename", "Mentor_Rename", "Companion_Rename" };
                    List<AppConfiguration> renameConfigValues = await _userRepository.GetUserConfigurationValueAsync(configvalue, OrgCode, "");
                    string companion = renameConfigValues.Where(a => a.Code == "Companion_Rename").Select(a => a.value).FirstOrDefault();
                    string mentor = renameConfigValues.Where(a => a.Code == "Mentor_Rename").Select(a => a.value).FirstOrDefault();
                    string hrbp = renameConfigValues.Where(a => a.Code == "HRBP_Rename").Select(a => a.value).FirstOrDefault();
                    if (user.BuddyTrainerId != null || user.MentorId != null || user.HRBPId != null)
                    {
                        if (user.BuddyTrainerId == user.MentorId && (user.BuddyTrainerId != null && user.MentorId != null) && (user.BuddyTrainerId != 0 && user.MentorId != 0))
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumDescription(MessageType.DataNotValid), Description = "The " + companion + " you're trying to map is already mapped as " + mentor + " to the selected user. Please select another user for your intended purpose." });

                        else if (user.BuddyTrainerId == user.HRBPId && (user.BuddyTrainerId != null && user.HRBPId != null) && (user.BuddyTrainerId != 0 && user.HRBPId != 0))
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumDescription(MessageType.DataNotValid), Description = "The " + companion + " you're trying to map is already mapped as " + hrbp + " to the selected user. Please select another user for your intended purpose." });

                        else if (user.HRBPId == user.MentorId && (user.HRBPId != null && user.MentorId != null) && (user.HRBPId != 0 && user.MentorId != 0))
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumDescription(MessageType.DataNotValid), Description = "The " + hrbp + " you're trying to map is already mapped as " + mentor + " to the selected user. Please select another user for your intended purpose." });
                    }

                    if (OrgCode.ToLower() != "tcl")
                    {
                        if (user.MobileNumber != "")
                        {
                            if (user.MobileNumber.Length < 10)
                            {
                                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });
                            }
                        }
                    }
                    user.UserId = user.UserId.ToLower(); // Save UserID in Lower Case
                    APIUserMaster oldUser = await this._userRepository.GetUser(user.Id);

                    if (oldUser == null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                    }
                    user.ProfilePicture = oldUser.ProfilePicture;
                    if (oldUser.Gender != user.Gender )
                    { 
                        if (!string.IsNullOrEmpty(user.ProfilePicture) )
                        {
                            if (Path.GetFileNameWithoutExtension(user.ProfilePicture).Length > 4)
                            {
                                user.ProfilePicture = oldUser.ProfilePicture;
                            }
                            else
                            {
                                user.ProfilePicture = _userRepository.RandomProfileImage(user.Gender);
                            }
                        }
                        else
                        {
                            user.ProfilePicture = oldUser.ProfilePicture;
                        }
                    }
                    else if (string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        user.ProfilePicture = _userRepository.RandomProfileImage(user.Gender);
                    }
                    user.FederationId = oldUser.FederationId;
                    int? HouseId = user.HouseId;

                    user = await this._userRepository.GetUserObject(user, UserRole, OrgCode);
                    user.HouseId = HouseId;

                    if (!user.IsAllConfigured)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });


                    if (await this._userRepository.IsUserRoleExist(user.UserRole))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    if (user.TermsCondintionsAccepted != oldUser.TermsCondintionsAccepted || user.AcceptanceDate != oldUser.AcceptanceDate)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    if (!(string.IsNullOrEmpty(user.Area)))
                    {
                        if (user.AreaId == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.Business)))
                    {
                        if (user.BusinessId == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.Group)))
                    {
                        if (user.GroupId == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.Location)))
                    {
                        if (user.LocationId == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn1)))
                    {
                        if (user.ConfigurationColumn1Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn2)))
                    {
                        if (user.ConfigurationColumn2Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn3)))
                    {
                        if (user.ConfigurationColumn3Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn4)))
                    {
                        if (user.ConfigurationColumn4Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn5)))
                    {
                        if (user.ConfigurationColumn5Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn6)))
                    {
                        if (user.ConfigurationColumn6Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn7)))
                    {
                        if (user.ConfigurationColumn7Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn8)))
                    {
                        if (user.ConfigurationColumn8Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn9)))
                    {
                        if (user.ConfigurationColumn9Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn10)))
                    {
                        if (user.ConfigurationColumn10Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn11)))
                    {
                        if (user.ConfigurationColumn11Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }

                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn12)))
                    {
                        if (user.ConfigurationColumn12Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn13)))
                    {
                        if (user.ConfigurationColumn13Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn14)))
                    {
                        if (user.ConfigurationColumn14Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.ConfigurationColumn15)))
                    {
                        if (user.ConfigurationColumn15Id == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                    }
                    if (!(string.IsNullOrEmpty(user.JobRoleName)))
                    {
                        if (user.JobRoleId == null)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.DataNotValid) });
                        }
                        if (user.JobRoleId != null)
                        {
                            if (user.DateIntoRole == null)
                            {
                                user.DateIntoRole = DateTime.UtcNow;
                            }

                        }
                    }

                   

                    if (user.EmailId != "")
                    {
                        if (user.EmailId == user.ReportsTo)
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.SelfReportToNotAllow), Description = EnumHelper.GetEnumDescription(MessageType.SelfReportToNotAllow) });
                    }

                    if (oldUser.House != user.House || oldUser.HouseId != user.HouseId || oldUser.AcceptanceDate != user.AcceptanceDate || oldUser.TermsCondintionsAccepted != user.TermsCondintionsAccepted || oldUser.CustomerCode.ToLower() != user.CustomerCode.ToLower())
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    if (user.EmailId == "")
                    {
                        user.EmailId = null;
                    }
                    if (user.MobileNumber == "")
                    {
                        user.MobileNumber = null;
                    }

                    if (OrgCode.ToLower() == "tcl")
                    {
                        if (await this._userRepository.IsUniqueFieldExistWithoutMobile(user.EmailId, user.MobileNumber, user.UserId, user.Id))
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                    else
                    {
                        if (await this._userRepository.IsUniqueFieldExist(user.EmailId, user.MobileNumber, user.UserId, user.Id))
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                    

                    if (oldUser.Lock == true && user.Lock == false)
                    {
                        await this._userRepository.ResetFailedLoginStatistics(Security.Encrypt(user.UserId));
                    }

                    user.IsPasswordModified = oldUser.IsPasswordModified;
                    user.PasswordModifiedDate = oldUser.PasswordModifiedDate;
                    user.RowGuid = oldUser.RowGuid;
                    user.HouseId = oldUser.HouseId;
                    user.CreatedBy = oldUser.CreatedBy;
                    user.CreatedDate = oldUser.CreatedDate;
                    user.LastModifiedDate = DateTime.UtcNow;
                    user.ModifiedBy = user.Id;
                    user.ModifiedDate = DateTime.UtcNow;
                    user.LastModifiedDate = DateTime.UtcNow;
                    user.Password = oldUser.Password;
                    user.BuddyTrainerId = user.BuddyTrainerId;
                    if (oldUser.IsActive == true && user.IsActive == false)
                    {
                        user.AccountDeactivationDate = DateTime.UtcNow;
                    }
                    else
                    {
                        user.AccountDeactivationDate = oldUser.AccountDeactivationDate;
                    }

                    if (OrgCode.ToLower().StartsWith("canh") && oldUser.UserRole == user.UserRole)
                    {
                        List<DesignationRoleMapping> designationRoleMappings = new List<DesignationRoleMapping>();
                        designationRoleMappings = await _userRepository.GetAllDesignationRoleList();
                        foreach (var designationrolemapping in designationRoleMappings)
                        {
                            if (designationrolemapping.Designation == user.ConfigurationColumn2)
                            {
                                user.UserRole = designationrolemapping.UserRole;
                            }

                        }
                    }

                    user.MobileNumber = Security.Encrypt(user.MobileNumber);
                    user.ReportsTo = Security.Encrypt(user.ReportsTo == null ? null : user.ReportsTo.ToLower());
                    user.UserId = Security.Encrypt(user.UserId);
                    user.EmailId = Security.Encrypt(user.EmailId);

                    string oldemailid = await this._userRepository.GetEmailUserExists(user.UserId);

                    UserMasterLogs _userMasterLogs = new UserMasterLogs();
                    _userMasterLogs.ModifiedBy = UserId;
                    _userMasterLogs.UserId = user.Id;
                    _userMasterLogs.IsUpdated = 1;

                    await this._userRepository.UpdateUser(user);
                    await this._userRepository.UpdateReportTo(oldemailid, user.EmailId);
                    await this._userRepository.AddUserHistory(oldUser, user);
                    await this._userRepository.AddUserMasterLogs(_userMasterLogs);
                    //Image File Delete
                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        StringBuilder sb = new StringBuilder();
                        string sWebRootFolder = this._configuration["ApiGatewayLXPFiles"];
                        sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
                        string[] pathRemove = user.ProfilePicture.Split("/");
                        string filename;
                        string remainpath;
                        int count = pathRemove.Count();
                        if (pathRemove.Count() >= 3)
                        {
                            filename = pathRemove[2];
                            remainpath = @"\" + pathRemove[0] + @"\" + pathRemove[1] + @"\";
                        }
                        else
                        {
                            filename = pathRemove[1];
                            remainpath = @"\" + pathRemove[0] + @"\";
                        }
                        sb = sb.Append(sWebRootFolder);
                        sb = sb.Append(remainpath);
                        string finalpath = sb.ToString();
                        FileInfo file = new FileInfo(Path.Combine(finalpath, filename));
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                    return this.Ok("Success");
                }
                return this.BadRequest(this.ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }



        [HttpPost("History")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> History([FromBody] ApiGetUserDetailsById apiGetUserDetailsById)
        {

            try
            {
                apiGetUserDetailsById.ID = Security.DecryptForUI(apiGetUserDetailsById.ID);
                //UserId is of type Int
                return Ok(await this._userHistoryRepository.GetuserHistory(Int32.Parse(apiGetUserDetailsById.ID)));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost("Search")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Search([FromBody] ApiUserId apiUserId)
        {
            try
            {
                apiUserId.UserId = Security.DecryptForUI(apiUserId.UserId);
                var users = await this._userRepository.SearchUserType(apiUserId.UserId);
                return this.Ok(users);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetHrms")]
        [PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetHrms([FromBody] APIUserSearch apiUserSearch)
        {
            try
            {
                var hrms = await this._hrmsRepository.GetAllHrms(apiUserSearch.Page, apiUserSearch.PageSize, apiUserSearch.Search);
                return this.Ok(hrms);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetHrmsById")]
        [PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetHrms([FromBody] ApiGetUserDetailsById apiGetUserDetailsById)
        {
            try
            {
                apiGetUserDetailsById.ID = Security.DecryptForUI(apiGetUserDetailsById.ID);
                var hrms = await this._hrmsRepository.Get(h => h.Id == Convert.ToInt32(apiGetUserDetailsById.ID) && h.IsDeleted == Record.NotDeleted);
                return this.Ok(hrms);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Hrms/GetTotalRecords")]

        [PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetHrmsCount([FromBody] APISearch apiSearch)
        {
            try
            {
                var count = await this._hrmsRepository.Count(apiSearch.Search);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Hrms")]
        [PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        public async Task<IActionResult> HrmsPost([FromBody] HRMS hrms)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (await this._hrmsRepository.IsColumnExist(hrms.ColumnName))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                    await this._hrmsRepository.Add(hrms);
                    return this.Ok(hrms);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }



        [HttpPost("HrmsPut")]
        [PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        public async Task<IActionResult> HrmsPut([FromBody] HRMS hrmsData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    HRMS hrms = await this._hrmsRepository.Get(h => h.Id == hrmsData.Id && h.IsDeleted == Record.NotDeleted);
                    if (hrms == null)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                    await this._hrmsRepository.Update(hrmsData);
                    return this.Ok(hrms);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string Exception = ex.Message;
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpDelete("Hrms")]
        [PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        public async Task<IActionResult> HrmsDelete([FromQuery]string id)
        {
            int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
            HRMS hrms = await this._hrmsRepository.Get(u => u.IsDeleted == Record.NotDeleted && u.Id == DecryptedId);
            if (hrms == null)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
            hrms.IsDeleted = Record.Deleted;
            await this._hrmsRepository.Update(hrms);
            return this.Ok(hrms);
        }

        //[HttpDelete("Hrms")]
        //[PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        //public async Task<IActionResult> HrmsDelete([FromBody] APIIntId apiIntId)
        //{
        //    try
        //    {
        //        HRMS hrms = await this._hrmsRepository.Get(u => u.IsDeleted == Record.NotDeleted && u.Id == apiIntId.Id);
        //        if (hrms == null)
        //        {
        //            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
        //        }
        //        hrms.IsDeleted = Record.Deleted;
        //        await this._hrmsRepository.Update(hrms);
        //        return this.Ok(hrms);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
        //    }
        //}

        [HttpGet("GetColumns")]
        [PermissionRequired(Permissions.UserHRAssosiation + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetColumns()
        {
            try
            {
                var columns = await this._hrmsRepository.GetColumnNames();
                return this.Ok(columns);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]

        public async Task<IActionResult> ForgotPassword([FromBody] ApiForgotPassword forgotpassword)
        {

            forgotpassword.userId = Security.DecryptForUI(forgotpassword.userId);
            forgotpassword.orgCode = Security.DecryptForUI(forgotpassword.orgCode);


            ResponseMessageV2 Response = new ResponseMessageV2();
            try
            {

                if (ModelState.IsValid)
                {

                    IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
                    var ipAddress = heserver.AddressList.ToList().Where(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().ToString();


                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(forgotpassword.orgCode);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                        });

                    }



                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    this._userMasterOtp.ChangeDbContext(OrgnizationConnectionString);
                    this._userMasterOtpHistory.ChangeDbContext(OrgnizationConnectionString);

                    this.url = this._configuration[Configuration.NotificationApi];
                    this.SMSURL = this._configuration[Configuration.NotificationApi];
                    var users = await this._userRepository.ForgotPassword(forgotpassword.userId);

                    if (users == null)
                    {
                        return this.BadRequest(new ResponseMessage
                        {
                            Message = EnumHelper.GetEnumName(MessageType.Fail),
                            Description = EnumHelper.GetEnumDescription(MessageType.NotExist)
                        });
                    }



                    string Otp = null;
                    int LoginCount = 0;

                    UserMasterOtpHistory UserOtpHistoryNew = await _userMasterOtpHistory.Get(o => o.UserMasterId == users.ID);
                    if (UserOtpHistoryNew != null)
                    {
                        DateTime ExpDate = UserOtpHistoryNew.CreatedDate.AddMinutes(30);
                        DateTime CurrentTime = DateTime.UtcNow;
                        int IsTimeExpired = DateTime.Compare(CurrentTime, ExpDate);
                        if (IsTimeExpired == -1)
                        {
                            LoginCount = UserOtpHistoryNew.Count;
                        }
                        else
                        {
                            UserOtpHistoryNew.CreatedDate = DateTime.UtcNow;
                            UserOtpHistoryNew.ModifiedDate = null;
                            UserOtpHistoryNew.Count = 0;
                            UserOtpHistoryNew.IPAddress = null;

                            await _userMasterOtpHistory.Remove(UserOtpHistoryNew);
                        }
                    }

                    string customerCode = users.CustomerCode;
                    string cslEmpCode = await this._userRepository.GetCslEmpCode(users.ID);
                    Otp = RandomPassword.GenerateRandomPassword();

                    if (LoginCount <= 3)
                    {

                        if (String.Equals(forgotpassword.orgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Task<bool> value = this._userRepository.checkuserexistanceinldap(forgotpassword.userId);
                            users.Result = value.Result;
                            if (value.Result == false)
                            {
                                Response.StatusCode = 400;
                                Response.Message = Record.userNotFound;
                                return BadRequest(Response);
                            }
                            else
                            {

                                bool UpdateRecord = true;
                                UserMasterOtp UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == users.ID);
                                if (UserOtp == null)
                                {
                                    UpdateRecord = false;
                                    UserOtp = new UserMasterOtp();
                                }
                                UserOtp.CreatedDate = DateTime.UtcNow;
                                UserOtp.Otp = Security.Encrypt(Otp);
                                UserOtp.UserMasterId = users.ID;
                                int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                                UserOtp.OtpExpiryDate = UserOtp.CreatedDate.AddMinutes(OtpValidityTime);
                                if (UpdateRecord)
                                    await _userMasterOtp.Update(UserOtp);
                                else
                                    await _userMasterOtp.Add(UserOtp);


                                UserMasterOtpHistory UserOtpHistory = await _userMasterOtpHistory.Get(o => o.UserMasterId == users.ID);
                                if (UserOtpHistory == null)
                                {

                                    UserOtpHistory = new UserMasterOtpHistory
                                    {
                                        UserMasterId = users.ID,
                                        Count = 1,
                                        IPAddress = ipAddress,
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    await _userMasterOtpHistory.Add(UserOtpHistory);
                                }
                                else
                                {
                                    UserOtpHistory.IPAddress = ipAddress;
                                    UserOtpHistory.ModifiedDate = DateTime.UtcNow;
                                    UserOtpHistory.Count = UserOtpHistory.Count + 1;
                                    await _userMasterOtpHistory.Update(UserOtpHistory);

                                }

                                string maskMobileNumber = String.Empty;
                                if (String.Equals(forgotpassword.orgCode, "compass", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    maskMobileNumber = MaskMobileCompass(users.MobileNumber);
                                }
                                else
                                {
                                    maskMobileNumber = MaskMobile(users.MobileNumber);
                                }

                                Response.Result = value.Result.ToString();
                                Response.StatusCode = 200;
                               // string maskMobileNumber = MaskMobile(users.MobileNumber);
                                string maskEmail = MaskEmail(users.ToEmail);
                                Response.MaskEmail = maskEmail;
                                Response.MaskMobile = maskMobileNumber;
                                return Ok(Response);
                            }
                        }

                        var SendSMSToUser = await _userRepository.GetMasterConfigurableParameterValueByConnectionString("SMS_FORGOTE_PASSWORD", OrgnizationConnectionString);
                        if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                        {
                            this.SMSURL += "/SMSForgotPasswordOTP";
                            if (users != null)
                            {
                                JObject oJsonObject = new JObject
                                {
                                    { "MobileNumber", users.MobileNumber },
                                    { "OrganizationCode", customerCode },
                                    { "UserId", users.ID },
                                    { "OTP", Otp },
                                    { "EmployeeCode",forgotpassword.userId},
                                    { "CslEmpCode", cslEmpCode },
                                    { "UserName", users.UserName }
                                };

                                bool UpdateRecord = true;
                                UserMasterOtp UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == users.ID);
                                if (UserOtp == null)
                                {
                                    UpdateRecord = false;
                                    UserOtp = new UserMasterOtp();
                                }
                                UserOtp.CreatedDate = DateTime.UtcNow;
                                UserOtp.Otp = Security.Encrypt(Otp);
                                UserOtp.UserMasterId = users.ID;
                                int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                                UserOtp.OtpExpiryDate = UserOtp.CreatedDate.AddMinutes(OtpValidityTime);

                                try
                                {
                                    Task.Run(() => CallAPI(SMSURL, oJsonObject).Result);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }


                        }


                        this.url += "/UserAddedOtp";
                        if (users != null)
                        {
                            JObject oJsonObject = new JObject
                            {
                                { "toEmail", users.ToEmail },
                                { "organizationCode", customerCode },
                                { "userName", users.UserName },
                                { "UserID", users.UserId },
                                { "Otp", Otp }
                            };

                            bool UpdateRecord = true;
                            UserMasterOtp UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == users.ID);
                            if (UserOtp == null)
                            {
                                UpdateRecord = false;
                                UserOtp = new UserMasterOtp();
                            }
                            UserOtp.CreatedDate = DateTime.UtcNow;
                            UserOtp.Otp = Security.Encrypt(Otp);
                            UserOtp.UserMasterId = users.ID;
                            int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                            UserOtp.OtpExpiryDate = UserOtp.CreatedDate.AddMinutes(OtpValidityTime);
                            if (UpdateRecord)
                                await _userMasterOtp.Update(UserOtp);
                            else
                                await _userMasterOtp.Add(UserOtp);


                            UserMasterOtpHistory UserOtpHistory = await _userMasterOtpHistory.Get(o => o.UserMasterId == users.ID);
                            if (UserOtpHistory == null)
                            {

                                UserOtpHistory = new UserMasterOtpHistory
                                {
                                    UserMasterId = users.ID,
                                    Count = 1,
                                    IPAddress = ipAddress,
                                    CreatedDate = DateTime.UtcNow
                                };
                                await _userMasterOtpHistory.Add(UserOtpHistory);
                            }
                            else
                            {
                                UserOtpHistory.IPAddress = ipAddress;
                                UserOtpHistory.ModifiedDate = DateTime.UtcNow;
                                UserOtpHistory.Count = UserOtpHistory.Count + 1;
                                await _userMasterOtpHistory.Update(UserOtpHistory);
                            }

                            string maskMobileNumber = String.Empty;
                            if (String.Equals(forgotpassword.orgCode, "compass", StringComparison.CurrentCultureIgnoreCase))
                            {
                                maskMobileNumber = MaskMobileCompass(users.MobileNumber);
                            }
                            else
                            {
                                maskMobileNumber = MaskMobile(users.MobileNumber);
                            }
                            Response.StatusCode = 200;
                            Response.Message = Record.OtpSent;
                            //string maskMobileNumber = MaskMobile(users.MobileNumber);
                            string maskEmail = MaskEmail(users.ToEmail);
                            Response.MaskEmail = maskEmail;
                            Response.MaskMobile = maskMobileNumber;
                            try
                            {
                                Task.Run(() => CallAPI(url, oJsonObject).Result);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                        }
                        return this.Ok(Response);

                    }
                    else
                    {
                        Response.StatusCode = 400;
                        Response.Message = Record.OtpLimit;
                        return BadRequest(Response);
                    }

                }
                else
                    return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }


        }


        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            //take first 2 characters
            string firstPart = email.Substring(0, 2);

            //take last character after @ characters
            int len = email.LastIndexOf("@");
            string lastPart = email.Substring(len, email.Length - len);

            //take the middle part (****)
            int middlePartLenght = email.Length - (firstPart.Length + lastPart.Length);
            string middlePart = new String('*', middlePartLenght);

            return firstPart + middlePart + lastPart;
        }

        private string MaskMobileCompass(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;


            //take last 4 characterss
            int len = phone.Length;
            string lastPart = phone.Substring(len - 4, 4);

            //take the middle part (****)
            int middlePartLenght = len - (lastPart.Length);
            string middlePart = new String('*', middlePartLenght);

            return middlePart + lastPart;
        }

        private string MaskMobile(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            //take first 2 characters
            string firstPart = phone.Substring(0, 2);

            //take last 4 characters
            int len = phone.Length;
            string lastPart = phone.Substring(len - 4, 4);

            //take the middle part (****)
            int middlePartLenght = len - (firstPart.Length + lastPart.Length);
            string middlePart = new String('*', middlePartLenght);

            return firstPart + middlePart + lastPart;
        }

        [AllowAnonymous]
        [HttpPost("ForgotPasswordIOS")]

        public async Task<IActionResult> ForgotPasswordIOS([FromBody] ApiForgotPassword forgotpassword)
        {

            ResponseMessageV2 Response = new ResponseMessageV2();
            try
            {

                string Key = "a1b2c3d4e5f6g7h8";
                string _initVector = "1234123412341234";

                forgotpassword.userId = CryptLib.decrypt(forgotpassword.userId, Key, _initVector);
                forgotpassword.orgCode = CryptLib.decrypt(forgotpassword.orgCode, Key, _initVector);
                forgotpassword.userId = forgotpassword.userId.ToLower();



                //// --- Android & Angular --- //

                ////string decryptedData = DecryptData(encryptedData);                

                //// --- Android & Angular --- //

                //// --- IOS --- //

                //string decryptedData = CryptLib.decrypt(encryptedData, Key, _initVector);

                //// --- IOS --- //



                if (ModelState.IsValid)
                {

                    IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
                    var ipAddress = heserver.AddressList.ToList().Where(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().ToString();


                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(forgotpassword.orgCode);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                        });

                    }



                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    this._userMasterOtp.ChangeDbContext(OrgnizationConnectionString);
                    this._userMasterOtpHistory.ChangeDbContext(OrgnizationConnectionString);

                    this.url = this._configuration[Configuration.NotificationApi];
                    this.SMSURL = this._configuration[Configuration.NotificationApi];
                    var users = await this._userRepository.ForgotPassword(forgotpassword.userId);
                    if (users == null)
                    {
                        return this.BadRequest(new ResponseMessage
                        {
                            Message = EnumHelper.GetEnumName(MessageType.Fail),
                            Description = EnumHelper.GetEnumDescription(MessageType.NotExist)
                        });
                    }



                    string Otp = null;
                    int LoginCount = 0;

                    UserMasterOtpHistory UserOtpHistoryNew = await _userMasterOtpHistory.Get(o => o.UserMasterId == users.ID);
                    if (UserOtpHistoryNew != null)
                    {
                        DateTime ExpDate = UserOtpHistoryNew.CreatedDate.AddMinutes(30);
                        DateTime CurrentTime = DateTime.UtcNow;
                        int IsTimeExpired = DateTime.Compare(CurrentTime, ExpDate);
                        if (IsTimeExpired == -1)
                        {
                            LoginCount = UserOtpHistoryNew.Count;
                        }
                        else
                        {
                            UserOtpHistoryNew.CreatedDate = DateTime.UtcNow;
                            UserOtpHistoryNew.ModifiedDate = null;
                            UserOtpHistoryNew.Count = 0;
                            UserOtpHistoryNew.IPAddress = null;

                            await _userMasterOtpHistory.Remove(UserOtpHistoryNew);
                        }
                    }

                    string customerCode = users.CustomerCode;
                    Otp = RandomPassword.GenerateRandomPassword();

                    if (LoginCount < 5)
                    {

                        if (String.Equals(forgotpassword.orgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Task<bool> value = this._userRepository.checkuserexistanceinldap(forgotpassword.userId);
                            users.Result = value.Result;
                            if (value.Result == false)
                            {

                                Response.StatusCode = 400;
                                Response.Message = Record.userNotFound;
                                return BadRequest(Response);
                            }
                            else
                            {

                                bool UpdateRecord = true;
                                UserMasterOtp UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == users.ID);
                                if (UserOtp == null)
                                {
                                    UpdateRecord = false;
                                    UserOtp = new UserMasterOtp();
                                }
                                UserOtp.CreatedDate = DateTime.UtcNow;
                                UserOtp.Otp = Security.Encrypt(Otp);
                                UserOtp.UserMasterId = users.ID;
                                int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                                UserOtp.OtpExpiryDate = UserOtp.CreatedDate.AddMinutes(OtpValidityTime);
                                if (UpdateRecord)
                                    await _userMasterOtp.Update(UserOtp);
                                else
                                    await _userMasterOtp.Add(UserOtp);


                                UserMasterOtpHistory UserOtpHistory = await _userMasterOtpHistory.Get(o => o.UserMasterId == users.ID);
                                if (UserOtpHistory == null)
                                {

                                    UserOtpHistory = new UserMasterOtpHistory
                                    {
                                        UserMasterId = users.ID,
                                        Count = 1,
                                        IPAddress = ipAddress,
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    await _userMasterOtpHistory.Add(UserOtpHistory);
                                }
                                else
                                {
                                    UserOtpHistory.IPAddress = ipAddress;
                                    UserOtpHistory.ModifiedDate = DateTime.UtcNow;
                                    UserOtpHistory.Count = UserOtpHistory.Count + 1;
                                    await _userMasterOtpHistory.Update(UserOtpHistory);

                                }


                                string maskMobileNumber = String.Empty;
                                if (String.Equals(forgotpassword.orgCode, "compass", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    maskMobileNumber = MaskMobileCompass(users.MobileNumber);
                                }
                                else
                                {
                                    maskMobileNumber = MaskMobile(users.MobileNumber);
                                }
                                Response.Result = value.Result.ToString();
                                Response.StatusCode = 200;
                                //string maskMobileNumber = MaskMobile(users.MobileNumber);
                                string maskEmail = MaskEmail(users.ToEmail);
                                Response.MaskEmail = maskEmail;
                                Response.MaskMobile = maskMobileNumber;
                                return Ok(Response);
                            }
                        }

                        var SendSMSToUser = await this._userRepository.GetMasterConfigurableParameterValueByConnectionString("SMS_FORGOTE_PASSWORD", OrgnizationConnectionString);
                        if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                        {
                            this.SMSURL += "/SMSForgotPasswordOTP";
                            if (users != null)
                            {
                                JObject oJsonObject = new JObject
                                {
                                    { "MobileNumber", users.MobileNumber },
                                    { "OrganizationCode", customerCode },
                                    { "UserId", users.ID },
                                    { "OTP", Otp },
                                    { "UserName", users.UserName }
                                };

                                bool UpdateRecord = true;
                                UserMasterOtp UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == users.ID);
                                if (UserOtp == null)
                                {
                                    UpdateRecord = false;
                                    UserOtp = new UserMasterOtp();
                                }
                                UserOtp.CreatedDate = DateTime.UtcNow;
                                UserOtp.Otp = Security.Encrypt(Otp);
                                UserOtp.UserMasterId = users.ID;
                                int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                                UserOtp.OtpExpiryDate = UserOtp.CreatedDate.AddMinutes(OtpValidityTime);


                                UserMasterOtpHistory UserOtpHistory = await _userMasterOtpHistory.Get(o => o.UserMasterId == users.ID);

                                try
                                {
                                    Task.Run(() => CallAPI(SMSURL, oJsonObject).Result);
                                }

                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }


                        this.url += "/UserAddedOtp";
                        if (users != null)
                        {
                            JObject oJsonObject = new JObject
                            {
                                { "toEmail", users.ToEmail },
                                { "organizationCode", customerCode },
                                { "userName", users.UserName },
                                { "UserID", users.UserId },
                                { "Otp", Otp }
                            };

                            bool UpdateRecord = true;
                            UserMasterOtp UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == users.ID);
                            if (UserOtp == null)
                            {
                                UpdateRecord = false;
                                UserOtp = new UserMasterOtp();
                            }
                            UserOtp.CreatedDate = DateTime.UtcNow;
                            UserOtp.Otp = Security.Encrypt(Otp);
                            UserOtp.UserMasterId = users.ID;
                            int OtpValidityTime = Int32.Parse(this._configuration[Configuration.OtpValidityTime]);
                            UserOtp.OtpExpiryDate = UserOtp.CreatedDate.AddMinutes(OtpValidityTime);
                            if (UpdateRecord)
                                await _userMasterOtp.Update(UserOtp);
                            else
                                await _userMasterOtp.Add(UserOtp);


                            UserMasterOtpHistory UserOtpHistory = await _userMasterOtpHistory.Get(o => o.UserMasterId == users.ID);
                            if (UserOtpHistory == null)
                            {

                                UserOtpHistory = new UserMasterOtpHistory
                                {
                                    UserMasterId = users.ID,
                                    Count = 1,
                                    IPAddress = ipAddress,
                                    CreatedDate = DateTime.UtcNow
                                };
                                await _userMasterOtpHistory.Add(UserOtpHistory);
                            }
                            else
                            {
                                UserOtpHistory.IPAddress = ipAddress;
                                UserOtpHistory.ModifiedDate = DateTime.UtcNow;
                                UserOtpHistory.Count = UserOtpHistory.Count + 1;
                                await _userMasterOtpHistory.Update(UserOtpHistory);
                            }

                            string maskMobileNumber = String.Empty;
                            if (String.Equals(forgotpassword.orgCode, "compass", StringComparison.CurrentCultureIgnoreCase))
                            {
                                maskMobileNumber = MaskMobileCompass(users.MobileNumber);
                            }
                            else
                            {
                                maskMobileNumber = MaskMobile(users.MobileNumber);
                            }
                            Response.StatusCode = 200;
                            Response.Message = Record.OtpSent;
                            //string maskMobileNumber = MaskMobile(users.MobileNumber);
                            string maskEmail = MaskEmail(users.ToEmail);
                            Response.MaskEmail = maskEmail;
                            Response.MaskMobile = maskMobileNumber;
                            try
                            {
                                Task.Run(() => CallAPI(url, oJsonObject).Result);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                        }
                        return this.Ok(Response);

                    }
                    else
                    {
                        Response.StatusCode = 400;
                        Response.Message = Record.OtpLimit;
                        return BadRequest(Response);
                    }

                }
                else
                    return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }


        }

        private static string DecryptData(string textToDecrypt)
        {
            string Key = "a1b2c3d4e5f6g7h8";
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            RijndaelManaged rijndaelCipher = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,

                KeySize = 0x80,
                BlockSize = 0x80
            };

            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(Key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return encoding.GetString(plainText);
        }

        private JObject EncryptFinalOutput(JObject jsonData11)
        {
            string Key = "a1b2c3d4e5f6g7h8";
            string dataNew = EncryptNEWMethod(jsonData11.ToString(), Key);
            ResultClass resultEncrypted = new ResultClass
            {
                Result = dataNew
            };

            return JObject.Parse(JsonConvert.SerializeObject(resultEncrypted));
        }

        private static string EncryptNEWMethod(string data, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged
            {
                Mode = CipherMode.CBC, //remember this parameter
                Padding = PaddingMode.PKCS7, //remember this parameter

                KeySize = 0x80,
                BlockSize = 0x80
            };
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;

            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }

            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(data);

            return Convert.ToBase64String
            (transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }

        [AllowAnonymous]
        [HttpPost("ForgotPasswordOtp")]
        public async Task<IActionResult> ForgotPasswordOtp([FromBody] APIForgotPassword apiForgotPassword)
        {
            try
            {
                apiForgotPassword.OrganizationCode = Security.DecryptForUI(apiForgotPassword.OrganizationCode);
                apiForgotPassword.Otp = Security.DecryptForUI(apiForgotPassword.Otp);
                apiForgotPassword.UserId = Security.DecryptForUI(apiForgotPassword.UserId);

                if (ModelState.IsValid)
                {
                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiForgotPassword.OrganizationCode);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                        });
                    }
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    this._userMasterOtp.ChangeDbContext(OrgnizationConnectionString);
                    APIUserMaster user = await this._userRepository.GetByEmailOrUserId(apiForgotPassword.UserId);

                    if (user == null)
                        return Ok(false);
                    UserMasterOtp UserOtp = await _userRepository.GetOTP(user.Id);
                    if (UserOtp == null)
                        return Ok(false);
                    DateTime CurrentTime = DateTime.UtcNow;
                    int IsTimeExpired = TimeSpan.Compare(UserOtp.OtpExpiryDate.TimeOfDay, CurrentTime.TimeOfDay);
                    if (IsTimeExpired == -1)
                        if (apiForgotPassword.OrganizationCode.Contains("hdfc"))
                            return this.Ok(new ResponseMessage { Message = Record.OtpExpired, Description = Record.OtpExpired, StatusCode = 419 });
                        else
                            return Ok(false);
                    if (Security.Decrypt(UserOtp.Otp) != apiForgotPassword.Otp)
                    {
                        return Ok(false);
                    }
                    // restrict opt to user only once //
                    UserMasterOtp UserOtpUpdate = await _userRepository.GetOTP(user.Id);
                    UserOtpUpdate.OtpExpiryDate = UserOtp.CreatedDate;
                    UserOtpUpdate.Otp = UserOtp.Otp;
                    await _userMasterOtp.Update(UserOtpUpdate);
                    // restrict opt to user only once //


                    return Ok(true);

                }
                else
                    return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [AllowAnonymous]
        [HttpPost("ForgotPasswordIOSOtp")]
        public async Task<IActionResult> ForgotPasswordIOSOtp([FromBody] APIForgotPassword apiForgotPassword)
        {
            try
            {
                string Key = "a1b2c3d4e5f6g7h8";
                string _initVector = "1234123412341234";

                apiForgotPassword.OrganizationCode = CryptLib.decrypt(apiForgotPassword.OrganizationCode, Key, _initVector);
                apiForgotPassword.Otp = CryptLib.decrypt(apiForgotPassword.Otp, Key, _initVector);
                apiForgotPassword.UserId = CryptLib.decrypt(apiForgotPassword.UserId, Key, _initVector).ToLower();


                if (ModelState.IsValid)
                {
                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiForgotPassword.OrganizationCode);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                        });
                    }
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    this._userMasterOtp.ChangeDbContext(OrgnizationConnectionString);
                    APIUserMaster user = await this._userRepository.GetByEmailOrUserId(apiForgotPassword.UserId);

                    if (user == null)
                        return Ok(false);
                    UserMasterOtp UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == user.Id);

                    if (UserOtp == null)
                        return Ok(false);
                    DateTime CurrentTime = DateTime.UtcNow;
                    int IsTimeExpired = TimeSpan.Compare(UserOtp.OtpExpiryDate.TimeOfDay, CurrentTime.TimeOfDay);
                    if (IsTimeExpired == -1)
                        if (apiForgotPassword.OrganizationCode.Contains("hdfc"))
                            return this.BadRequest(new ResponseMessage { Message = Record.OtpExpired, Description = Record.OtpExpired, StatusCode = 419 });
                        else
                            return this.BadRequest(new ResponseMessage { Message = Record.OtpExpired, Description = Record.OtpExpired });

                    if (!String.Equals(Security.Decrypt(UserOtp.Otp), apiForgotPassword.Otp, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Ok(false);
                    }
                    // restrict opt to user only once //
                    UserMasterOtp UserOtpUpdate = await _userMasterOtp.Get(o => o.UserMasterId == user.Id);
                    UserOtpUpdate.OtpExpiryDate = UserOtp.CreatedDate;
                    await _userMasterOtp.Update(UserOtpUpdate);

                    return Ok(true);

                }
                else
                    return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [AllowAnonymous]
        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword([FromBody] APINewPassword newPassword)
        {
            try
            {

                newPassword.UserId = Security.DecryptForUI(newPassword.UserId);
                newPassword.NewPassword = Security.DecryptForUI(newPassword.NewPassword);
                newPassword.confirmPassword = Security.DecryptForUI(newPassword.confirmPassword);
                newPassword.OrganizationCode = Security.DecryptForUI(newPassword.OrganizationCode);
                newPassword.OTP = Security.DecryptForUI(newPassword.OTP);
                newPassword.UserId = newPassword.UserId.ToLower();

                if (ModelState.IsValid)
                {
                    if (Security.ValidatePassword(newPassword.NewPassword, newPassword.OrganizationCode))
                    {

                        string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(newPassword.OrganizationCode);
                        if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                        {
                            return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                        }

                        if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                        {
                            return BadRequest(new OpenIdConnectResponse
                            {
                                Error = OpenIdConnectConstants.Errors.InvalidGrant,
                                ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                            });
                        }

                        this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                        //get user details from database, as LINQ was throwing timeout expired error.
                        APIUserMaster user = await this._userRepository.GetByUserIdNew(Security.Encrypt(newPassword.UserId));
                        if (user == null)
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                        UserMasterOtp UserOtp = new UserMasterOtp();
                        if (!String.Equals(newPassword.OrganizationCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this._userMasterOtp.ChangeDbContext(OrgnizationConnectionString);
                            UserOtp = await _userMasterOtp.Get(o => o.UserMasterId == user.Id);
                            if (UserOtp == null)
                                return Ok(false);

                            //restrict otp to use only once //
                            if (UserOtp.Otp == null)
                                return this.BadRequest(new ResponseMessage { Message = Record.OtpExpired, Description = Record.OtpExpired });
                            //restrict otp to use only once //

                            DateTime CurrentTime = DateTime.UtcNow;
                            int IsTimeExpired = TimeSpan.Compare(UserOtp.OtpExpiryDate.TimeOfDay, CurrentTime.TimeOfDay);


                            if (!String.Equals(Security.Decrypt(UserOtp.Otp), newPassword.OTP, StringComparison.CurrentCultureIgnoreCase))
                                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });

                            this._passwordHistory.ChangeDbContext(OrgnizationConnectionString);


                            var checkPassword = Helper.Security.EncryptSHA512(newPassword.NewPassword);

                            bool IsOldPassword = await _passwordHistory.IsOldPassword(user.Id, newPassword.NewPassword);

                            if (checkPassword == user.Password)
                            {
                                IsOldPassword = true;
                            }

                            if (IsOldPassword)
                                return this.BadRequest(new ResponseMessage { Message = Record.OldPassword, Description = Record.OldPassword });
                        }

                        if (String.Equals(newPassword.OrganizationCode, "sbil", StringComparison.CurrentCultureIgnoreCase) && (newPassword.UserId.ToLower() != "sammir" && newPassword.UserId.ToLower() != "satish" && newPassword.UserId.ToLower() != "pradeep"))
                        {
                            DataTable dt = new DataTable();

                            DataColumn dc1 = new DataColumn("StatusCode");
                            dt.Columns.Add(dc1);
                            DataColumn dc2 = new DataColumn("Message");
                            dt.Columns.Add(dc2);
                            DataColumn dc3 = new DataColumn("UserType");
                            dt.Columns.Add(dc3);
                            DataRow dr = dt.NewRow();

                            string StatusCode;
                            string Message;

                            Task<string> value = this._userRepository.resetPasswordWeb(newPassword.UserId, newPassword.dob, newPassword.NewPassword, newPassword.confirmPassword);
                            string val = value.Result;

                            DataTable dtResult = this._userRepository.CheckResultString(val, dt, dr, null);

                            StatusCode = dt.Rows[0]["StatusCode"].ToString();
                            Message = dt.Rows[0]["Message"].ToString();
                            return Ok(new { StatusCode, Message });
                        }
                        else
                        {
                            PasswordHistory PasswordHistory = new PasswordHistory();

                            if (newPassword.NewPassword.Contains(user.UserName))
                                return StatusCode(StatusCodes.Status400BadRequest, Record.PasswordUserName);
                            List<string> query = await _userRepository.passwordchanged(user.Id, newPassword.OrganizationCode);
                            if (query.Contains(Security.EncryptSHA512(newPassword.NewPassword)))
                            {
                                if (string.Equals(newPassword.OrganizationCode, "bandhan", StringComparison.CurrentCultureIgnoreCase) || string.Equals(newPassword.OrganizationCode, "bandhanbank", StringComparison.CurrentCultureIgnoreCase) )
                                    return StatusCode(StatusCodes.Status400BadRequest, Record.PasswordFailedLastFive);
                                else
                                    return StatusCode(StatusCodes.Status400BadRequest, Record.PasswordFailed);
                            }
                            PasswordHistory.Password = user.Password;
                            PasswordHistory.UserMasterId = user.Id;
                            PasswordHistory.CreatedBy = user.Id;
                            PasswordHistory.CreatedDate = DateTime.UtcNow;
                            user.Password = Helper.Security.EncryptSHA512(newPassword.NewPassword);
                            UserOtp.Otp = null;
                            user.MobileNumber = Security.Encrypt(user.MobileNumber);
                            user.ReportsTo = Security.Encrypt(user.ReportsTo == null ? null : user.ReportsTo.ToLower());
                            user.UserId = Security.Encrypt(user.UserId);
                            user.EmailId = Security.Encrypt(user.EmailId);
                            user.PasswordModifiedDate = DateTime.UtcNow;
                            user.IsPasswordModified = true;
                            user.TermsCondintionsAccepted = true; //todo: added for IOCL, pls check
                            //hack added to get rid of encrypt method problem.
                            user.Lock = false;


                            string tempEmail = Security.Encrypt(user.EmailId);
                            await _passwordHistory.Add(PasswordHistory);
                            await _userRepository.UpdateUserPasswordHistory(user.UserId);
                            await _userRepository.UpdateUser(user);
                            await _userMasterOtp.Update(UserOtp);


                            return Ok(true);
                        }

                    }
                    else
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.PasswordNotMatch), Description = EnumHelper.GetEnumDescription(MessageType.PasswordNotMatch) });
                    }

                }
                else
                    return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [AllowAnonymous]
        [HttpPost("NewPasswordIOS")]
        public async Task<IActionResult> NewPasswordIOS([FromBody] APINewPassword newPassword)
        {
            try
            {

                string Key = "a1b2c3d4e5f6g7h8";
                string _initVector = "1234123412341234";


                newPassword.UserId = CryptLib.decrypt(newPassword.UserId, Key, _initVector);
                newPassword.NewPassword = CryptLib.decrypt(newPassword.NewPassword, Key, _initVector);
                newPassword.confirmPassword = CryptLib.decrypt(newPassword.confirmPassword, Key, _initVector);
                newPassword.OrganizationCode = CryptLib.decrypt(newPassword.OrganizationCode, Key, _initVector);
                newPassword.OTP = CryptLib.decrypt(newPassword.OTP, Key, _initVector);
                newPassword.UserId = newPassword.UserId.ToLower();


                if (ModelState.IsValid)
                {
                    if (Security.ValidatePassword(newPassword.NewPassword, newPassword.OrganizationCode))
                    {

                        string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(newPassword.OrganizationCode);
                        if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                        {
                            return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                        }

                        if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                        {
                            return BadRequest(new OpenIdConnectResponse
                            {
                                Error = OpenIdConnectConstants.Errors.InvalidGrant,
                                ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                            });
                        }

                        this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                        //get user details from database, as LINQ was throwing timeout expired error.
                        APIUserMaster user = await this._userRepository.GetByUserIdNew(Security.Encrypt(newPassword.UserId));
                        if (user == null)
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                        UserMasterOtp UserOtp = new UserMasterOtp();
                        if (!String.Equals(newPassword.OrganizationCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                        {
                            UserOtp = await _userRepository.GetUserMasterotp(user.Id);
                            if (UserOtp == null)
                                return Ok(false);

                            //restrict otp to use only once //
                            if (UserOtp.Otp == null)
                                return this.BadRequest(new ResponseMessage { Message = Record.OtpExpired, Description = Record.OtpExpired });

                            if (!String.Equals(Security.Decrypt(UserOtp.Otp), newPassword.OTP, StringComparison.CurrentCultureIgnoreCase))
                                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });


                            var checkPassword = Helper.Security.EncryptSHA512(newPassword.NewPassword);

                            bool IsOldPassword = await _userRepository.IsOldPassword(user.Id, newPassword.NewPassword);

                            if (checkPassword == user.Password)
                            {
                                IsOldPassword = true;
                            }

                            if (IsOldPassword)
                                return this.BadRequest(new ResponseMessage { Message = Record.OldPassword, Description = Record.OldPassword });
                        }

                        if (String.Equals(newPassword.OrganizationCode, "sbil", StringComparison.CurrentCultureIgnoreCase) && (newPassword.UserId.ToLower() != "sammir" && newPassword.UserId.ToLower() != "satish" && newPassword.UserId.ToLower() != "pradeep"))
                        {
                            DataTable dt = new DataTable();

                            DataColumn dc1 = new DataColumn("StatusCode");
                            dt.Columns.Add(dc1);
                            DataColumn dc2 = new DataColumn("Message");
                            dt.Columns.Add(dc2);
                            DataColumn dc3 = new DataColumn("UserType");
                            dt.Columns.Add(dc3);
                            DataRow dr = dt.NewRow();

                            string StatusCode;
                            string Message;

                            Task<string> value = this._userRepository.resetPasswordWeb(newPassword.UserId, newPassword.dob, newPassword.NewPassword, newPassword.confirmPassword);
                            string val = value.Result;

                            DataTable dtResult = this._userRepository.CheckResultString(val, dt, dr, null);

                            StatusCode = dt.Rows[0]["StatusCode"].ToString();
                            Message = dt.Rows[0]["Message"].ToString();
                            return Ok(new { StatusCode, Message });
                        }
                        else
                        {
                            PasswordHistory PasswordHistory = new PasswordHistory();


                            PasswordHistory.Password = user.Password;
                            PasswordHistory.UserMasterId = user.Id;
                            PasswordHistory.CreatedBy = user.Id;
                            PasswordHistory.CreatedDate = DateTime.UtcNow;
                            user.Password = Helper.Security.EncryptSHA512(newPassword.NewPassword);
                            UserOtp.Otp = null;
                            user.MobileNumber = Security.Encrypt(user.MobileNumber);
                            user.ReportsTo = Security.Encrypt(user.ReportsTo == null ? null : user.ReportsTo.ToLower());
                            user.UserId = Security.Encrypt(user.UserId);
                            user.EmailId = Security.Encrypt(user.EmailId);
                            user.PasswordModifiedDate = DateTime.UtcNow;
                            user.IsPasswordModified = true;
                            user.TermsCondintionsAccepted = true; //todo: added for IOCL, pls check
                            //hack added to get rid of encrypt method problem.
                            user.Lock = false;


                            string tempEmail = Security.Encrypt(user.EmailId);
                            await _userRepository.AddPasswordHistory(PasswordHistory);
                            await _userRepository.UpdateUserPasswordHistory(user.UserId);
                            await _userRepository.UpdateUser(user);
                            await _userRepository.AddUpdateUserMasterOTP(UserOtp);


                            return Ok(true);
                        }

                    }
                    else
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.PasswordNotMatch), Description = EnumHelper.GetEnumDescription(MessageType.PasswordNotMatch) });
                    }

                }
                else
                    return BadRequest(ModelState);



            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        private async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = this.url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }
        }




        [HttpPost("ChangeUserPassword")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] APIChangeUserPassword changePassword)
        {
            try
            {
                if (!IsiOS)
                {
                    changePassword.NewPassword = Security.DecryptForUI(changePassword.NewPassword);
                    changePassword.CurrentPassword = Security.DecryptForUI(changePassword.CurrentPassword);
                }
                else
                {
                    string Key = "a1b2c3d4e5f6g7h8";
                    string _initVector = "1234123412341234";
                    changePassword.NewPassword = CryptLib.decrypt(changePassword.NewPassword, Key, _initVector);
                    changePassword.CurrentPassword = CryptLib.decrypt(changePassword.CurrentPassword, Key, _initVector);
                }

                if (ModelState.IsValid || OrgCode.ToLower() == "pepperfry")
                {
                    if (Security.ValidatePassword(changePassword.NewPassword, OrgCode))
                    {


                        APIUserMaster user = await this._userRepository.GetUser(UserId);

                        if (user == null)
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });

                        if (OrgCode.ToLower() == "sbil" && IsLDAP && !IsExternalUser && (UserName.ToLower() != "sammir" && UserName.ToLower() != "satish" && UserName.ToLower() != "pradeep"))
                        {
                            DataTable dt = new DataTable();

                            DataColumn dc1 = new DataColumn("StatusCode");
                            dt.Columns.Add(dc1);
                            DataColumn dc2 = new DataColumn("Message");
                            dt.Columns.Add(dc2);
                            DataColumn dc3 = new DataColumn("UserType");
                            dt.Columns.Add(dc3);
                            DataRow dr = dt.NewRow();

                            string StatusCode;
                            string Message;

                            var resultString = await ChangePasswordForWeb(UserName, changePassword.CurrentPassword, changePassword.NewPassword);

                            DataTable dtResult = this._userRepository.CheckResultString(resultString, dt, dr, null);

                            StatusCode = dt.Rows[0]["StatusCode"].ToString();
                            Message = dt.Rows[0]["Message"].ToString();
                            return Ok(new { StatusCode, Message });



                        }
                        else if (OrgCode.ToLower() == "sbil" && IsExternalUser)
                        {
                            string StatusCode;
                            string Message;

                            if (!string.Equals(Security.EncryptSHA512(changePassword.CurrentPassword), user.Password))
                            {
                                StatusCode = Record.InvalidCredentials;
                                Message = Record.InvalidCredentials;
                                return BadRequest(new { StatusCode, Message });
                            }


                            if (changePassword.NewPassword.Contains(UserName))
                            {
                                StatusCode = Record.PasswordUserName;
                                Message = Record.PasswordUserName;
                                return BadRequest(new { StatusCode, Message });
                            }


                            List<string> query = await _userRepository.passwordchanged(user.Id, OrgCode.ToLower());

                            if (query.Contains(Security.EncryptSHA512(changePassword.NewPassword)))
                            {
                                if (string.Equals(OrgCode, "bandhan", StringComparison.CurrentCultureIgnoreCase) || string.Equals(OrgCode, "bandhanbank", StringComparison.CurrentCultureIgnoreCase) )
                                {
                                    StatusCode = Record.PasswordFailedLastFive;
                                    Message = Record.PasswordFailedLastFive;
                                }
                                else
                                {
                                    StatusCode = Record.PasswordFailed;
                                    Message = Record.PasswordFailed;
                                }

                                return BadRequest(new { StatusCode, Message });
                            }


                            user.Password = Helper.Security.EncryptSHA512(changePassword.NewPassword);
                            user.UserId = Security.Encrypt(user.UserId);
                            user.MobileNumber = Security.Encrypt(user.MobileNumber);
                            user.EmailId = Security.Encrypt(user.EmailId);
                            user.ModifiedDate = DateTime.UtcNow;
                            user.ReportsTo = Security.Encrypt(user.ReportsTo == null ? null : user.ReportsTo.ToLower());
                            user.IsPasswordModified = true;
                            user.PasswordModifiedDate = DateTime.UtcNow;
                            await _userRepository.UpdateUser(user);


                            StatusCode = "ExternalUser";
                            Message = "SUCCESS";
                            return Ok(new { StatusCode, Message });


                        }
                        else
                        {

                            if (!string.Equals(Security.EncryptSHA512(changePassword.CurrentPassword), user.Password))
                                return StatusCode(StatusCodes.Status400BadRequest, Record.PasswordNotMatch);

                            if (changePassword.NewPassword.Contains(UserName))
                                return StatusCode(StatusCodes.Status400BadRequest, Record.PasswordUserName);

                            List<string> query = await _userRepository.passwordchanged(user.Id, OrgCode.ToLower());

                            if (query.Contains(Security.EncryptSHA512(changePassword.NewPassword)))
                            {
                                if (string.Equals(OrgCode, "bandhan", StringComparison.CurrentCultureIgnoreCase) || string.Equals(OrgCode, "bandhanbank", StringComparison.CurrentCultureIgnoreCase) )
                                    return StatusCode(StatusCodes.Status400BadRequest, Record.PasswordFailedLastFive);
                                else
                                    return StatusCode(StatusCodes.Status400BadRequest, Record.PasswordFailed);
                            }


                            user.Password = Helper.Security.EncryptSHA512(changePassword.NewPassword);
                            user.UserId = Security.Encrypt(user.UserId);
                            user.MobileNumber = Security.Encrypt(user.MobileNumber);
                            user.EmailId = Security.Encrypt(user.EmailId);
                            user.ModifiedDate = DateTime.UtcNow;
                            user.ReportsTo = Security.Encrypt(user.ReportsTo == null ? null : user.ReportsTo.ToLower());
                            user.IsPasswordModified = true;
                            user.PasswordModifiedDate = DateTime.UtcNow;
                            await _userRepository.UpdateUser(user);

                            PasswordHistory passwordHistory = new PasswordHistory();
                            passwordHistory.Password = user.Password;
                            passwordHistory.UserMasterId = user.Id;
                            passwordHistory.CreatedBy = passwordHistory.ModifiedBy = user.Id;
                            passwordHistory.CreatedDate = passwordHistory.ModifiedDate = DateTime.UtcNow;

                            await _passwordHistory.Add(passwordHistory);

                            return Ok(true);
                        }
                    }
                    else
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.PasswordNotMatch), Description = EnumHelper.GetEnumDescription(MessageType.PasswordNotMatch) });

                }
                else
                    return BadRequest(ModelState);



            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }



        private async Task<string> ChangePasswordForWeb(string uid, string opass, string npass)
        {

            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
            //Create endpointAddress of the Service
            System.ServiceModel.EndpointAddress endpointAddress = new EndpointAddress("https://securelogin.sbilife.co.in/acas/LoginService?wsdl");

            LoginServiceClient objAgentAuth = new LoginServiceClient(binding, endpointAddress);
            changePasswordweb chprw = new changePasswordweb
            {
                username = uid,
                oldpassword = opass,
                password = npass
            };
            var result = await objAgentAuth.changePasswordwebAsync(chprw);
            return result.changePasswordwebResponse.@return.ToString();
        }

        [HttpPost("GetUserById")]
        [PermissionRequired(Permissions.HRResponsetorequest + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetUserByUserId([FromBody] ApiUserId apiUserId)
        {
            try
            {
                apiUserId.UserId = Security.DecryptForUI(apiUserId.UserId);
                APIUserMaster user = await this._userRepository.GetUser(Convert.ToInt32(apiUserId.UserId));
                string DomainName = this._configuration["ApiGatewayUrl"];
                user.ProfilePicturePath = string.Concat(DomainName, user.ProfilePicture);
                return this.Ok(user);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message /*EnumHelper.GetEnumDescription(MessageType.InternalServerError)*/ });
            }
        }

        [HttpPost("GetUserDetailsById")]
        public async Task<IActionResult> GetUserDetailsById([FromBody] ApiGetUserDetailsById apiGetUserById)
        {
            try
            {
                APIUserMaster loggedinUserInfo = await this._userRepository.GetUser(UserId);
                IList<APIUserMyTeam> team = await this._userRepository.GetMyTeam(Security.Encrypt(loggedinUserInfo.EmailId), OrgCode);

                List<APIUserMyTeam> isInMyTeam = (from u in team where u.UserId.Equals(apiGetUserById.ID) select u).ToList();

                //if (isInMyTeam.Count == 0)
                //{
                //    return Unauthorized();
                //}

                APIUserMasterDetails user = await this._userRepository.GetUserDetailsById(apiGetUserById.ID);
                return this.Ok(user);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = "Invalid" /*EnumHelper.GetEnumDescription(MessageType.InternalServerError)*/ });
            }
        }

        [HttpPost("GetJobRoleDetails")]
        public async Task<IActionResult> GetJobRoleDetails([FromBody] ApiGetJobRoleDetails apiGetJobRoleDetails)
        {
            try
            {

                APICompetencyJdUpload record = await this._userRepository.GetCompetencyJdView(apiGetJobRoleDetails.JobRoleId);
                //await this._userRepository.GetCompetencyJdView(apiGetJobRoleDetails.JobRoleId);
                return this.Ok(record);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = "Invalid" /*EnumHelper.GetEnumDescription(MessageType.InternalServerError)*/ });
            }
        }





        [HttpPost("GetUserDetailsByIdforDWTC")]
        public async Task<IActionResult> GetUserDetailsByIdforDWTC([FromBody] ApiGetUserDetailsById apiGetUserById)
        {
            try
            {

                if (OrgCode == "dwtc")
                {
                    apiGetUserById.ID = Security.DecryptForUI(apiGetUserById.ID);
                    APIUserMaster userInfo = await this._userRepository.GetUser(Convert.ToInt32(apiGetUserById.ID));

                    APIUserMasterDetails user = new APIUserMasterDetails
                    {
                        UserName = userInfo.UserName,
                        Area = userInfo.Area
                    };
                    user.Business = user.Business;
                    user.ConfigurationColumn1 = userInfo.ConfigurationColumn1;
                    user.ConfigurationColumn2 = userInfo.ConfigurationColumn2;
                    user.ConfigurationColumn3 = userInfo.ConfigurationColumn3;
                    user.ConfigurationColumn4 = userInfo.ConfigurationColumn4;
                    user.ConfigurationColumn5 = userInfo.ConfigurationColumn5;
                    user.ConfigurationColumn6 = userInfo.ConfigurationColumn6;
                    user.ConfigurationColumn7 = userInfo.ConfigurationColumn7;
                    user.ConfigurationColumn8 = userInfo.ConfigurationColumn8;
                    user.ConfigurationColumn9 = userInfo.ConfigurationColumn9;
                    user.ConfigurationColumn10 = userInfo.ConfigurationColumn10;
                    user.ConfigurationColumn11 = userInfo.ConfigurationColumn11;
                    user.ConfigurationColumn12 = userInfo.ConfigurationColumn12;

                    return this.Ok(user);
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = "Invalid" /*EnumHelper.GetEnumDescription(MessageType.InternalServerError)*/ });
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = "Invalid" /*EnumHelper.GetEnumDescription(MessageType.InternalServerError)*/ });
            }
        }

        #region GetUserProfile
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                APIUserMaster user = await this._userRepository.GetUser(UserId, await _userRepository.GetDecryptUserId(UserId));
                user.Password = null;
                var request = this._httpContextAccessor.HttpContext.Request;

                return this.Ok(user);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message /*EnumHelper.GetEnumDescription(MessageType.InternalServerError)*/ });
            }
        }

        [HttpGet("GetUserProfile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                APIUserProfile user = await this._userRepository.GetUserProfile(UserId, await _userRepository.GetDecryptUserId(UserId));

                var request = this._httpContextAccessor.HttpContext.Request;
                user.ProfilePicture = "";
                user.ProfilePicturePath = "";

                return this.Ok(user);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message /*EnumHelper.GetEnumDescription(MessageType.InternalServerError)*/ });
            }
        }

        #endregion GetUserProfile

        #region UpdateUserProfile




        [HttpPost("UpdateUserProfile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] APIUpdateUserProfile apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string DomainName = this._configuration["ApiGatewayUrl"];
                    apiUser.ProfilePicturePath = string.Concat(DomainName, apiUser.ProfilePicture);
                    apiUser.UserId = Security.Encrypt(UserName); // UserName is UserId
                    apiUser.Id = UserId;
                    string mobile = Security.DecryptForUI(apiUser.MobileNumber);
                    if (mobile.Length < 10)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberLessNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberLessNotExists) });

                    }
                    apiUser.MobileNumber = Security.DecryptForUI(apiUser.MobileNumber);
                    if (await this._userRepository.ExistsForUpdate("mobile", apiUser.MobileNumber, UserId))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                    }
                    else
                    {

                        apiUser.TimeZone = apiUser.TimeZone;
                        apiUser.Currency = apiUser.Currency;
                        apiUser.ProfilePicture = apiUser.ProfilePicture;
                        apiUser.ModifiedDate = DateTime.UtcNow;
                        await this._userRepository.UpdateUserProfile(apiUser, OrgCode);
                        return this.Ok(apiUser);
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("UpdateUserMobileEmail")]
        public async Task<IActionResult> UserPatch([FromBody] APIUpdateUserMobileEmail apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (apiUser.MobileNumber.Trim().Length != 10)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberLessNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberLessNotExists) });
                    }

                    if (await this._userRepository.ExistsForUpdate("mobile", apiUser.MobileNumber, UserId))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                    }
                    else if (await this._userRepository.ExistsForUpdate("email", apiUser.EmailId, UserId))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateEmailId), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateEmailId) });
                    }
                    else
                    {

                        await this._userRepository.UserPatch(apiUser, OrgCode, UserId);
                        return this.Ok(apiUser);
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        #endregion UpdateUserProfile





        [HttpPost]
        [Route("GetTypeAhead")]
        public async Task<IActionResult> GetTypeAhead([FromBody] UserSearch apiUserSearch)
        {
            try
            {
                apiUserSearch.SearchByColumn = Security.DecryptForUI(apiUserSearch.SearchByColumn);
                apiUserSearch.SearchText = Security.DecryptForUI(apiUserSearch.SearchText);

                return Ok(await this._userRepository.Search(apiUserSearch.SearchByColumn, apiUserSearch.SearchText, OrgCode, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("GetTypeAhead/{search?}")]
        public async Task<IActionResult> GetTypeAheadUserName(string search)
        {
            try
            {
                return Ok(await this._userRepository.SearchUserName(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [Route("GetEncryptedUserId")]
        public async Task<IActionResult> GetEncryptedUserId()
        {
            try
            {
               
                 string EncryptedUserId = await this._userRepository.GetEncryptedUserId(UserId);
                 return Ok(new { EncryptedUserId });

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("GetNameById/{orgCode}/{searchBy}/{id}")]
        public async Task<IActionResult> GetNameById(string orgCode, string searchBy, int id)
        {
            try
            {
                string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(orgCode);
                if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                {
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                }

                if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "Invalid Organization Code! Please contact System Administrator to know your Organization Code!"
                    });

                }
                this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                return Ok(await this._userRepository.GetNameById(searchBy, id));
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("UserLogOut")]
        public async Task<IActionResult> UserLogOut()
        {
            try
            {
                LoggedInHistory loggedInHistory = await this._loggedInHistoryRepository.GetLatestRecord(UserId);

                if (loggedInHistory == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });

                loggedInHistory.LogOutTime = DateTime.UtcNow;
                loggedInHistory.LocalLoggedOutTime = DateTime.Now;
                DateTime localLoggedIn = (DateTime)loggedInHistory.LocalLoggedInTime;
                DateTime localLoggedOut = DateTime.Now;
                int DateDifference = Convert.ToInt32((localLoggedOut - localLoggedIn).Minutes);
                loggedInHistory.TotalTimeInMinutes = DateDifference;

                await this._loggedInHistoryRepository.Update(loggedInHistory);

                //add token entry in black list if the user has log out
                TokenBlacklist tokenBlacklist = new TokenBlacklist
                {
                    CreatedDate = DateTime.Now,
                    Token = Token.Substring(Token.Length - 100),
                    UserId = UserId
                };
                await this._tokenBlacklistRepository.Add(tokenBlacklist);

                if (!string.IsNullOrEmpty(FCMToken))
                {
                    await this._tokensRepository.DeleteFCMToken(UserId, FCMToken);
                }

                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




        [HttpPost]
        [Route("UserDataExists")]
        [PermissionRequired(Permissions.userconfiguration)]
        public async Task<IActionResult> UserDataExists([FromBody] ApiName apiName)
        {
            try
            {

                return Ok(await _userRepository.UserDataExist(apiName.Name));
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UserLoginRewardPoints")]
        public async Task<IActionResult> UserLoginRewardPoints()
        {
            try
            {
                int Count = await this._rewardsPoint.FirstTimeLoggedInLogin(UserId);
                if (Count == 0)
                {
                    if (await this._loggedInHistoryRepository.FirstTimeLoggedIn(UserId) == 1)
                    {
                        await this._rewardsPoint.AddFirstTimeLoginRewardPoints(UserId);
                    }
                }
                else
                {

                    await this._rewardsPoint.AddDailyLoginRewardPoints(UserId);

                }
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("GetTopRanking")]
        public async Task<IActionResult> GetTopRanking([FromBody] ApiGetTopRanking apiGetTopRanking)
        {
            try
            {
                int? userId = null;

                if (apiGetTopRanking.configuredColumnName != null)
                    apiGetTopRanking.configuredColumnName = apiGetTopRanking.configuredColumnName.ToLower().Equals("null") ? null : apiGetTopRanking.configuredColumnName;

                if (apiGetTopRanking.configuredColumnValue != null)
                    apiGetTopRanking.configuredColumnValue = apiGetTopRanking.configuredColumnValue.ToLower().Equals("null") ? null : apiGetTopRanking.configuredColumnValue;

                if (apiGetTopRanking.houseCode != null)
                    apiGetTopRanking.houseCode = apiGetTopRanking.houseCode.Equals("null") ? null : apiGetTopRanking.houseCode;


                if (!string.IsNullOrEmpty(apiGetTopRanking.configuredColumnValue))
                {
                    apiGetTopRanking.configuredColumnValue = apiGetTopRanking.configuredColumnValue.ToLower();

                }

                if (!string.IsNullOrEmpty(apiGetTopRanking.configuredColumnName))
                {
                    apiGetTopRanking.configuredColumnName = apiGetTopRanking.configuredColumnName.ToLower();
                }
                userId = UserId; // if config value is not null then get details based on logged in user
                IEnumerable<APIRanking> result = await this._rewardsPoint.GetTopRanking(apiGetTopRanking.ranks, userId, apiGetTopRanking.configuredColumnName, apiGetTopRanking.houseCode, false, OrgCode, apiGetTopRanking.configuredColumnValue, IsInstitute); //get top 10 ranking if id is null

                if (result.Count() == 0)
                    return this.NoContent();
                else
                    return this.Ok(result);


            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("DatewiseLeaderBoard")]
        public async Task<IActionResult> DatewiseLeaderBoard([FromBody] APIRewardLeaderBoardDate apiGetTopRankingDateWise)
        {
            try
            {
                List<APIRewardLeaderBoard> result = await this._rewardsPoint.GetDatewiseLeaderBoardData(apiGetTopRankingDateWise, UserId);
                if (result.Count() == 0)
                    return this.NoContent();
                else
                    return this.Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }






        [HttpPost("GetMyRanking")]
        public async Task<IActionResult> GetMyRanking([FromBody] ApiGetTopRanking apiGetTopRanking)
        {
            try
            {
                if (apiGetTopRanking.configuredColumnName != null)
                    apiGetTopRanking.configuredColumnName = apiGetTopRanking.configuredColumnName.ToLower().Equals("null") ? null : apiGetTopRanking.configuredColumnName;

                if (apiGetTopRanking.houseCode != null)
                    apiGetTopRanking.houseCode = apiGetTopRanking.houseCode.Equals("null") ? null : apiGetTopRanking.houseCode;

                if (!string.IsNullOrEmpty(apiGetTopRanking.configuredColumnName))
                {
                    apiGetTopRanking.configuredColumnName = apiGetTopRanking.configuredColumnName.ToLower();
                }

                IEnumerable<APIRanking> ranking = new List<APIRanking>();
                ranking = await this._rewardsPoint.GetMyRanking(null, UserId, apiGetTopRanking.configuredColumnName, apiGetTopRanking.houseCode, false, IsInstitute);

                if (ranking != null && !ranking.Any()) // get data for AppearOnLeaderboard is true
                    ranking = await this._rewardsPoint.GetMyRanking(apiGetTopRanking.ranks, UserId, apiGetTopRanking.configuredColumnName, apiGetTopRanking.houseCode, true, IsInstitute);

                return Ok(ranking);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("SevenDayCount")]
        public async Task<IActionResult> SevenDayCount()
        {
            try
            {
                int SevenDayCount = await this._rewardsPoint.GetSevenDayCount(UserId);
                return Ok(new { SevenDayCount });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("AddSevenDayPoints")]
        public async Task<IActionResult> AddSevenDayPoints()
        {
            try
            {
                int Result = await this._rewardsPoint.AddSevenDayPoints(UserId);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return Ok("Success");
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetUserRole")]
        public async Task<IActionResult> GetUserRole()
        {
            string role = await this._userRepository.GetUserRole(UserId);
            return this.Ok(new { role });
        }

        [HttpGet("GetUserConfiguration")]
        public async Task<IActionResult> GetUserConfiguration()
        {
            try
            {
                if (UserName == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                APIUserConfiguration apiUserConfiguration = await this._userRepository.GetUserConfiguration(UserName, UserId, OrgCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                apiUserConfiguration.ProfilePictureFullPath = string.Empty;
                return Ok(apiUserConfiguration);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "Error", Description = ex.Message });
            }
        }

        [HttpGet("GetUserDashboardConfiguration")]
        public async Task<IActionResult> GetUserDashboardConfiguration()
        {
            try
            {
                if (UserName == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                APIUserDashboardConfiguration apiUserConfiguration = await this._userRepository.GetUserDashboardConfiguration(UserName, UserId, OrgCode);
                return Ok(apiUserConfiguration);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "Error", Description = ex.Message });
            }
        }


        [HttpGet("GetCEOProfile")]
        public async Task<IActionResult> GetCEOProfile()
        {
            try
            {
                if (UserName == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                APIUserDashboardConfiguration apiUserConfiguration = await this._userRepository.GetUserDashboardConfiguration(UserName, UserId, OrgCode);
                if (Path.GetExtension(apiUserConfiguration.CEOProfilePicture) == ".jpeg")
                {
                    return PhysicalFile(this._configuration["ApiGatewayCEOFiles"] + "/" + apiUserConfiguration.CEOProfilePicture, "image/jpeg");
                }
                else if (Path.GetExtension(apiUserConfiguration.CEOProfilePicture) == ".jpg")
                {
                    return PhysicalFile(this._configuration["ApiGatewayCEOFiles"] + "/" + apiUserConfiguration.CEOProfilePicture, "image/jpeg");
                }
                return PhysicalFile(this._configuration["ApiGatewayCEOFiles"] + "/" + apiUserConfiguration.CEOProfilePicture, "image/jpeg");
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "Error", Description = ex.Message });
            }
        }



        [HttpPost("orgnization/typeahead")]
       // [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> OrgnizationTypeAhead([FromBody] APISearch apiSearch)
        {

            return this.Ok(await this._userRepository.GetUserNameAndId(apiSearch.Search));
        }

        [HttpPost("AddUserHR")]
        [PermissionRequired(Permissions.UserHRAssosiation)]
        public async Task<IActionResult> AddUserHR([FromBody] APIUserHRAssociation aPIUserHRAssociation)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);
                int Result = await _userRepository.AddUserHRAssociation(aPIUserHRAssociation, UserId);
                if (Result == 0)
                    return StatusCode(409, Json("Duplicate"));
                return this.Ok(aPIUserHRAssociation);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetHRAssociation/{page:int}/{pageSize:int}")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IEnumerable<UserHRAssociation>> GetHRAssociationData(int page, int pageSize)
        {
            return await this._userRepository.GetHRAssociationData(page, pageSize);

        }

        [HttpGet("GetTotalHRRecords")]
        [PermissionRequired(Permissions.UserHRAssosiation)]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var Count = await this._userRepository.GetHRCount();
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }






        [HttpPost("UpdateHRA")]
        [PermissionRequired(Permissions.UserHRAssosiation)]
        public async Task<IActionResult> UpdateHRA([FromBody] APIUserHRAssociation param)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                UserHRAssociation userHRAssociation = await this._userRepository.GetUserHRA(param.Id);

                if (userHRAssociation == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                }

                if (userHRAssociation != null)
                {
                    userHRAssociation.UserMasterId = param.UserMasterId;
                    userHRAssociation.UserName = param.UserName;
                    userHRAssociation.ModifiedBy = UserId;
                    userHRAssociation.ModifiedDate = DateTime.UtcNow;
                    userHRAssociation.Level = param.Level;

                    if (await this._userRepository.UpdateUserHRA(userHRAssociation) == 1)
                        return Ok();
                }

                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpPost("GetHRByID")]
        [PermissionRequired(Permissions.UserHRAssosiation)]
        public async Task<IActionResult> GetById([FromBody] APIIntId apiIntId)
        {
            try
            {
                var result = await this._userRepository.GetHRByID(apiIntId.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }





        [HttpPost("UserHR")]
        [PermissionRequired(Permissions.UserHRAssosiation)]
        public async Task<IActionResult> UpdateHR([FromBody] APIUserHRAssociation param)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                UserHRAssociation userHRAssociation = await this._userRepository.GetUserHR(param.Id);

                if (userHRAssociation == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                }

                if (userHRAssociation != null)
                {
                    userHRAssociation.Id = param.Id;
                    userHRAssociation.UserName = param.UserName;
                    userHRAssociation.ModifiedBy = UserId;
                    userHRAssociation.ModifiedDate = DateTime.UtcNow;
                    userHRAssociation.Level = param.Level;

                    if (await this._userRepository.UpdateUserHR(userHRAssociation) == 1)
                        return Ok();
                }

                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }

        }


        [HttpGet("GetAllHouseMaster")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<HouseMaster> housemasterList = await this._userRepository.GetAllHouseMaster();
                return Ok(housemasterList);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetHouseMasterByID")]
        [PermissionRequired(Permissions.HouseMaster)]
        public async Task<IActionResult> GetByID([FromBody] APIIntId apiIntId)
        {
            try
            {
                var result = await this._userRepository.GetHouseMasterByID(apiIntId.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }





        [HttpPost("UpdateHouseMasterById")]
        [PermissionRequired(Permissions.HouseMaster)]
        public async Task<IActionResult> UpdateHouseMaster([FromBody] APIHouseMaster param)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                HouseMaster housemaster = await this._userRepository.GetHouseMaster(param.Id);

                if (housemaster == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                }
                if (await this._userRepository.ExistsCode(param.Code))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                if (housemaster != null)
                {

                    housemaster.Name = param.Name;

                    if (await this._userRepository.UpdateHouseMaster(housemaster) == 1)
                        return Ok();
                }

                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }


        [HttpDelete("HRADelete")]
        [PermissionRequired(Permissions.UserHRAssosiation)]
        public async Task<IActionResult> HRADelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await this._userRepository.HRADelete(DecryptedId);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

       /* [HttpDelete("HRADelete")]
        [PermissionRequired(Permissions.UserHRAssosiation)]
        public async Task<IActionResult> HRADelete([FromBody] APIIntId apiIntId)
        {
            try
            {
                int Result = await this._userRepository.HRADelete(apiIntId.Id);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }*/



        [HttpPost]
        [Route("GetDynamicColumnDetails")]
        [PermissionRequired(Permissions.UserTrainingAdminAssosiation + " " + Permissions.BuisnessHeadResponsetorequest + " " + Permissions.linemananagerapproval + " " + Permissions.HRResponsetorequest)]
        public async Task<IActionResult> GetDynamicColumnDetails([FromBody] ApiName apiName)
        {
            try
            {
                return Ok(await this._userRepository.GetDynamicColumnDetails(apiName.Name));
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "Error", Description = ex.Message });
            }
        }



        [HttpPost]
        [Route("GetUsersOfTA")]
        [PermissionRequired(Permissions.RequestNomination)]
        public async Task<IActionResult> GetUsersOfTA([FromBody] APISearch apiSearch)
        {
            try
            {
                return Ok(await this._userRepository.GetUsersForTA(UserId, apiSearch.Search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("AddUserAdminTraining")]
        public async Task<IActionResult> AddUserAdminTraining([FromBody] APIUserTrainingAdminAssociation aPIUserTrainingAdminAssociation)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);
                int Result = await _userRepository.AddUserAdminTrainingAssociation(aPIUserTrainingAdminAssociation, UserId);
                if (Result == 0)
                    return StatusCode(409, Json("Duplicate"));
                return this.Ok(aPIUserTrainingAdminAssociation);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetTrainingAdminAssociation/{page:int}/{pageSize:int}")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IEnumerable<UserTrainingAdminAssociation>> GetTrainingAdminAssociationData(int page, int pageSize)
        {
            try
            {
                return await this._userRepository.GetTrainingAdminAssociationData(page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpGet("GetTotalTrainingAdminRecords")]
        [PermissionRequired(Permissions.UserTrainingAdminAssosiation)]
        public async Task<IActionResult> GetTotalTrainingAdminRecords()
        {
            try
            {
                var Count = await this._userRepository.GetTrainingAdminCount();
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpDelete("TrainingAdminDelete")]
        public async Task<IActionResult> TrainingAdminDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await this._userRepository.TrainingAdminDelete(DecryptedId);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        //[HttpDelete("TrainingAdminDelete")]
        //public async Task<IActionResult> TrainingAdminDelete([FromBody] APIIntId apiIntId)
        //{
        //    try
        //    {
        //        int Result = await this._userRepository.TrainingAdminDelete(apiIntId.Id);
        //        if (Result == 0)
        //            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
        //        return this.Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
        //    }
        //}


        [HttpPost]
        [Route("GetUsersOfDept")]
        [PermissionRequired(Permissions.UserTrainingAdminAssosiation)]
        public async Task<IActionResult> GetUsersOfDept([FromBody] ApiDepartmentSearch apiDepartmentSearch)
        {
            try
            {
                return Ok(await this._userRepository.GetUsersOfDept(apiDepartmentSearch.DeptId, apiDepartmentSearch.Search));

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AddOrganizationPreferences")]
        [PermissionRequired(Permissions.OrganizationPreference)]
        public async Task<IActionResult> AddOrganizationPreferences([FromForm] APIOrganizationPreferences organizationPreferences)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);
                int Result = await _userRepository.AddOrganizationPrefernces(organizationPreferences, UserId, OrgCode);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok(1);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string Exception = ex.Message;
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }




        [HttpPost("UpdateOrganizationLogo")]
        [PermissionRequired(Permissions.OrganizationPreference)]
        public async Task<IActionResult> UpdateOrganizationLogo([FromForm] APIOrganizationPreferences aPIOrganization)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int result = await this._userRepository.UpdateOrganizationPrefernces(aPIOrganization.Id, aPIOrganization, UserId, OrgCode);
                if (result == 0)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                }
                return Ok(aPIOrganization);

            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpGet("GetAllImagesData")]
        public async Task<IActionResult> GetAllImages()
        {
            try
            {

                var response = await this._userRepository.GetAllImages();
                return this.Ok(response);
            }
            catch (Exception ex)
            {

                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetOrganizationData")]
        [PermissionRequired(Permissions.OrganizationPreference)]
        public async Task<IActionResult> GetOrganizationData()
        {
            try
            {
                APIOrganizationPreferences aPIOrganization = new APIOrganizationPreferences();
                OrganizationPreferences organizationPreferences = await this._userRepository.GetOrganizationLogo();
                if (organizationPreferences == null)
                {
                    _logger.Error("Organization Details Not Found.");
                    return this.BadRequest(new ResponseMessage { Message = "Organization Details Not Found.", Description = "Organization Details Not Found." });

                }
                aPIOrganization.Id = organizationPreferences.Id;
                aPIOrganization.LandingPage = organizationPreferences.LandingPage;
                aPIOrganization.Language = organizationPreferences.Language;
                aPIOrganization.ColorCode = organizationPreferences.ColorCode;
                aPIOrganization.LogoPath = organizationPreferences.LogoPath;
                return this.Ok(aPIOrganization);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("LoggedInHistory/{page}/{pageSize}/{userID?}")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> GetLoggedInHistory(int page, int pageSize, int? userID = null)
        {
            try
            {
                if (userID == null)
                {
                    return Ok(await this._userRepository.GetLoggedInHistory(UserId, page, pageSize));
                }
                else
                {
                    return Ok(await this._userRepository.GetLoggedInHistory(userID, page, pageSize));
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpPost("LoggedInHistory")]
        public async Task<IActionResult> GetLoggedInHistory([FromBody] ApiLoggedHistory apiLoggedHistory)
        {
            try
            {
                if (apiLoggedHistory.userID == null)
                {
                    return Ok(await this._userRepository.GetLoggedInHistory(UserId, apiLoggedHistory.page, apiLoggedHistory.pageSize));
                }
                else
                {
                    //apiLoggedHistory.userID = Security.Decrypt(apiLoggedHistory.userID);
                    return Ok(await this._userRepository.GetLoggedInHistory(Convert.ToInt32(UserId), apiLoggedHistory.page, apiLoggedHistory.pageSize));
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }




        [HttpGet("LoggedInHistoryCount/{userID?}")]
        public async Task<IActionResult> LoggedInHistoryCount(int? userID = null)
        { //Access For End User 
            try
            {
                if (userID == null)
                {
                    return Ok(await this._userRepository.LoggedInHistoryCount(UserId));
                }
                else
                {
                    return Ok(await this._userRepository.LoggedInHistoryCount(userID));
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }



        [HttpGet("GetConfigurableParameterValue/{value}")]
        public async Task<IActionResult> GetConfigurableParameterValue(string value)
        {
            try
            {
                return Ok(await this._userRepository.GetConfigurationValueAsync(value, OrgCode));
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpPost("UserConfigurableParameter")]
        public async Task<IActionResult> UserConfigurableParameter([FromBody] string [] str_arr )
        {
            try
            {
                return Ok(await this._userRepository.GetUserConfigurationValueAsync(str_arr, OrgCode));
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpPost]
        [Route("AddBusiness")]
        [PermissionRequired(Permissions.organization_master)]
        public async Task<IActionResult> Post([FromBody] APIBusiness business)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await this._businessRepository.Exists(business.Name, business.Code))
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                else
                {

                    Business objbusiness = new Business
                    {
                        Name = business.Name,
                        NameEncrypted = Security.Encrypt(business.Name),
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = 0,
                        Code = business.Code,
                        Theme = business.Theme,
                        LogoName = business.LogoName
                    };
                    await _businessRepository.Add(objbusiness);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




        [HttpPost]
        [Route("PutBusiness/{id}")]
        [PermissionRequired(Permissions.organization_master)]
        public async Task<IActionResult> PutBusiness(int id, [FromBody] APIBusiness business)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Business objbusiness = await this._businessRepository.Get(id);
                if (objbusiness == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else if (await this._businessRepository.Exists(business.Name, business.Code, business.Id))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                else if (objbusiness.Code != business.Code)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else
                {
                    objbusiness.Name = business.Name;
                    objbusiness.NameEncrypted = Security.Encrypt(business.Name);
                    objbusiness.Code = business.Code;
                    objbusiness.Theme = business.Theme;
                    objbusiness.LogoName = business.LogoName;
                    await _businessRepository.Update(objbusiness);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("Business")]
        [PermissionRequired(Permissions.organization_master)]
        public async Task<IActionResult> BusinessDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Business objbusiness = await this._businessRepository.Get(DecryptedId);
                if (objbusiness == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (await _businessRepository.IsDependacyExist(DecryptedId))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                await _businessRepository.Remove(objbusiness);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAllBusiness/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.organization_master)]
        public async Task<IActionResult> GetBusiness(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await _businessRepository.GetAll(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("BusinessCount/{search?}")]
        [PermissionRequired(Permissions.organization_master)]
        public async Task<IActionResult> GetBusinessCount(string search = null)
        {
            try
            {
                return Ok(await _businessRepository.Count(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetOrganizationDetails")]
        public async Task<IActionResult> GetOrganizationDetails()
        {// Access for END User
            try
            {
                string subOrgValue = SubOrganization == "-" ? string.Empty : SubOrganization;
                return Ok(new { CustomerCode, subOrgValue, Theme, Logoname, LogonameDark, IsInstitute });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }


        [HttpGet("GetHouseRewardPointCount")]
        public async Task<IActionResult> GetHouseRewardPointCount()
        {
            //Access For End User 
            try
            {
                return Ok(await this._rewardsPoint.GetHouseRewardPointCount());
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("ChangeSelectedUserPassword")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> ChangeSelectedUserPassword([FromBody] ApiUserId apiUserId)
        {
            try
            {
                apiUserId.UserId = Security.DecryptForUI(apiUserId.UserId);
                APIUserMaster user = await this._userRepository.GetUser(Convert.ToInt32(apiUserId.UserId));

                if (user == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });

                var allowRANDOMPASSWORD = await _userRepository.GetConfigurationValueAsync("RANDOM_PASSWORD", OrgCode);
                string userpassword = this._configuration["DeafultPassword"];
                if (Convert.ToString(allowRANDOMPASSWORD).ToLower() == "yes")
                {
                    userpassword = RandomPassword.GenerateUserPassword(8, 1);
                    user.Password = Helper.Security.EncryptSHA512(userpassword);
                }
                else if (OrgCode == "bandhan" || OrgCode == "bandhanbank" )
                {
                    if (user.DateOfBirth == null)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.MissingBirthDate) });
                    user.Password = Security.EncryptSHA512(user.DateOfBirth?.ToString("MMyyyy"));

                }
                else
                {
                    if (OrgCode.ToLower().Contains("keventers"))
                    {
                        userpassword = "Keventers@123";
                        user.Password = Helper.Security.EncryptSHA512(userpassword);
                    }
                    else
                    {
                        if (OrgCode.ToLower() == "pepperfry")
                        {
                            user.Password = Helper.Security.EncryptSHA512("123456");
                        }
                        else
                        {
                            user.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);
                        }

                    }
                }

                user.UserId = Security.Encrypt(user.UserId);
                user.MobileNumber = Security.Encrypt(user.MobileNumber);
                user.EmailId = Security.Encrypt(user.EmailId);
                user.ModifiedDate = DateTime.UtcNow;
                user.ReportsTo = Security.Encrypt(user.ReportsTo == null ? null : user.ReportsTo.ToLower());
                user.IsPasswordModified = false;

                await _userRepository.UpdateUser(user);

                var allowUSERPASSWORD = await _userRepository.GetConfigurationValueAsync("USER_PASSWORD", OrgCode);

                string cslEmpCode = await this._userRepository.GetCslEmpCode(user.Id);

                if (Convert.ToString(allowUSERPASSWORD).ToLower() == "yes")
                {
                    this.SendRandomPassword_Mail(Convert.ToString(user.Id), userpassword, Security.Decrypt(user.EmailId), OrgCode, user.UserName, user.UserId);
                    this.SendRandomPassword_SMS(Convert.ToString(user.Id), userpassword, Security.Decrypt(user.MobileNumber), OrgCode, user.UserName, user.UserId, cslEmpCode);
                }
                bool PasswordChanged = true;
                return Ok(new { PasswordChanged });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("getUserType")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> getUserType([FromBody] ApiUserId apiUserId)
        {
            try
            {
                string userType = await this._userRepository.GetUserType(apiUserId.UserId);
                if (userType == "EMPLOYEE")
                    return Ok(this._userRepository.checkuserexistanceinldap(apiUserId.UserId));
                return Ok(null);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetStatusCodeResult")]
        public async Task<IActionResult> GetStatusCodeResult()
        { //Access For End User 
            try
            {
                string statuCode = StatusCodeResult == "-" ? string.Empty : StatusCodeResult;
                string statuCodeMessage = StatusCodeMessage == "-" ? string.Empty : StatusCodeMessage;
                return Ok(new { statuCode, statuCodeMessage });
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }
        //added to check user token after login
        [HttpPost]
        [Route("CheckUserToken")]
        public async Task<IActionResult> CheckUserToken()
        {  //Access For End User 
            try
            {
                if (!string.IsNullOrEmpty(FCMToken))
                {
                    await this._tokensRepository.AddFCMToken(UserId, FCMToken,IsiOS);
                }
                bool IsCheckToken = Convert.ToBoolean(this._configuration["CheckTocken"]);
                if (IsCheckToken == true)
                {
                    if (await new TokenRequiredFilter(_tokensRepository, _userRepository, _identitySvc, _configuration).GetMultipleLoginConfigValueAsync(OrgCode) == false)
                        return Ok(await this._tokensRepository.CheckUserToken(UserId, Token.Substring(Token.Length - 100)));
                }
                return Ok(1);

            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("UserTokenExists")]
        public async Task<IActionResult> UserTokenExists(string token)
        { //Access For End User 
            try
            {
                return Ok(await this._tokensRepository.UserTokenExists(token.Substring(token.Length - 100)));
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetBespokeUserDetails")]
        public async Task<IActionResult> GetBespokeUserDetails()
        {
            try
            {
                return this.Ok(await this._userRepository.GetUserByBespoke(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("GetUsernamefromgroup")]
        [PermissionRequired(Permissions.TrainingPassportReport)]
        public async Task<IActionResult> GetUsernamefromgroup([FromBody] ApiUserNameByGroup apiUserNameByGroup)
        {
            try
            {
                return this.Ok(await this._userRepository.GetUsernamefromgroup(apiUserNameByGroup.Groupname, apiUserNameByGroup.Username));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet]
        [Route("ExportRejected")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> ExportRejected()
        {
            try
            {
                var userrejected = await this._userMasterRejectedRepository.GetAllUsersRejected();
                string OrgCode = Security.Decrypt(_identitySvc.GetCustomerCode());
                String ExcelName = @UserMasterImportField.UserRejected;

                string WwwRootFolder = this._configuration["ApiGatewayWwwroot"];
                WwwRootFolder = Path.Combine(WwwRootFolder, OrgCode);
                if (!Directory.Exists(WwwRootFolder))
                {
                    Directory.CreateDirectory(WwwRootFolder);
                }
                string FileName = ExcelName;
                FileInfo file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Courses");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "UserId";
                    worksheet.Cells[1, 2].Value = "EmailId";
                    worksheet.Cells[1, 3].Value = "UserName";
                    worksheet.Cells[1, 4].Value = "Mobile Number";
                    worksheet.Cells[1, 5].Value = "Date";
                    worksheet.Cells[1, 6].Value = "Error Message";

                    int row = 2, column = 1;
                    foreach (APIUserReject userReject in userrejected)
                    {
                        worksheet.Cells[row, column++].Value = userReject.UserId;
                        worksheet.Cells[row, column++].Value = userReject.EmailId;
                        worksheet.Cells[row, column++].Value = userReject.UserName;
                        worksheet.Cells[row, column++].Value = userReject.MobileNumber;
                        worksheet.Cells[row, column++].Value = userReject.ModifiedDate;
                        worksheet.Cells[row, column++].Value = userReject.ErrorMessage;

                        row++;
                        column = 1;
                    }
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;

                    }
                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                if (file.Exists)
                {
                    file.Delete();

                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet]
        [Route("ExportUserMasterStatus")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> ExportUserMasterStatus()
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "UserMasterStatusImport.xlsx";
                string URL = string.Concat(DomainName, "/", sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("UserMasterStatusImport");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "UserId*";
                    worksheet.Cells[1, 2].Value = "Status*";


                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;

                    }
                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("SaveStatusImportFileData")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> SaveStatusImportFileData([FromBody] APIFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                //get customerCode from token
                string customerCode = Security.Decrypt(_identitySvc.GetCustomerCode());
                if (customerCode == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }

                string customerCodeCap = customerCode.ToUpper();
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string filefinal = sWebRootFolder + aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(filefinal));
                string result = await this._userRepository.ProcessImportFile(file,
                    _userSettingsRepository,
                    _userRepository,
                    _userMasterRejectedRepository,
                    _configuration,
                   _customerConnectionStringRepository);
                return Ok(new { result });





            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }

        }


        [HttpPost("GetAllDeletedUser")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.iltnominate)]
        public async Task<IActionResult> GetAllDeletedUser([FromBody] APIUserSearch apiUserSearch)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (apiUserSearch.Search != null)
                        apiUserSearch.Search = apiUserSearch.Search.ToLower().Equals("null") ? null : apiUserSearch.Search;
                    if (apiUserSearch.ColumnName != null)
                        apiUserSearch.ColumnName = apiUserSearch.ColumnName.ToLower().Equals("null") ? null : apiUserSearch.ColumnName;


                    IEnumerable<APIUserMasterDelete> users = await this._userRepository.GetAllDeletedUser(apiUserSearch.Page, apiUserSearch.PageSize, apiUserSearch.Search, apiUserSearch.ColumnName, UserId);
                    return this.Ok(users);
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest();
            }
        }




        [HttpPost("GetDeletedUserCount")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.iltnominate)]
        public async Task<IActionResult> GetDeletedUserCount([FromBody] APIUserSearch apiUserSearch)
        {


            if (!ModelState.IsValid)
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });


            if (apiUserSearch.Search != null)
                apiUserSearch.Search = apiUserSearch.Search.ToLower().Equals("null") ? null : apiUserSearch.Search;
            if (apiUserSearch.ColumnName != null)
                apiUserSearch.ColumnName = apiUserSearch.ColumnName.ToLower().Equals("null") ? null : apiUserSearch.ColumnName;




            var count = await this._userRepository.GetDeletedUserCount(apiUserSearch.Search, apiUserSearch.ColumnName, UserId);
            return this.Ok(count);
        }
        [HttpPost("GetDeletedUserInfo")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> GetDeletedUserInfo([FromBody] ApiGetUserDeleteById getuserbyid)
        {
            try
            {
                getuserbyid.ID = Security.DecryptForUI(getuserbyid.ID);

                APIUserMasterDelete user = await this._userRepository.GetDeletedUserInfo(Convert.ToInt32(getuserbyid.ID));
                user.Password = null;
                return this.Ok(user);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllUserRejectedStatus")]

        public async Task<IEnumerable<UserRejectedStatus>> GetAllUserRejectedStatus()
        {
            try
            {
                return await this._userRepository.GetAllUserRejectedStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpGet("GetMobilePermissions")]
        public async Task<IEnumerable<APIMobilePermissions>> GetMobilePermissions()
        {
            try
            {
                return await this._userRepository.GetMobilePermissions(UserRole, UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }

        [HttpPost("GetUserDetailsByUserId")]
        public async Task<IActionResult> GetUserDetailsByUserId([FromBody] ApiGetUserDeleteById apiGetUser)
        {
            try
            {
                return Ok(await this._userRepository.GetUserDetailsByUserId(apiGetUser.ID));
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("Export")]
        public async Task<IActionResult> Export([FromBody] APIGetTopRankingExport apiGetTopRanking)
        {
            try
            {

                int? userId = null;

                if (apiGetTopRanking.configuredColumnName != null)
                    apiGetTopRanking.configuredColumnName = apiGetTopRanking.configuredColumnName.ToLower().Equals("null") ? null : apiGetTopRanking.configuredColumnName;

                if (apiGetTopRanking.houseCode != null)
                    apiGetTopRanking.houseCode = apiGetTopRanking.houseCode.Equals("null") ? null : apiGetTopRanking.houseCode;

                if (!string.IsNullOrEmpty(apiGetTopRanking.configuredColumnName))
                {
                    apiGetTopRanking.configuredColumnName = apiGetTopRanking.configuredColumnName.ToLower();
                    userId = UserId; // if config value is not null then get details based on logged in user
                }
                string OrgCode = Security.Decrypt(_identitySvc.GetCustomerCode());
                var HouseInformation = await _userRepository.GetConfigurationValueAsync("SHOW_HOUSE_ON_LEADERBOARD", OrgCode);
                var topranking = await this._rewardsPoint.GetTopRankingForExport(apiGetTopRanking.ranks, userId, apiGetTopRanking.configuredColumnName, apiGetTopRanking.houseCode, false, OrgCode, apiGetTopRanking.configuredColumnValue);
                String ExcelName = @UserMasterImportField.TopRanking;

                string WwwRootFolder = this._configuration["ApiGatewayWwwroot"];
                WwwRootFolder = Path.Combine(WwwRootFolder, OrgCode);
                if (!Directory.Exists(WwwRootFolder))
                {
                    Directory.CreateDirectory(WwwRootFolder);
                }
                string FileName = ExcelName;
                FileInfo file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
                }

                using (ExcelPackage package = new ExcelPackage(file))
                {
                    if (HouseInformation.ToLower() == "yes")
                    {
                        // add a new worksheet to the empty workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TopRanking");
                        //First add the headers
                        worksheet.Cells[1, 1].Value = "Rank";
                        worksheet.Cells[1, 2].Value = "Employee Code";
                        worksheet.Cells[1, 3].Value = "User Name";
                        worksheet.Cells[1, 4].Value = "House Code";
                        worksheet.Cells[1, 5].Value = "Club";
                        worksheet.Cells[1, 6].Value = "Score";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 7].Value = "User Category";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 8].Value = "Sales Area";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 9].Value = "Sales Officer Name";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 10].Value = "Controlling Office";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 11].Value = "District";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 12].Value = "Designation";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 13].Value = "User Status";

                        int row = 2, column = 1;
                        foreach (APIRankingExport course in topranking)
                        {
                            worksheet.Cells[row, column++].Value = course.Rank;
                            worksheet.Cells[row, column++].Value = Security.Decrypt(course.EUSerId);
                            worksheet.Cells[row, column++].Value = course.UserName;
                            worksheet.Cells[row, column++].Value = course.HouseCode;
                            worksheet.Cells[row, column++].Value = course.Level;
                            worksheet.Cells[row, column++].Value = course.TotalPoint;

                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.UserCategory;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.SalesArea;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.SalesOfficer;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.ControllingOffice;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.District;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.Designation;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.UserStatus == true ? "Active" : "InActive";
                            row++;
                            column = 1;
                        }
                        using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;

                        }
                    }
                    else
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TopRanking");
                        //First add the headers
                        worksheet.Cells[1, 1].Value = "Rank";
                        worksheet.Cells[1, 2].Value = "Employee Code";
                        worksheet.Cells[1, 3].Value = "User Name";

                        worksheet.Cells[1, 4].Value = "Club";
                        worksheet.Cells[1, 5].Value = "Score";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 6].Value = "User Category";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 7].Value = "Sales Area";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 8].Value = "Sales Officer Name";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 9].Value = "Controlling Office";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 10].Value = "District";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 11].Value = "Designation";

                        if (OrgCode.ToLower().Contains("iocl"))
                            worksheet.Cells[1, 12].Value = "User Status";
                        int row = 2, column = 1;
                        foreach (APIRankingExport course in topranking)
                        {
                            worksheet.Cells[row, column++].Value = course.Rank;
                            worksheet.Cells[row, column++].Value = Security.Decrypt(course.EUSerId);
                            worksheet.Cells[row, column++].Value = course.UserName;

                            worksheet.Cells[row, column++].Value = course.Level;
                            worksheet.Cells[row, column++].Value = course.TotalPoint;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.UserCategory;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.SalesArea;

                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.SalesOfficer;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.ControllingOffice;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.District;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.Designation;
                            if (OrgCode.ToLower().Contains("iocl"))
                                worksheet.Cells[row, column++].Value = course.UserStatus == true ? "Active" : "InActive";

                            row++;
                            column = 1;
                        }
                        using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;

                        }
                    }
                    package.Save(); //Save the workbook.
                }

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                if (file.Exists)
                {
                    file.Delete();

                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetOAuthUser")]
        public async Task<IActionResult> GetOAuthUser()
        {
            try
            {
                if (OrgCode.ToLower().Trim().Equals("titan"))
                {
                    return this.Ok(await this._userRepository.GetOAuthUser(Security.Encrypt(UserName)));
                }
                else
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("GetConfigurationColumnsData")]
        public async Task<IActionResult> GetConfigurationColumsData([FromBody] APIConfigurationColumns aPIConfigurationColumns)
        {

            try
            {
                return Ok(await this._userRepository.GetConfigurationColumsData(aPIConfigurationColumns.configurationcolumn));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetConfigurationColumnsDataForDashboard")]
        public async Task<IActionResult> GetConfigurationColumnsDataForDashboard([FromBody] APIConfigurationColumns aPIConfigurationColumns)
        {

            try
            {
                return Ok(await this._userRepository.GetConfigurationColumsDataForDashboard(aPIConfigurationColumns.configurationcolumn));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        private bool SendRandomPassword_Mail(string UserID, string DefaultPassword, string EmailId, string OrganisationCode, string UserName, string userId)
        {
            string url = _configuration[Configuration.NotificationApi];
            url += "/RandomPasswordMail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("UserMasterId", UserID);
            oJsonObject.Add("Password", DefaultPassword);
            oJsonObject.Add("EmailId", EmailId);
            oJsonObject.Add("OrganisationCode", OrganisationCode);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("UserId", userId);
            try
            {
                HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return true;

        }
        private bool SendRandomPassword_SMS(string UserID, string DefaultPassword, string MobileNumber, string OrganizationCode, string UserName, string userId, string cslEmpCode)
        {
            string url = _configuration[Configuration.NotificationApi];
            url += "/RandomPasswordSms";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("UserMasterId", UserID);
            oJsonObject.Add("Password", DefaultPassword);
            oJsonObject.Add("MobileNumber", MobileNumber);
            oJsonObject.Add("OrganizationCode", OrganizationCode);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("UserId", userId);
            oJsonObject.Add("CslEmpCode", cslEmpCode);
            try
            {
                HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return true;
        }

        [HttpGet("UpdateUserStatus/{UserId}")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> InActiveUser(int UserId)
        {

            try
            {
                int Result = await this._userRepository.InActiveUser(UserId);

                if (Result == -1)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotValid), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });

                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpPost("CapturePhotos")]
        public async Task<IActionResult> CapturePhotos([FromBody] APIPhotos aPIPhotos)
        {
            try
            {
                {
                    var base64string = aPIPhotos.Photo.Remove(0, 22);
                    var base64array = Convert.FromBase64String(base64string);
                    var filePath = Path.Combine($"C:/Photos/");
                    if (!System.IO.Directory.Exists(filePath))
                        System.IO.Directory.CreateDirectory(filePath);
                    filePath = filePath + Guid.NewGuid() + ".jpg";
                    System.IO.File.WriteAllBytes(filePath, base64array);
                }

                return this.Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.Ok();
            }

        }
        [HttpGet("SearchUserTypeAhead/{locationId}/{search?}")]

        public async Task<IActionResult> GetUsersByLocation(int locationId, string search = null)
        {

            try
            {
                return Ok(await this._userRepository.GetUsersByLocation(locationId, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("SearchApplicationTypeAhead/{search?}")]
        public async Task<IActionResult> GetSearchApplicationTypeAhead(string search = null)
        {
            try
            {
                return Ok(await this._userRepository.GetSearchApplicationTypeAhead(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetLeaderBoardDetails")]
        public async Task<IActionResult> GetLeaderBoardDetails()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string UserID = UserId.ToString();
                    var result = await this._rewardsPoint.GetLeaderBoardData(UserID);

                    if (result.SpecificUser.Count() == 0 && result.AllUsers.Count() == 0)
                        return this.NoContent();
                    else
                        return this.Ok(result);
                }
                catch (System.Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
            return this.BadRequest(this.ModelState);
        }


        [HttpGet("GetRegion")]
        public async Task<IActionResult> GetRegion()
        {
            try
            {
                return Ok(await this._userRepository.GetRegion());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetDepartment")]
        public async Task<IActionResult> GetDepartment()
        {
            try
            {
                return Ok(await this._userRepository.GetDepartment());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserName/{Id}")]
        public async Task<IActionResult> GetUserName(int Id)
        {
            try
            {
                return Ok(await this._userRepository.GetUserName(Id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        

        [HttpGet("GetBusinessDetails/{Userid}")]
        public async Task<IActionResult> GetBusinessDetails(int Userid)
        {
            try
            {
                List<APIGetBusinessDetails> aPIGetBusinessDetails = await this._userRepository.GetBusinessDetails(Userid);
                return this.Ok(aPIGetBusinessDetails);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostBusinessDetails")]
        public async Task<IActionResult> PostBusinessDetails([FromBody] APIBusinessDetails[] BusinessDetails)
        {
            try
            {
                return Ok(await this._userRepository.BusinessDetailsPost(BusinessDetails,UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("DeleteBusinessDetails")]
        public async Task<IActionResult> DeleteBusinessDetails([FromBody] APIBusinessDetails BusinessDetails)
        {
            try
            {
                return Ok(await this._userRepository.DeleteBusinessDetails(BusinessDetails));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetDecryptedValues")]
        public async Task<IActionResult> GetDecryptedValues([FromBody] DecryptedValues postModel)
        {
            try
            {
                return Ok(await this._userRepository.GetDecryptedValues(postModel));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #region "Teams"
        [HttpPost("TeamsCred")]
        public async Task<IActionResult> TeamsCred([FromBody]ApiTeamsCred apiTeamsCred)
        {
            try
            {
                if (apiTeamsCred.TeamsEmail == null && apiTeamsCred.Password == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData) });
                }
                apiTeamsCred.Password = Security.DecryptForUI(apiTeamsCred.Password);
                apiTeamsCred.Password = Security.Encrypt(apiTeamsCred.Password);
                return Ok(await this._userRepository.PostTeamsCred(apiTeamsCred, UserId));
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTeamsCred")]
        public async Task<IActionResult> GetTeamsCredential()
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._userRepository.GetTeamsCrediential(UserId);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetDefaultTeamsCred")]
        public async Task<IActionResult> GetDefaultTeamsCred()
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._userRepository.GetDefaultTeamsCrediential();
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("EditTeamsCred")]
        public async Task<IActionResult> EditTeamsCred([FromBody] ApiTeamsCred apiTeamsCred)
        {
            try
            {
                apiTeamsCred.Password = Security.DecryptForUI(apiTeamsCred.Password);
                apiTeamsCred.Password = Security.Encrypt(apiTeamsCred.Password);
                return Ok(await this._userRepository.EditTeamscred(apiTeamsCred, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("DeleteTeamsCred")]
        public async Task<IActionResult> DeleteTeamsCredential([FromBody] TeamsEmail teamsemail)
        {
            try
            {
                int Result = await this._userRepository.DeleteTeamsUser(teamsemail.Teamsemail);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok(1);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion
        #region "Zoom"
        [HttpPost("ZoomCred")]
        public async Task<IActionResult> ZoomCred([FromBody] ApiZoomCred apiZoomCred)
        {
            if (apiZoomCred == null)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidInfo), Description = EnumHelper.GetEnumDescription(MessageType.InvalidInfo) });
            }
            if (apiZoomCred.ZoomEmail == null)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidInfo), Description = EnumHelper.GetEnumDescription(MessageType.InvalidInfo) });
            }
            try
            {
                return Ok(await this._userRepository.PostZoomCred(apiZoomCred, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetZoomCred")]
        public async Task<IActionResult> GetZoomCredential()
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._userRepository.GetZoomCrediential(UserId);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetDefaultZoomCred")]
        public async Task<IActionResult> GetDefaultZoomCred()
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._userRepository.GetDefaultZoomCrediential();
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("EditZoomCred")]
        public async Task<IActionResult> EditZoomCred([FromBody] ApiZoomCred apiZoomCred)
        {
            try
            {
                apiZoomCred.ZoomEmail = Security.DecryptForUI(apiZoomCred.ZoomEmail);
                return Ok(await this._userRepository.EditZoomcred(apiZoomCred, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("DeleteZoomCred")]
        public async Task<IActionResult> DeleteZoomCredential([FromBody] ApiZoomCred apiZoomCred)
        {
            try
            {
                apiZoomCred.ZoomEmail = Security.DecryptForUI(apiZoomCred.ZoomEmail);
                int Result = await this._userRepository.DeleteZoomUser(apiZoomCred.ZoomEmail);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok(1);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        #endregion

        #region gsuit
        [HttpPost("GsuitCred")]
        public async Task<IActionResult> GsuitCred([FromBody] ApiGsuitCred apiGsuitCred)
        {
            if (apiGsuitCred == null)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidInfo), Description = EnumHelper.GetEnumDescription(MessageType.InvalidInfo) });
            }
            if (apiGsuitCred.Email == null)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidInfo), Description = EnumHelper.GetEnumDescription(MessageType.InvalidInfo) });
            }
            try
            {
                return Ok(await this._userRepository.PostGsuitCred(apiGsuitCred, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }        
       
        [HttpPost("EditGsuitCred")]
        public async Task<IActionResult> EditGsuitCred([FromBody] ApiGsuitCred apiGsuitCred)
        {
            try
            {
                apiGsuitCred.Email = Security.DecryptForUI(apiGsuitCred.Email);
                return Ok(await this._userRepository.EditGuitcred(apiGsuitCred, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("DeleteGsuitCred")]
        public async Task<IActionResult> DeleteGsuitCredential([FromBody] ApiGsuitCred apiGsuitCred)
        {
            try
            {
                apiGsuitCred.Email = Security.DecryptForUI(apiGsuitCred.Email);
                int Result = await this._userRepository.DeleteGsuitUser(apiGsuitCred.Email);
                if (Result == 0)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok(1);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetDefaultGsuitCred")]
        public async Task<IActionResult> GetDefaultGsuitCred()
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._userRepository.GetDefaultGSuitCredientials(UserId);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetGsuitCred")]
        public async Task<IActionResult> GetGsuitCredential()
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._userRepository.GetGsuitCrediential(UserId);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        #endregion
        [HttpPost("searchActiveInActiveUser")]
        public async Task<IActionResult> SearchAllUser([FromBody] ApiSearchUser searchUser)
        {
            try
            {
                if (searchUser.UserId != null)
                    searchUser.UserId = Security.DecryptForUI(searchUser.UserId);
                if (searchUser.UserType != null)
                    searchUser.UserType = Security.DecryptForUI(searchUser.UserType);

                searchUser.UserType = searchUser.UserType.ToLower().Equals("null") ? null : searchUser.UserType;


                return this.Ok(await this._userRepository.SearchAllUser(searchUser.UserId, searchUser.UserType));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
