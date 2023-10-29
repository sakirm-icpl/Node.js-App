using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.TokenPermissions;
using Courses.API.Helper;
using log4net;
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/c/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CourseCompletionMailReminderController : IdentityController
    {
        private ICourseCompletionMailReminder _mailReminder;
        public CourseCompletionMailReminderController(ICourseCompletionMailReminder mailReminder, IIdentityService identitySvc) : base(identitySvc)
        {
            this._mailReminder = mailReminder;
        }


        [HttpPost]
        public async Task<IActionResult> PostCompletionMailReminder([FromBody] CourseCompletionMailReminder data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CourseCompletionMailReminder data1 = await this._mailReminder.PostCompletionMailReminder(data, UserId);
                    if (data1 == null)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
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


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetCompletionMailReminder(int page, int pageSize , string search = null)
        {
            try
            {
                return Ok(await this._mailReminder.GetCompletionMailReminder(page, pageSize, search ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CourseCompletionMailReminder data)
        {
            try
            {
                CourseCompletionMailReminder mailReminderData = await _mailReminder.Get(id);
                if (mailReminderData == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                mailReminderData.ModifiedBy = UserId;
                mailReminderData.ModifiedDate = DateTime.Now;
                mailReminderData.FirstRemDays = data.FirstRemDays;
                mailReminderData.FirstRemTemplate = data.FirstRemTemplate;
                mailReminderData.SecondRemDays= data.SecondRemDays;
                mailReminderData.SecondRemTemplate = data.SecondRemTemplate;
                mailReminderData.ThirdRemDays = data.ThirdRemDays;
                mailReminderData.ThirdRemTemplate = data.ThirdRemTemplate;
                mailReminderData.FourthRemDays = data.FourthRemDays;
                mailReminderData.FourthRemTemplate = data.FourthRemTemplate;
                mailReminderData.FifthRemDays = data.FifthRemDays;
                mailReminderData.FifthRemTemplate = data.FifthRemTemplate;
                mailReminderData.CourseId = data.CourseId;
                await _mailReminder.Update(mailReminderData);
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
                CourseCompletionMailReminder mailReminderData = await _mailReminder.Get(id);

                if (mailReminderData == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                await _mailReminder.Remove(mailReminderData);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
