// ======================================
// <copyright file="PollsManagementController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Gadget.API.APIModel;
using Gadget.API.Helper;
using Gadget.API.Helper.Log_API_Count;
using Gadget.API.Metadata;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.IO;
using System.Threading.Tasks;
using static Gadget.API.Common.TokenPermissions;

namespace Gadget.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/g/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    [TokenRequired()]
    public class DigitalAdoptionReviewController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DigitalAdoptionReviewController));
        private readonly IDigitalAdoptionReviewList _digitalAdoptionReviewListRepository;
        private readonly IUseCasesList _useCasesListRepository;
        private readonly IDigitalRolesList _digitalRolesList;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration { get; }
        public DigitalAdoptionReviewController(IConfiguration configure, IHttpContextAccessor httpContextAccessor, IIdentityService identitySvc, IDigitalAdoptionReviewList digitalAdoptionReviewListRepository, IUseCasesList useCasesListRepository, IDigitalRolesList digitalRolesList) : base(identitySvc)
        {
            this._digitalAdoptionReviewListRepository = digitalAdoptionReviewListRepository;
            _useCasesListRepository = useCasesListRepository;
            _digitalRolesList = digitalRolesList;
            _configuration = configure;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> PostDigitalAdoptionReviewList([FromBody] APIDigitalAdoptionReview data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (data.EmployeeId != UserId)
                    {
                        DigitalAdoptionReview digitalAdoptionReview = Mapper.Map<DigitalAdoptionReview>(data);
                        digitalAdoptionReview.ModifiedBy = UserId;
                        digitalAdoptionReview.CreatedBy = UserId;
                        digitalAdoptionReview.IsActive = true;
                        digitalAdoptionReview.IsDeleted = false;
                        digitalAdoptionReview.CreatedDate = DateTime.UtcNow;
                        digitalAdoptionReview.ModifiedDate = DateTime.UtcNow;
                        await _digitalAdoptionReviewListRepository.Add(digitalAdoptionReview);
                        return Ok();
                    }
                    else
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.ReviewYourself), Description = EnumHelper.GetEnumDescription(MessageType.ReviewYourself) });
                    }
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

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetDigitalAdoptionReview(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._digitalAdoptionReviewListRepository.GetDigitalAdoptionReview(page, pageSize, search));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("DigiAdopDashboard")]
        public async Task<IActionResult> DigitalAdoptionReviewDashboard([FromBody] APIFilter data)
        {
            try
            {
                return Ok(await this._digitalAdoptionReviewListRepository.DigitalAdoptionReviewDashboard(data));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("UserDigitalAdoption")]
        public async Task<IActionResult> UserDigitalAdoption()
        {
            try
            {
                return Ok(await this._digitalAdoptionReviewListRepository.UserDigitalAdoption(UserId));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APIDigitalAdoptionReview data)
        {
            try
            {
                DigitalAdoptionReview digitalAdoptionReviewData = await _digitalAdoptionReviewListRepository.Get(id);
                if (digitalAdoptionReviewData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                digitalAdoptionReviewData.ModifiedBy = data.ModifiedBy;
                digitalAdoptionReviewData.ModifiedDate = DateTime.Now;
                digitalAdoptionReviewData.DigitalAwareness = data.DigitalAwareness;
                digitalAdoptionReviewData.InvolvementLevel = data.InvolvementLevel;
                digitalAdoptionReviewData.UseCaseKnowledge = data.UseCaseKnowledge;
                digitalAdoptionReviewData.Remarks = data.Remarks;
                digitalAdoptionReviewData.code = data.code;
                digitalAdoptionReviewData.ReviewerId = data.ReviewerId;

                await _digitalAdoptionReviewListRepository.Update(digitalAdoptionReviewData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id, [FromBody] DigitalAdoptionReview data)
        {
            try
            {
                DigitalAdoptionReview digitalAdoptionReviewData = await _digitalAdoptionReviewListRepository.Get(id);
                if (digitalAdoptionReviewData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                digitalAdoptionReviewData.ModifiedBy = data.ModifiedBy;
                digitalAdoptionReviewData.ModifiedDate = DateTime.Now;
                digitalAdoptionReviewData.DigitalAwareness = data.DigitalAwareness;
                digitalAdoptionReviewData.InvolvementLevel = data.InvolvementLevel;
                digitalAdoptionReviewData.UseCaseKnowledge = data.UseCaseKnowledge;
                digitalAdoptionReviewData.IsDeleted = data.IsDeleted;

                await _digitalAdoptionReviewListRepository.Update(digitalAdoptionReviewData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("UseCases")]
        public async Task<IActionResult> PostUseCasesList([FromBody] APIUseCase data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UseCase useCase = new UseCase();

                    useCase.ModifiedBy = UserId;
                    useCase.CreatedBy = UserId;
                    useCase.IsActive = true;
                    useCase.IsDeleted = false;
                    useCase.CreatedDate = DateTime.UtcNow;
                    useCase.ModifiedDate = DateTime.UtcNow;
                    useCase.Code = data.Code;
                    useCase.Description = data.Description;

                    await _useCasesListRepository.Add(useCase);
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

        [HttpGet("UseCases/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetUseCases(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._useCasesListRepository.GetUseCases(page, pageSize, search));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UseCases/{id}")]
        public async Task<IActionResult> PutUseCases(int id, [FromBody] APIUseCase data)
        {
            try
            {
                UseCase useCaseData = await _useCasesListRepository.Get(id);
                if (useCaseData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                useCaseData.ModifiedBy = UserId;
                useCaseData.ModifiedDate = DateTime.Now;
                useCaseData.Description = data.Description;
                useCaseData.Code = data.Code;
                useCaseData.IsActive = data.IsActive;
                await _useCasesListRepository.Update(useCaseData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UseCases/Delete/{id}")]
        public async Task<IActionResult> DeleteUseCases(int id, [FromBody] APIUseCase data)
        {
            try
            {
                UseCase useCaseData = await _useCasesListRepository.Get(id);
                if (useCaseData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                useCaseData.ModifiedBy = UserId;
                useCaseData.ModifiedDate = DateTime.Now;
                useCaseData.IsActive = data.IsActive;
                useCaseData.IsDeleted = true;
                await _useCasesListRepository.Update(useCaseData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("DigitalRole")]
        public async Task<IActionResult> PostDigitalRolesList([FromBody] APIDigitalRole data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DigitalRole digitalRole = new DigitalRole();

                    digitalRole.ModifiedBy = UserId;
                    digitalRole.CreatedBy = UserId;
                    digitalRole.IsActive = true;
                    digitalRole.IsDeleted = false;
                    digitalRole.CreatedDate = DateTime.UtcNow;
                    digitalRole.ModifiedDate = DateTime.UtcNow;
                    digitalRole.Code = data.Code;
                    digitalRole.Description = data.Description;

                    await _digitalRolesList.Add(digitalRole);
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

        [HttpGet("DigitalRole/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetDigitalRole(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._digitalRolesList.GetDigitalRoles(page, pageSize, search));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("DigitalRole/{id}")]
        public async Task<IActionResult> PutDigitalRole(int id, [FromBody] APIDigitalRole data)
        {
            try
            {
                DigitalRole digitalRoleData = await _digitalRolesList.Get(id);
                if (digitalRoleData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                digitalRoleData.ModifiedBy = UserId;
                digitalRoleData.ModifiedDate = DateTime.Now;
                digitalRoleData.Description = data.Description;
                digitalRoleData.Code = data.Code;
                digitalRoleData.IsActive = data.IsActive;
                await _digitalRolesList.Update(digitalRoleData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("DigitalRole/Delete/{id}")]
        public async Task<IActionResult> Delete(int id, [FromBody] UseCase data)
        {
            try
            {
                DigitalRole digitalRoleData = await _digitalRolesList.Get(id);
                if (digitalRoleData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                digitalRoleData.ModifiedBy = UserId;
                digitalRoleData.ModifiedDate = DateTime.Now;
                digitalRoleData.IsActive = data.IsActive;
                digitalRoleData.IsDeleted = true;
                await _digitalRolesList.Update(digitalRoleData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
                string sFileName = @"DigitalAdoptionReview.xlsx";
                string DomainName = this._configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);

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

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("DigitalAdoptionReview");

                    worksheet.Cells[1, 1].Value = "EmployeeCode*";
                    worksheet.Cells[1, 2].Value = "UseCase*";
                    worksheet.Cells[1, 3].Value = "Role*";
                    worksheet.Cells[1, 4].Value = "InvolvementLevel*";
                    worksheet.Cells[1, 5].Value = "DigitalAwareness*";
                    worksheet.Cells[1, 6].Value = "UseCaseKnowledge*";
                    worksheet.Cells[1, 7].Value = "Remark*";
                    package.Save();
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "DigitalAdoptionReview.xlsx");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostFileUpload")]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
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

                        if (FileValidation.IsValidXLSX(fileUpload))
                        {
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrgCode);
                            string DomainName = this._configuration["ApiGatewayUrl"];
                            fileDir = Path.Combine(fileDir, customerCode, FileType);
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

        [HttpPost]
        [Route("SaveFileData")]
        public async Task<IActionResult> PostFile([FromBody] APIDARImport path)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                APIResponse response = await this._digitalAdoptionReviewListRepository.ProcessImportFile(path, UserId, OrgCode, UserName);
                return Ok(response.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }
    }
}




