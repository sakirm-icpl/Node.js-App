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
    [Produces("application/json")]
    [Route("api/v1/Section")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired(Permissions.coursemanage)]
    public class SectionController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SectionController));
        private ISectionRepository _sectionRepository;
        private ILessonRepository _lessonRepository;
        private readonly ITokensRepository _tokensRepository;
        public SectionController(ISectionRepository sectionRepository, ILessonRepository lessonRepository, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this._sectionRepository = sectionRepository;
            this._lessonRepository = lessonRepository;
            this._tokensRepository = tokensRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {

                return Ok(await this._sectionRepository.GetAll());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }


        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code)
        {
            try
            {
                return Ok(await this._sectionRepository.GetByCourseCode(code));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ApiSection apiSection)
        {
            try
            {
                int SectionId;
                Section SectionObj = Mapper.Map<Section>(apiSection);
                SectionObj.CreatedDate = DateTime.UtcNow;
                SectionObj.CreatedBy = UserId;
                if (_sectionRepository.Exist(SectionObj))
                {
                    return BadRequest(
                   new ResponseMessage
                   {
                       Message = EnumHelper.GetEnumName(MessageType.Duplicate),
                       Description = EnumHelper.GetEnumDescription(MessageType.Duplicate)
                   });
                }

                await this._sectionRepository.Add(SectionObj);
                SectionId = SectionObj.Id;
                List<Lesson> lessonsList = new List<Lesson>();
                Lesson lesson = new Lesson();
                foreach (APILesson apilesson in apiSection.ApiLessons)
                {
                    lesson.LessonNumber = apilesson.LessonNumber;
                    lesson.SectionId = SectionId;
                    lesson.Title = apilesson.Title;
                    lesson.Description = apilesson.Description;
                    lessonsList.Add(lesson);
                }
                await this._lessonRepository.AddRange(lessonsList);
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Put([FromBody] ApiSection apiSection)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Section OldSection = await this._sectionRepository.Get(apiSection.Id);
                if (OldSection == null)
                    return NotFound();

                OldSection = Mapper.Map<Section>(apiSection);
                OldSection.ModifiedDate = DateTime.UtcNow;
                OldSection.ModifiedBy = UserId;

                if (await _sectionRepository.ExistForUpdate(OldSection))
                    return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 201,
                       Message = EnumHelper.GetEnumName(MessageType.Duplicate),
                       Description = EnumHelper.GetEnumDescription(MessageType.Duplicate)
                   });

                await this._sectionRepository.Update(OldSection);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {

                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Section section = await _sectionRepository.Get(DecryptedId);
                if (section == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (await _sectionRepository.IsDependacyExist(DecryptedId))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });

                await _sectionRepository.Remove(section);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
        }
    }
}