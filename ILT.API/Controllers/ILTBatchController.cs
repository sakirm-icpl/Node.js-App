using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper.Metadata;
using ILT.API.Model.ILT;
using ILT.API.Model.Log_API_Count;
using ILT.API.Repositories.Interfaces;
using ILT.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ILT.API.Common.AuthorizePermissions;
using static ILT.API.Common.TokenPermissions;

namespace ILT.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/i/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class ILTBatchController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTScheduleController));
        private IILTBatchRepository _batchRepository;
        public ILTBatchController(IIdentityService identityService,
                                    IILTBatchRepository batchRepository) :base(identityService)
        {
            _batchRepository = batchRepository;
        }

        [HttpGet("BatchCode")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetBatchCode()
        {
            try
            {
                BatchCode batchCode = await _batchRepository.GetBatchCode(UserId);
                string Code = "B" + batchCode.Id;
                return Ok(Code);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("CancelBatchCode")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> CancelScheduleCode([FromBody] APIBatchCode aPIBatchCode)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                await _batchRepository.CancelBatchCode(aPIBatchCode, UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> Post([FromBody] APIILTBatch aPIILTBatch)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _batchRepository.PostBatch(aPIILTBatch, UserId);
                if(response.StatusCode==200)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("UpdateILTBatch")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> Put([FromBody] APIILTBatch aPIILTBatch)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _batchRepository.PutBatch(aPIILTBatch, UserId);
                if (response.StatusCode == 200)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("IsBatchwiseNominationEnabled")]
        public async Task<IActionResult> IsBatchwiseNominationEnabled()
        {
            try
            {
                string value = await _batchRepository.IsBatchwiseNominationEnabled();
                return Ok(value);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBatch/{Id}")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> Get(int Id)
        {
            try
            {
                APIILTBatch aPIILTBatch = await _batchRepository.GetBatch(Id);
                if(aPIILTBatch==null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                return Ok(aPIILTBatch);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("DeleteBatch")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> Delete([FromBody] APIILTBatchDelete aPIILTBatchDelete)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                MessageType messageType = await _batchRepository.DeleteBatch(aPIILTBatchDelete);
                if (messageType != MessageType.Success)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(messageType), Description = EnumHelper.GetEnumDescription(messageType) });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBatches/{page}/{pageSize}/{search?}/{searchText?}")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetBatches(int page, int pageSize, string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;

                var result = await _batchRepository.GetBatches(UserId, page, pageSize, search, searchText, false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBatchCount/{search?}/{searchText?}")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetBatchCount(string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;

                var result = await _batchRepository.GetBatchCount(UserId, search, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ExportFormat")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> ExportImportFormat()
        {
            try
            {
                var result = await _batchRepository.ExportImportFormat(OrganisationCode);
                Response.ContentType = FileContentType.Excel;
                return File(result, FileContentType.Excel, FileName.ILTBatchImportFormat);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SaveFileData")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> PostFile([FromBody] APIILTBatchImport aPIILTBatchImport)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ApiResponse response = await this._batchRepository.ProcessImportFile(aPIILTBatchImport, UserId, OrganisationCode);
                return Ok(response.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }
        [HttpGet("GetRejected/{page:int}/{pageSize:int}")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetAllRejected(int page, int pageSize)
        {
            try
            {
                IEnumerable<APIILTBatchRejected> aPIILTBatchRejecteds = await this._batchRepository.GetBatchRejected(page, pageSize);
                return Ok(aPIILTBatchRejecteds);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ExportRejected")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> ExportRejected()
        {
            try
            {
                FileInfo ExcelFile;
                ExcelFile = await this._batchRepository.ExportILTBatchReject();

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(data, FileContentType.Excel, FileName.ILTBatchRejected);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetRejectedCount")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetRejectedCount()
        {
            try
            {
                int count = await _batchRepository.CountRejected();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBatchTypeAhead/{CourseId}/{search?}")]
        public async Task<IActionResult> GetBatchName(int CourseId, string search = null)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._batchRepository.GetBatchName(CourseId, search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBatchForNomination/{CourseId}/{search?}")]
        public async Task<IActionResult> GetBatchForNomination(int CourseId, string search = null)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._batchRepository.GetBatchForNomination(CourseId, search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ExportBatches/{search?}/{searchText?}")]
        [PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> ExportBatches(string search = null, string searchText = null)
        {
            try
            {
                FileInfo ExcelFile;
                ExcelFile = await this._batchRepository.ExportBatches(UserId, OrganisationCode, search, searchText);

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(data, FileContentType.Excel, FileName.ILTBatches);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
