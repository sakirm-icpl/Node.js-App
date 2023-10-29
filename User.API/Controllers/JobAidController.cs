//======================================
// <copyright file="JobAidController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================


using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Common;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.AuthorizePermissions;
using static User.API.Common.TokenPermissions;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using User.API.Helper.Log_API_Count;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired(Permissions.JobRoleResponsbilities)]
    public class JobAidController : IdentityController
    {
        IUserRepository _userRepository;
        IAzureStorage _azurestorage;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobAidController));
        private IJobAidRepository jobAidRepository;
        IUserTeamsRepository _userTeamsRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRewardsPoint _rewardsPoint;
        private readonly ITokensRepository _tokensRepository;

        public JobAidController(IJobAidRepository jobAidController,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IRewardsPoint rewardsPoint,
            IConfiguration confugu,
            IUserRepository userRepository,
            IIdentityService identityService,
            IAzureStorage azurestorage,
            IUserTeamsRepository userteamsrepository,
            ITokensRepository tokensRepository) : base(identityService)
        {
            this._userRepository = userRepository;
            this._userTeamsRepository = userteamsrepository;
            this._azurestorage = azurestorage;
            this.jobAidRepository = jobAidController;
            this.hostingEnvironment = environment;
            this._configuration = confugu;
            this._httpContextAccessor = httpContextAccessor;
            this._rewardsPoint = rewardsPoint;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                IEnumerable<APIJobAid> jobAid = await this.jobAidRepository.GetAllRecordJobAid();
                return Ok(jobAid);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                IEnumerable<APIJobAid> jobAid = await this.jobAidRepository.GetAllJobAid(page, pageSize, search, columnName);
                return Ok(jobAid);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this.jobAidRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Exist/{search}")]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> Exist(string search)
        {
            try
            {
                if (await this.jobAidRepository.Exist(search))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                IEnumerable<APIJobAid> jobAid = await this.jobAidRepository.GetJobAid(id);
                return Ok(jobAid);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> Post([FromBody] APIJobAid aPIJobAid)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    JobAid jobAid = new JobAid();
                    if (await this.jobAidRepository.Exist(aPIJobAid.Title))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    jobAid.ContentId = aPIJobAid.CustomerCode + "/" + Convert.ToString(await this.jobAidRepository.GetTotalJobAidCount() + 1);
                    jobAid.Title = aPIJobAid.Title;
                    jobAid.FileType = aPIJobAid.FileType;
                    jobAid.AdditionalDescription = aPIJobAid.AdditionalDescription;
                    jobAid.Content = aPIJobAid.Content;
                    jobAid.KeywordForSearch = aPIJobAid.KeywordForSearch;
                    jobAid.CreatedBy = UserId;
                    jobAid.CreatedDate = DateTime.UtcNow;
                    jobAid.ModifiedBy = UserId;
                    jobAid.ModifiedDate = DateTime.UtcNow;

                    await this.jobAidRepository.Add(jobAid);
                    return this.Ok(jobAid);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

            }
        }

        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.FileType : request.Form[Record.FileType].ToString();

                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }

                        if (FileValidation.IsValidImageVideoPdf(fileUpload))
                        {

                            var EnableBlobStorage = await _userRepository.GetConfigurationValueAsync("Enable_BlobStorage", OrgCode);

                           if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                           {
                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrgCode);
                                fileDir = Path.Combine(fileDir, fileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
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
                            else
                            {
                                try
                                {


                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        // PUT api/values/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> Put(int id, [FromBody] APIJobAid aPIJobAid)
        {
            try
            {
                JobAid jobAid = await this.jobAidRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == id);
                if (ModelState.IsValid && jobAid != null)
                {

                    if (jobAid.Title.ToString() != aPIJobAid.Title)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    }

                    string path = jobAid.Content.ToString();
                    jobAid.Title = aPIJobAid.Title;
                    jobAid.FileType = aPIJobAid.FileType;
                    jobAid.AdditionalDescription = aPIJobAid.AdditionalDescription;
                    jobAid.Content = aPIJobAid.Content;
                    jobAid.KeywordForSearch = aPIJobAid.KeywordForSearch;
                    jobAid.ModifiedBy = UserId;
                    jobAid.ModifiedDate = DateTime.UtcNow;
                    await this.jobAidRepository.Update(jobAid);

                    //Image File Delete

                    if (aPIJobAid.Content.ToString() == null || String.IsNullOrEmpty(aPIJobAid.Content))
                    {
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        string sWebRootFolder = this._configuration["ApiGatewayLXPFiles"];
                        sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
                        string[] pathRemove = path.Split("/");
                        string filename = pathRemove[3];
                        string remainpath;
                        remainpath = @"\" + pathRemove[1] + @"\" + pathRemove[2] + @"\";
                        sb = sb.Append(sWebRootFolder);
                        sb = sb.Append(remainpath);
                        string finalpath = sb.ToString();
                        FileInfo file = new FileInfo(Path.Combine(finalpath, filename));
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                    return this.Ok(jobAid);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                JobAid jobAid = await this.jobAidRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == DecryptedId);
                if (jobAid == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                jobAid.IsDeleted = Record.Deleted;
                await this.jobAidRepository.Update(jobAid);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Search/{q}")]
        [PermissionRequired(Permissions.JobAidRoleGuide)]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<JobAid> jobAid = await this.jobAidRepository.Search(q);
                return this.Ok(jobAid);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Read/{jobAidId}")]
        public async Task<IActionResult> ReadJobAid(int id)
        {
            try
            {
                await this._rewardsPoint.JobAidReadRewardPoint(UserId, id);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
