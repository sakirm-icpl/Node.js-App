// ======================================
// <copyright file="PollsManagementController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using Suggestion.API.Common;
using Suggestion.API.Helper;
using Suggestion.API.Models;
using Suggestion.API.Repositories.Interfaces;
using Suggestion.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Suggestion.API.Common.AuthorizePermissions;
using static Suggestion.API.Common.TokenPermissions;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using Suggestion.API.Data;
using Suggestion.API.APIModel;
using AutoMapper;
using Suggestion.API.Helper.Log_API_Count;

namespace Suggestion.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/s/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    [TokenRequired()]
    public class EmployeeSuggestionsController : IdentityController
    {
        IAzureStorage _azurestorage;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmployeeSuggestionsController));
        IEmployeeSuggestions _employeeSuggestionsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokensRepository _tokensRepository;
        private readonly ISuggestionCategories _suggestionCategories;
        private readonly IAwardList _awardList;
        private readonly IEmployeeAwards _employeeAwards;

        public IConfiguration _configuration { get; }
        public EmployeeSuggestionsController(IIdentityService identitySvc,
            IHttpContextAccessor httpContextAccessor,
            IEmployeeSuggestions employeeSuggestionsRepository,
            ITokensRepository tokensRepository,
            IAzureStorage azurestorage,
            IConfiguration confugu,
            ISuggestionCategories suggestionCategories,
            IAwardList awardList,
            IEmployeeAwards employeeAwards
            ) : base(identitySvc)
        {
            this._employeeSuggestionsRepository = employeeSuggestionsRepository;
            this._httpContextAccessor = httpContextAccessor;
            this._tokensRepository = tokensRepository;
            this._azurestorage = azurestorage;
            this._configuration = confugu;
            this._suggestionCategories = suggestionCategories;
            this._awardList = awardList;
            this._employeeAwards = employeeAwards;
        }

        [HttpPost]
        public async Task<IActionResult> PostEmployeeSuggestions([FromBody] APIEmployeeSuggestions data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    EmployeeSuggestions employeeSuggestions = Mapper.Map<EmployeeSuggestions>(data);
                    employeeSuggestions.EmployeeId = UserId;
                    employeeSuggestions.ModifiedBy = UserId;
                    employeeSuggestions.CreatedBy = UserId;
                    employeeSuggestions.IsActive = true;
                    employeeSuggestions.IsDeleted = false;
                    employeeSuggestions.CreatedDate = DateTime.UtcNow;
                    employeeSuggestions.ModifiedDate = DateTime.UtcNow;
                    await _employeeSuggestionsRepository.Add(employeeSuggestions);
                    return Ok();
                }
                else
                {
                    return this.BadRequest(ModelState);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{searchBy?}/{search?}")]
        public async Task<IActionResult> GetEmployeeSuggestions(int page, int pageSize, string searchBy = null, string search = null)
        {
            try
            {
                return Ok(await this._employeeSuggestionsRepository.GetEmployeeSuggestions(page, pageSize, UserId, searchBy, search));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("LikeList/{suggestionId?}/{like?}")]
        public async Task<IActionResult> GetLikeList(int suggestionId, string like)
        {
            try
            {
                return Ok(await this._employeeSuggestionsRepository.GetLikeList(suggestionId, like));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Top5")]
        public async Task<IActionResult> GetEmployeeSuggestionsTop5([FromBody] APIFilter data)
        {
            try
            {
                return Ok(await this._employeeSuggestionsRepository.GetEmployeeSuggestionsTop5(data));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ForUser/{page:int}/{pageSize:int}")]
        public async Task<IActionResult> GetEmployeeSuggestionsForUser(int page, int pageSize)
        {
            try
            {
                return Ok(await this._employeeSuggestionsRepository.GetEmployeeSuggestionsForUser(page, pageSize, UserId));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Area")]
        public async Task<IActionResult> GetArea()
        {
            try
            {
                return Ok(await this._employeeSuggestionsRepository.GetArea());
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Cluster")]
        public async Task<IActionResult> GetCluster()
        {
            try
            {
                return Ok(await this._employeeSuggestionsRepository.GetCluster());
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] EmployeeSuggestionLike data)
        {
            try
            {
                EmployeeSuggestions employeeSuggestionsData = await _employeeSuggestionsRepository.Get(id);
                if (employeeSuggestionsData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                EmployeeSuggestionLike RemarkData = await _employeeSuggestionsRepository.GetRemarkData(id, UserId);

                if (RemarkData == null)
                {
                    return Ok(await _employeeSuggestionsRepository.PostEmployeeSuggestionLike(id, data, UserId));
                }
                else
                {
                    return Ok(await _employeeSuggestionsRepository.UpdateEmployeeSuggestionLike(id, RemarkData, data, UserId));

                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("UserUpdate/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APIEmployeeSuggestions data)
        {
            try
            {
                EmployeeSuggestions EmployeeSuggestionsData = await _employeeSuggestionsRepository.Get(id);
                if (EmployeeSuggestionsData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                EmployeeSuggestionsData.ModifiedBy = UserId;
                EmployeeSuggestionsData.ModifiedDate = DateTime.Now;
                EmployeeSuggestionsData.Suggestion = data.Suggestion;
                EmployeeSuggestionsData.AdditionalDescription = data.AdditionalDescription;
                EmployeeSuggestionsData.IsActive = data.IsActive;
                EmployeeSuggestionsData.Category = data.Category;
                await _employeeSuggestionsRepository.Update(EmployeeSuggestionsData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.EmployeeSuggestions)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {

                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.MP4 : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }


                        // allowed .pdf,video,images


                        if (FileValidation.IsValidImageVideoPdf(fileUpload))
                        {
                            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrgCode, FileType);
                                // fileDir = Path.Combine(fileDir, , FileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
                                using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
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
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode, FileType);

                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            string file = res.Blob.Name.ToString();

                                            return Ok("/" + file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }
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
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("SaveMedia")]
        [PermissionRequired(Permissions.EmployeeSuggestions)]
        public async Task<IActionResult> PostSaveMedia([FromBody] EmployeeSuggestionFileV2 employeeSuggestionFile)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    foreach (APIEmployeeSuggestionMerge opt in employeeSuggestionFile.aPIEmployeeSuggestionMerge)
                    {
                        EmployeeSuggestionFile es = new EmployeeSuggestionFile();
                        es.IsDeleted = false;
                        es.IsActive = true;
                        es.ModifiedBy = UserId;
                        es.ModifiedDate = DateTime.UtcNow;
                        es.CreatedBy = UserId;
                        es.CreatedDate = DateTime.UtcNow;
                        es.SuggestionId = employeeSuggestionFile.Id;
                        es.FileName = opt.FileName;
                        es.FileType = opt.FileType;
                        es.FilePath = opt.FilePath;
                        _employeeSuggestionsRepository.savefile(es);
                    }
                    return Ok();
                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetAttachedFiles(int Id)
        {
            try
            {
                return Ok(await this._employeeSuggestionsRepository.GetAttachedFiles(Id));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id, [FromBody] APIEmployeeSuggestions data)
        {
            try
            {
                EmployeeSuggestions employeeSuggestionsData = await _employeeSuggestionsRepository.Get(id);
                List<EmployeeSuggestionLike> employeeSuggestionsLikeData = _employeeSuggestionsRepository.GetLikeData(id);
                List<EmployeeSuggestionFile> employeeSuggestionsFileData = _employeeSuggestionsRepository.GetFileData(id);
                if (employeeSuggestionsData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                foreach (EmployeeSuggestionFile file in employeeSuggestionsFileData)
                {
                    file.ModifiedBy = UserId;
                    file.ModifiedDate = DateTime.Now;
                    file.IsActive = data.IsActive;
                    file.IsDeleted = true;
                    _employeeSuggestionsRepository.UpdateDeleteInFileTable(file);
                }

                foreach (EmployeeSuggestionLike like in employeeSuggestionsLikeData)
                {
                    like.ModifiedBy = data.ModifiedBy;
                    like.ModifiedDate = DateTime.Now;
                    like.IsActive = data.IsActive;
                    like.IsDeleted = true;
                    _employeeSuggestionsRepository.UpdateDeleteInLikeTable(like);
                }

                employeeSuggestionsData.ModifiedBy = data.ModifiedBy;
                employeeSuggestionsData.ModifiedDate = DateTime.Now;
                employeeSuggestionsData.IsActive = data.IsActive;
                employeeSuggestionsData.IsDeleted = true;
                await _employeeSuggestionsRepository.Update(employeeSuggestionsData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("count")]
        public async Task<List<getfilecount>> GetFileCount()
        {
            try
            {
                List<getfilecount> count = await _employeeSuggestionsRepository.GetFileCount();
                return count;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("Category")]
        public async Task<IActionResult> PostSuggestionCategories([FromBody] APISuggestionCategory data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SuggestionCategory SuggestionCategory = Mapper.Map<SuggestionCategory>(data);
                    SuggestionCategory.ModifiedBy = UserId;
                    SuggestionCategory.CreatedBy = UserId;
                    SuggestionCategory.IsActive = true;
                    SuggestionCategory.IsDeleted = false;
                    SuggestionCategory.CreatedDate = DateTime.UtcNow;
                    SuggestionCategory.ModifiedDate = DateTime.UtcNow;
                    await _suggestionCategories.Add(SuggestionCategory);
                    return Ok();
                }
                else
                {
                    return this.BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Category/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetSuggestionCategories(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._suggestionCategories.GetSuggestionCategories(page, pageSize, search));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Category/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APISuggestionCategory data)
        {
            try
            {
                SuggestionCategory suggestionCategoryData = await _suggestionCategories.Get(id);
                if (suggestionCategoryData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                suggestionCategoryData.ModifiedBy = UserId;
                suggestionCategoryData.ModifiedDate = DateTime.Now;
                suggestionCategoryData.SuggestionsCategory = data.SuggestionsCategory;
                suggestionCategoryData.Code = data.Code;
                suggestionCategoryData.IsActive = true;
                await _suggestionCategories.Update(suggestionCategoryData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("Category/Delete/{id}")]
        public async Task<IActionResult> Delete(int id, [FromBody] APISuggestionCategory data)
        {
            try
            {
                SuggestionCategory suggestionCategoryData = await _suggestionCategories.Get(id);
                if (suggestionCategoryData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                suggestionCategoryData.ModifiedBy = UserId;
                suggestionCategoryData.ModifiedDate = DateTime.Now;
                suggestionCategoryData.IsActive = false;
                suggestionCategoryData.IsDeleted = true;
                await _suggestionCategories.Update(suggestionCategoryData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Awardlist")]
        public async Task<IActionResult> PostAwardList([FromBody] APIAwardList data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AwardList awardList = Mapper.Map<AwardList>(data);
                    awardList.ModifiedBy = UserId;
                    awardList.CreatedBy = UserId;
                    awardList.IsActive = true;
                    awardList.IsDeleted = false;
                    awardList.CreatedDate = DateTime.UtcNow;
                    awardList.ModifiedDate = DateTime.UtcNow;
                    await _awardList.Add(awardList);
                    return Ok();
                }
                else
                {
                    return this.BadRequest(ModelState);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("AwardList/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetAwardList(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._awardList.GetAwardList(page, pageSize, search));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AwardDashBoard")]
        public async Task<IActionResult> BestAwardDashboard([FromBody] APIFilter data)
        {
            try
            {
                return Ok(await this._employeeAwards.BestAwardDashboard(data));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AwardList/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APIAwardList data)
        {
            try
            {
                AwardList awardListData = await _awardList.Get(id);
                if (awardListData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                awardListData.ModifiedBy = UserId;
                awardListData.ModifiedDate = DateTime.Now;
                awardListData.Title = data.Title;
                awardListData.IsActive = data.IsActive;
                awardListData.FilePath = data.FilePath;
                await _awardList.Update(awardListData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("AwardListDelete/{id}")]
        public async Task<IActionResult> Delete(int id, [FromBody] AwardList data)
        {
            try
            {
                AwardList awardListData = await _awardList.Get(id);
                if (awardListData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                awardListData.ModifiedBy = UserId;
                awardListData.ModifiedDate = DateTime.Now;
                awardListData.IsActive = false;
                awardListData.IsDeleted = true;
                await _awardList.Update(awardListData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("EmployeeAward")]
        public async Task<IActionResult> PostEmployeeAwards([FromBody] APIEmployeeAwards data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string flag = _employeeAwards.CheckAwardExist(data);
                    if (flag == "Yes")
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAward), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAward) });
                    }

                    EmployeeAwards employeeAward = Mapper.Map<EmployeeAwards>(data);
                    employeeAward.ModifiedBy = UserId;
                    employeeAward.CreatedBy = UserId;
                    employeeAward.IsActive = true;
                    employeeAward.IsDeleted = false;
                    employeeAward.CreatedDate = DateTime.UtcNow;
                    employeeAward.ModifiedDate = DateTime.UtcNow;
                    await _employeeAwards.Add(employeeAward);
                    return Ok();
                }
                else
                {
                    return this.BadRequest(ModelState);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("EmployeeAward/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetEmployeeAwards(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._employeeAwards.GetEmployeeAwards(page, pageSize, search));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("EmployeeAward")]
        public async Task<IActionResult> GetEmployeeAwardsByUserId()
        {
            try
            {
                return Ok(await this._employeeAwards.GetEmployeeAwardsByUserId(UserId));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("EmployeeAward/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APIEmployeeAwards data)
        {
            try
            {
                EmployeeAwards employeeAwardsData = await _employeeAwards.Get(id);
                if (employeeAwardsData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                employeeAwardsData.ModifiedBy = UserId;
                employeeAwardsData.ModifiedDate = DateTime.Now;
                employeeAwardsData.EmployeeId = data.EmployeeId;
                employeeAwardsData.AwardId = data.AwardId;
                employeeAwardsData.Remarks = data.Remarks;
                employeeAwardsData.Month = data.Month;
                employeeAwardsData.Year = data.Year;

                await _employeeAwards.Update(employeeAwardsData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }

}




