using AspNet.Security.OAuth.Introspection;
using Courses.API.Common;
using Courses.API.Model;
using Courses.API.Model.Log_API_Count;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/c/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]

    public class ExternalTrainingRequestController : IdentityController
    {
        private IExternalTrainingRequest _trainingRequest;
        public ExternalTrainingRequestController(IExternalTrainingRequest trainingRequest, IIdentityService identitySvc) : base(identitySvc)
        {
            this._trainingRequest = trainingRequest;
        }

        [HttpPost]
        public async Task<IActionResult> PostExternalTrainingRequest([FromBody] ExternalTrainingRequest data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ExternalTrainingRequest data1 = await this._trainingRequest.PostExternalTrainingRequest(data, UserId);
                    return Ok();
                }
                else
                {
                    return this.BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{userId:int}/{search?}")]
        public async Task<IActionResult> GetExternalTrainingRequest(int page, int pageSize,int userId,string search=null)
        {
            try
            {
                    return Ok(await this._trainingRequest.GetExternalTrainingRequest(page, pageSize, search,userId));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetExternalTrainingRequest/{page:int}/{pageSize:int}/{searchBy?}/{search?}")]
        public async Task<IActionResult> GetExternalTrainingRequestAllUser(int page, int pageSize,string searchBy = null ,string search = null)
        {
            try
            {
                return Ok(await this._trainingRequest.GetExternalTrainingRequestAllUser(page, pageSize,searchBy ,search,UserId));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetExternalTrainingRequestEdit/{reqId:int}/{userId:int}")]
        public async Task<IActionResult> GetExternalTrainingRequestEdit(int reqId,int userId)
        {
            try
            {
                return Ok(await this._trainingRequest.GetExternalTrainingRequestEdit(reqId,userId));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ExternalTrainingRequest data)
        {
            try
            {
                ExternalTrainingRequest externalTrainData = await _trainingRequest.Get(id);
                if (externalTrainData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                externalTrainData.ModifiedBy = UserId;
                externalTrainData.ModifiedDate = DateTime.Now;
                externalTrainData.Title = data.Title;
                externalTrainData.Fee = data.Fee;
                externalTrainData.Trainer = data.Trainer;
                externalTrainData.StartDate = data.StartDate;
                externalTrainData.EndDate = data.EndDate;
                externalTrainData.Traveling = data.Traveling;
                externalTrainData.Reason = data.Reason;
                externalTrainData.Status = data.Status;
                externalTrainData.ContentUrl = data.ContentUrl;
                externalTrainData.Currency = data.Currency;
                await _trainingRequest.Update(externalTrainData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            try
            {
                ExternalTrainingRequest externalTrainData = await _trainingRequest.Get(id);

                if (externalTrainData == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                await _trainingRequest.Remove(externalTrainData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




    }
}
