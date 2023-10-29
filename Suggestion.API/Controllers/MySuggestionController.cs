// ======================================
// <copyright file="MySuggestionController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Suggestion.API.APIModel;
using Suggestion.API.Common;
using Suggestion.API.Helper;
using Suggestion.API.Metadata;
using Suggestion.API.Models;
using Suggestion.API.Repositories.Interfaces;
using Suggestion.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Suggestion.API.Common.AuthorizePermissions;
using static Suggestion.API.Common.TokenPermissions;
using log4net;
using Suggestion.API.Helper.Log_API_Count;

namespace Suggestion.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class MySuggestionController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MySuggestionController));
        private IMySuggestionRepository mySuggestionRepository;
        private IMySuggestionDetailRepository mySuggestionDetailRepository;
        private IRewardsPointRepository _rewardsPointRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        public MySuggestionController(IMySuggestionRepository mySuggestionController, IMySuggestionDetailRepository mySuggestionDetailController, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, IConfiguration confugu, IIdentityService identitySvc, IRewardsPointRepository rewardsPointRepository, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.mySuggestionRepository = mySuggestionController;
            this.mySuggestionDetailRepository = mySuggestionDetailController;
            this.hostingEnvironment = environment;
            this._configuration = confugu;
            this._httpContextAccessor = httpContextAccessor;
            this._identitySvc = identitySvc;
            this._rewardsPointRepository = rewardsPointRepository;
            this._tokensRepository = tokensRepository;
        }

        // GET: api/<controller>
        [HttpGet]
        // [PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<MySuggestion> mysuggestions = await this.mySuggestionRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIMySuggestion>>(mysuggestions));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        //[PermissionRequired(Permissions.MySuggestion)] This method is called by EU, So commented
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<APIMySuggestion> mysuggestions = await this.mySuggestionRepository.GetAllSuggestions(UserId, page, pageSize, search);
                return Ok(mysuggestions);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        //[PermissionRequired(Permissions.MySuggestion)] This method is called by EU, So commented
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int mysuggestions = await this.mySuggestionRepository.Count(UserId, search);
                return Ok(mysuggestions);
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // GET api/<controller>/5
        // GET api/<controller>/5
        [HttpGet("{id}")]
        [PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                MySuggestion mysuggestions = await this.mySuggestionRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APIMySuggestion>(mysuggestions));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        // POST api/values
        [HttpPost]
        //[PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Post([FromBody]APIMySuggestionMerge aPIMySuggestionMerge)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    MySuggestion mySuggestion = new MySuggestion
                    {
                        Date = DateTime.UtcNow,
                        SuggestionBrief = aPIMySuggestionMerge.SuggestionBrief,
                        ContextualAreaofBusiness = aPIMySuggestionMerge.ContextualAreaofBusiness,
                        DetailedDescription = aPIMySuggestionMerge.DetailedDescription,
                        Status = "Registered",
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await this.mySuggestionRepository.Add(mySuggestion);

                    if (await this.mySuggestionRepository.Exists(UserId, mySuggestion.CreatedDate))
                    {
                        await this._rewardsPointRepository.MySuggestionSubmitRewardPoint(UserId, mySuggestion.Id,mySuggestion.ContextualAreaofBusiness,OrgCode);

                    }

                    List<MySuggestionDetail> mySuggestionDetails = new List<MySuggestionDetail>();
                    foreach (APIMySuggestionDetail opt in aPIMySuggestionMerge.aPIMySuggestionDetail)
                    {
                        MySuggestionDetail mySuggestionDetail = new MySuggestionDetail
                        {
                            SuggestionId = mySuggestion.Id,
                            FilePath = opt.FilePath,
                            FileType=opt.FileType,
                            ContentDescription = opt.ContentDescription,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow
                        };
                        mySuggestionDetails.Add(mySuggestionDetail);
                    }

                    await mySuggestionDetailRepository.AddRange(mySuggestionDetails);

                    return Ok();

                }
                return this.BadRequest(this.ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        

        [HttpPost("PostSuggestion")]
        public async Task<IActionResult> PostSuggestion([FromBody]APIMySuggestionMerge aPIMySuggestionMerge)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    MySuggestion mySuggestion = new MySuggestion
                    {
                        Date = DateTime.UtcNow,
                        SuggestionBrief = aPIMySuggestionMerge.SuggestionBrief,
                        ContextualAreaofBusiness = aPIMySuggestionMerge.ContextualAreaofBusiness,
                        DetailedDescription = aPIMySuggestionMerge.DetailedDescription,
                        Status = aPIMySuggestionMerge.Status,
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await this.mySuggestionRepository.Add(mySuggestion);

                    if (await this.mySuggestionRepository.Exists(UserId, mySuggestion.CreatedDate))
                    {
                        await this._rewardsPointRepository.MySuggestionSubmitRewardPoint(UserId, mySuggestion.Id, mySuggestion.ContextualAreaofBusiness, OrgCode);

                    }

                    List<MySuggestionDetail> mySuggestionDetails = new List<MySuggestionDetail>();
                    foreach (APIMySuggestionDetail opt in aPIMySuggestionMerge.aPIMySuggestionDetail)
                    {
                        MySuggestionDetail mySuggestionDetail = new MySuggestionDetail
                        {
                            SuggestionId = mySuggestion.Id,
                            FilePath = opt.FilePath,
                            FileType=opt.FileType,
                            ContentDescription = opt.ContentDescription,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow
                        };
                        mySuggestionDetails.Add(mySuggestionDetail);
                    }
                    await mySuggestionDetailRepository.AddRange(mySuggestionDetails);

                   // return Ok(aPIMySuggestionMerge);
                    return Ok(aPIMySuggestionMerge);
                }
                return this.BadRequest(this.ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostFileUpload")]
        //[PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {

                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                string areaofBuisiness = string.IsNullOrEmpty(request.Form[Record.ContextualAreaofBusiness]) ? "4.5" : request.Form[Record.ContextualAreaofBusiness].ToString();
                string filetype = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.Pdf : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }

                        APIMySuggestionDetails mySuggestionDetail = new APIMySuggestionDetails();

                        if (FileValidation.IsValidImageVideoPdf(fileUpload))
                        {
                            string fileDir = this._configuration["ApiGatewayLXPFiles"];
                            fileDir = Path.Combine(fileDir, OrgCode, filetype);
                            //fileDir = Path.Combine(fileDir, areaofBuisiness, filetype);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + filetype);
                            using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            if ((fileUpload.ContentType.Contains(FileType.Video)) && (FileValidation.IsValidVideo(fileUpload)))
                            {
                                mySuggestionDetail.FileType = FileType.Video;
                                mySuggestionDetail.FilePath = await this.mySuggestionRepository.SaveFile(fileUpload, FileType.Video, OrgCode);
                                return Ok(mySuggestionDetail);
                            }

                            else if ((fileUpload.ContentType.Contains(FileType.Audio)) && (FileValidation.IsValidVideo(fileUpload)))
                            {
                                mySuggestionDetail.FileType = FileType.Audio;
                                mySuggestionDetail.FilePath = await this.mySuggestionRepository.SaveFile(fileUpload, FileType.Audio, OrgCode);
                                return Ok(mySuggestionDetail);
                            }
                            else if ((fileUpload.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(fileUpload)))
                            {
                                mySuggestionDetail.FileType = FileType.Image;
                          mySuggestionDetail.FilePath = await this.mySuggestionRepository.SaveFile(fileUpload, FileType.Audio, OrgCode);
                                return Ok(mySuggestionDetail);
                            }
                            else
                            {
                                foreach (string docType in FileType.Doc)
                                {
                                    if ((fileUpload.ContentType.Contains(docType)) && (FileValidation.IsValidLCMSDocument(fileUpload)))
                                    {
                                        mySuggestionDetail.FileType = docType;
                                        mySuggestionDetail.FilePath = await this.mySuggestionRepository.SaveFile(fileUpload, docType, OrgCode);
                                        return Ok(mySuggestionDetail);
                                    }
                                }
                            }
                        }
                        else
                        
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        
                    }

                }
                else
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // PUT api/<controller>/5
        //[HttpPut("{id}")]
        //[PermissionRequired(Permissions.MySuggestion)]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        [HttpPost("UpdateSuggestionDetail")]
        //[PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> UpdateSuggestions([FromBody]APIUpdateSuggestions aPIMySuggestion)
        {
            try
            {
                MySuggestion mySuggestion = await this.mySuggestionRepository.GetSuggestionDetails(Convert.ToInt32(aPIMySuggestion.Id));
                mySuggestion.ModifiedBy = UserId;
                if (aPIMySuggestion.ApprovalStatus == Record.Approved)
                    mySuggestion.Status = Record.Approved;
                else if (aPIMySuggestion.ApprovalStatus == Record.Rejected)
                    mySuggestion.Status = Record.Rejected;
                else if (aPIMySuggestion.ApprovalStatus == "" || aPIMySuggestion.ApprovalStatus == null)
                    mySuggestion.Status = "";
                await this.mySuggestionRepository.Update(mySuggestion);

                MySuggestionDetail Suggestion = await this.mySuggestionRepository.GetSuggestion(Convert.ToInt32(aPIMySuggestion.Id));

                Suggestion.FilePath = aPIMySuggestion.FilePath;
                Suggestion.FileType = aPIMySuggestion.FileType;

                await this.mySuggestionDetailRepository.Update(Suggestion);
                if (mySuggestion.Status == "Approved")
                {
                    await this._rewardsPointRepository.MySuggestionRewardPoint(UserId, Convert.ToInt32(aPIMySuggestion.Id), mySuggestion.DetailedDescription, OrgCode);
                }
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.StatusChanged), Description = EnumHelper.GetEnumDescription(MessageType.StatusChanged), StatusCode = 200 });
                //return Ok("status changed successfully");
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                MySuggestion mySuggestion = await this.mySuggestionRepository.Get(DecryptedId);

                if (ModelState.IsValid && mySuggestion != null)
                {
                    mySuggestion.IsDeleted = true;
                    await this.mySuggestionRepository.Update(mySuggestion);
                }

                if (mySuggestion == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        /// <summary>
        /// Search specific MediaLibrary.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<MySuggestion> mySuggestion = await this.mySuggestionRepository.Search(q);
                return Ok(Mapper.Map<List<APIMySuggestion>>(mySuggestion));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
