using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Repositories.Interfaces;
//using ILT.API.Repositories.Interfaces.ILT;
using ILT.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static ILT.API.Common.AuthorizePermissions;
using static ILT.API.Common.TokenPermissions;
using ILT.API.Helper;
using log4net;
using System.Collections.Generic;
using System.Linq;
using ILT.API.Model.ILT;
using ILT.API.Model.Log_API_Count;

namespace ILT.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/ILTRequestResponse")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class ILTRequestResponseController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTRequestResponseController));
        IILTRequestResponse _IILTRequestResponse;
        private readonly ITokensRepository _tokensRepository;
        public ILTRequestResponseController(IILTRequestResponse IILTRequestResponse, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _IILTRequestResponse = IILTRequestResponse;
            this._tokensRepository = tokensRepository;
        }


        [HttpGet("GetByModuleID/{moduleId}/{courseId}")]
        public async Task<IActionResult> GetByModuleID(int moduleId, int courseId)
        {
            try
            {
                APIILTRequestRsponse result = null;
                result = await this._IILTRequestResponse.GetAllRequestDetails(moduleId, courseId, UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByModuleIDFroDropout/{moduleId}/{courseId}")]
        public async Task<IActionResult> GetByModuleIDFroDropout(int moduleId, int courseId)
        {
            try
            {
                APIILTRequestRsponse result = null;
                result = await this._IILTRequestResponse.GetWNSAllRequestDetails(moduleId, courseId, UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBatchesForRequest/{CourseId}")]
        public async Task<IActionResult> GetBatchesForRequest(int CourseId)
        {
            try
            {
                var result = await this._IILTRequestResponse.GetBatchesForRequest(CourseId, UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page}/{pageSize}/{searchParameter}/{searchText}")]
        [PermissionRequired(Permissions.iltrequests)]
        public async Task<IActionResult> GetRequest(int page, int pageSize, string searchParameter = null, string searchText = null)
        {
            try
            {

                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTRequestResponse.GetRequest(UserId, RoleCode, page, pageSize, searchParameter, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Count/{searchParameter}/{searchText}")]
        [PermissionRequired(Permissions.iltrequests)]
        public async Task<IActionResult> Count(string searchParameter = null, string searchText = null)
        {
            try
            {
                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTRequestResponse.GetRequestedUserCount(UserId, RoleCode, searchParameter, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostRequest")]
        public async Task<IActionResult> PostRequest([FromBody] APIILTRequestResponse aPIILTRequestResponse)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string result = await _IILTRequestResponse.PostRequest(aPIILTRequestResponse, UserId, OrganisationCode);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

                ILTSchedule objSchedule = await _IILTRequestResponse.GetSchedulePurpose(aPIILTRequestResponse.ScheduleID);
                if(objSchedule==null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

                if (objSchedule.Purpose == "Planned Training" && aPIILTRequestResponse.TrainingRequesStatus== "Requested" && (OrgCode.ToLower().Contains("wns")  || OrgCode.ToLower().Contains("wnsuat")))
                {
                    if (objSchedule.RequestApproval == false)
                    {
                        ILTRequestResponse oldRequestResponce = await _IILTRequestResponse.GetRequestResponse(aPIILTRequestResponse, UserId);

                        if (oldRequestResponce != null)
                        {
                            if (oldRequestResponce.TrainingRequesStatus != "Waiting")
                            {
                                aPIILTRequestResponse.TrainingRequesStatus = "Approved";
                                aPIILTRequestResponse.UserID = UserId;


                                var resultApproved = await _IILTRequestResponse.PostResponse(aPIILTRequestResponse, UserId, OrganisationCode);
                                if (resultApproved != "Success")
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

                                aPIILTRequestResponse.TrainingRequesStatus = "Availability";
                                aPIILTRequestResponse.UserID = UserId;


                                var result1 = await _IILTRequestResponse.PostRequest(aPIILTRequestResponse, UserId, OrganisationCode);
                                if (result1 != "Success")
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

                                return Ok();
                            }
                        }
                    }
                    return Ok();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostDropOutRequest")]
        public async Task<IActionResult> PostDropOutRequest([FromBody] APIILTRequestResponse aPIILTRequestResponse)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string result = await _IILTRequestResponse.PostDropOutRequest(aPIILTRequestResponse, UserId, OrganisationCode);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

             
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostBatchRequest")]
        public async Task<IActionResult> PostBatchRequest([FromBody] APIILTBatchRequestResponse aPIILTBatchRequestResponse)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string result = await _IILTRequestResponse.PostBatchRequest(aPIILTBatchRequestResponse, UserId, OrganisationCode);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("PostResponse")]
        [PermissionRequired(Permissions.iltrequests)]
        public async Task<IActionResult> PostResponse([FromBody] APIILTRequestResponse aPIILTRequestResponse)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _IILTRequestResponse.PostResponse(aPIILTRequestResponse, UserId, OrganisationCode);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllRequestedBatches/{page}/{pageSize}/{searchParameter}/{searchText}")]
        [PermissionRequired(Permissions.iltbatchrequests)]
        public async Task<IActionResult> GetAllRequestedBatches(int page, int pageSize, string searchParameter = null, string searchText = null)
        {
            try
            {

                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTRequestResponse.GetAllRequestedBatches(UserId, RoleCode, page, pageSize, searchParameter, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllRequestedBatchesCount/{searchParameter}/{searchText}")]
        [PermissionRequired(Permissions.iltbatchrequests)]
        public async Task<IActionResult> GetAllRequestedBatchesCount(string searchParameter = null, string searchText = null)
        {
            try
            {
                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTRequestResponse.GetAllRequestedBatchesCount(UserId, RoleCode, searchParameter, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBatchRequests/{batchId}/{page}/{pageSize}/{searchParameter}/{searchText}")]
        [PermissionRequired(Permissions.iltbatchrequests)]
        public async Task<IActionResult> GetBatchRequests(int batchId, int page, int pageSize, string searchParameter = null, string searchText = null)
        {
            try
            {

                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTRequestResponse.GetBatchRequests(UserId, RoleCode, batchId, page, pageSize, searchParameter, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetBatchRequestsCount/{batchId}/{searchParameter}/{searchText}")]
        [PermissionRequired(Permissions.iltbatchrequests)]
        public async Task<IActionResult> GetBatchRequestsCount(int batchId, string searchParameter = null, string searchText = null)
        {
            try
            {
                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTRequestResponse.GetBatchRequestsCount(UserId, RoleCode, batchId, searchParameter, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("PostBatchResponse")]
        [PermissionRequired(Permissions.iltbatchrequests)]
        public async Task<IActionResult> PostBatchResponse([FromBody] List<APIILTBatchRequestApprove> aPIILTBatchRequestApprove)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _IILTRequestResponse.PostBatchResponse(aPIILTBatchRequestApprove, UserId, OrganisationCode);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}