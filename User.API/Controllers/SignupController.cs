using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Common;
using User.API.Helper;
using User.API.Helper.Log_API_Count;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.AuthorizePermissions;
using static User.API.Models.UserMaster;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/Signup")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    public class SignupController : Controller
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SignupController));
        private ISignUpRepository _signUpRepository;
        private IUserRepository _userRepository;
        private IConfiguration _configuration;
        private string url;
        private string supportMail;
        private string aPPurl;
        private string reagardName;
        private readonly IIdentityService _identitySvc;

        public SignupController(ISignUpRepository signUpRepository, IUserRepository userRepository, IIdentityService identityService, IConfiguration configuration)
        {
            this._signUpRepository = signUpRepository;
            this._userRepository = userRepository;
            this._configuration = configuration;
            this._identitySvc = identityService;
        }
        [HttpGet("{id}")]
        
        [PermissionRequired(Permissions.useractivation)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                Signup user = await this._signUpRepository.Get(id);
                return this.Ok(user);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.useractivation)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<Signup> users = await this._signUpRepository.GetAllUser(page, pageSize, search);
                return this.Ok(users);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.useractivation)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var count = await this._signUpRepository.GetUserCount(search);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpDelete]
        [PermissionRequired(Permissions.useractivation)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Signup user = await this._signUpRepository.Get(u => u.IsDeleted == Record.NotDeleted && u.Id == DecryptedId);
                if (user == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                }
                user.Accept = Record.Rejected;
                await this._signUpRepository.Update(user);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [PermissionRequired(Permissions.useractivation)]
        public async Task<IActionResult> Post([FromBody] Signup apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Exists exist = apiUser.Validations(this._signUpRepository, apiUser);
                    if (!exist.Equals(Exists.No))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(exist) });
                    else
                    {
                        apiUser.UserRole = "EndUser";
                        apiUser.CreatedDate = DateTime.UtcNow;
                        await this._signUpRepository.Add(apiUser);

                        if (!string.IsNullOrEmpty(apiUser.Accept) && apiUser.Accept.ToLower().Equals("accept"))
                        {
                            APIUserMaster user = apiUser.MapSignupToAPIUser(apiUser);
                            user.CustomerCode = await this._userRepository.GetCustomerCodeByEmailId(apiUser.ActivationSentTo);
                            user.Id = 0;
                            user.IsActive = true;
                            await this._userRepository.AddUser(user, "EU", user.CustomerCode, "false"); ///EU Not Implement Identity for this Controller
                        }
                        return Ok(apiUser);
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.useractivation)]
        [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody] Signup apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Signup oldUser = await this._signUpRepository.Get(u => u.IsDeleted == Record.NotDeleted && u.Id == id);
                    if (oldUser == null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                    }

                    if (oldUser.MobileNumber != apiUser.MobileNumber || oldUser.EmailId != apiUser.EmailId || oldUser.UserId != apiUser.UserId)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    if (apiUser.IsUniqueDataIsChanged(oldUser, apiUser))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.UniqueChanged), Description = EnumHelper.GetEnumDescription(MessageType.UniqueChanged) });
                    apiUser.Id = id;
                    apiUser.ModifiedBy = id;
                    apiUser.ModifiedDate = DateTime.UtcNow;
                    await this._signUpRepository.Update(apiUser);

                    if (oldUser.Accept.ToLower() == "reject" && apiUser.Accept.ToLower() == "accept")
                    {
                        APIUserMaster obj = new APIUserMaster();
                        obj = Mapper.Map<APIUserMaster>(apiUser);
                        obj.ReportsTo = apiUser.ActivationSentTo;

                        string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                        if (identity == null)
                        {
                            return StatusCode(401, Record.InvalidUserID);
                        }

                        int UserId = Convert.ToInt32(identity);

                        obj = await _userRepository.GenerateCustomerCode(obj, UserId);
                        obj.IsActive = true;
                        obj.IsDeleted = false;
                        await _userRepository.AddUser(obj, "EU", obj.CustomerCode, "false");
                    }

                    return this.Ok(apiUser);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Exist/{search:minlength(0)?}")]
        public async Task<IActionResult> Exist(string search)
        {
            try
            {
                if (this._signUpRepository.UserNameExists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.UserNameExist) });
                if (this._signUpRepository.EmailExists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.EmailIdExist) });
                if (this._signUpRepository.MobileExists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.MobileExist) });
                if (this._signUpRepository.Exists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.UserIdExist) });

                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("EmailExist/{search:minlength(0)?}")]
        public async Task<IActionResult> EmailExist(string search)
        {
            try
            {
                if (await this._userRepository.EmailExists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.ActivationSentToExist) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(Exists.ActivationSentToNotExist) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("SignupEmailExist/{search:minlength(0)?}")]
        public async Task<IActionResult> SignupEmailExist(string search)
        {
            try
            {
                if (this._signUpRepository.EmailExists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.EmailIdExist) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("MobileExists/{search:minlength(0)?}")]
        public async Task<IActionResult> MobileExists(string search)
        {
            try
            {
                if (this._signUpRepository.MobileExists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.MobileExist) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("UserIdExists/{search:minlength(0)?}")]
        public async Task<IActionResult> UserIdExists(string search)
        {
            try
            {
                if (this._signUpRepository.Exists(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(Exists.UserIdExist) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetOrganizations/{search?}")]
        public async Task<IActionResult> GetOrganizations(string search)
        {
            try
            {
                var Result = await this._signUpRepository.GetOrganization(search);
                return Ok(Result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
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
    }
}