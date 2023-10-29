using AspNet.Security.OAuth.Introspection;
using Gadget.API.Helper;
using Gadget.API.Common;
using Gadget.API.Metadata;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Gadget.API.Common.TokenPermissions;
using Gadget.API.APIModel;
using AutoMapper;
using Gadget.API.Helper.Log_API_Count;

namespace Gadget.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/g/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class WorkDiaryController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(WorkDiaryController));

        private readonly IHttpContextAccessor _httpContextAccessor;
        IWorkDiaryRepository _workDiaryRepository;

        public WorkDiaryController(IHttpContextAccessor httpContextAccessor,
                                IWorkDiaryRepository workDiaryRepository,
                                IIdentityService identitySvc) : base(identitySvc)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._workDiaryRepository = workDiaryRepository;
        }

        [HttpPost]
        public async Task<IActionResult> PostWorkDiary([FromBody] WorkDiary workDiaryEntry)
        {
            try
            {
                WorkDiary workDiaryObj = Mapper.Map<WorkDiary>(workDiaryEntry);
                workDiaryObj.IsDeleted = false;
                workDiaryObj.CreatedBy = UserId;
                workDiaryObj.CreatedDate = DateTime.Now;

                await this._workDiaryRepository.Add(workDiaryObj);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
