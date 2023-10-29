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
using Courses.API.Repositories;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/c/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired(Permissions.coursemanage)]
    public class OpenAIController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OpenAIController));
        IOpenAIRepository _openAIRepository;
        IOpenAICourseQuestionAssociationRepository _openAICourseQuestionAssociation;
        private readonly ITokensRepository _tokensRepository;
        public OpenAIController(IIdentityService identitySvc, ITokensRepository tokensRepository, IOpenAIRepository openAIRepository, IOpenAICourseQuestionAssociationRepository openAICourseQuestionAssociation) : base(identitySvc)
        {
            this._tokensRepository = tokensRepository;
            this._openAIRepository = openAIRepository;
            this._openAICourseQuestionAssociation = openAICourseQuestionAssociation;
        }

        [HttpGet("Questions")]
        public async Task<IActionResult> GetOpenAIQuestions()
        {
            try
            {
                return Ok(await _openAIRepository.GetQuestions());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Questions/{CourseCode}")]
        public async Task<IActionResult> GetOpenAIQuestionsByCourseId(string CourseCode)
        {
            try
            {
                return Ok(await _openAIRepository.GetQuestionsByCourseId(CourseCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostQuestions")]
        public async Task<IActionResult> PostOpenAIQuestions([FromBody]APIOpenAIQuestion data)
        {
            try
            {
                OpenAIQuestion Questions = new OpenAIQuestion();
                
                Questions.QuestionText = data.QuestionText;
                Questions.AnswerText = data.AnswerText;
                Questions.Metadata = data.Metadata;
                Questions.Industry = data.Industry;

                Questions.IsDeleted = false;
                Questions.CreatedBy = UserId;
                Questions.CreatedDate = DateTime.UtcNow;
                Questions.ModifiedBy = UserId;
                Questions.ModifiedDate = DateTime.UtcNow;

                await _openAIRepository.Add(Questions);

                if (data.CourseCode != null)
                {
                    OpenAICourseQuestionAssociation Mapping = new OpenAICourseQuestionAssociation();

                    Mapping.QuestionId = Questions.Id;
                    Mapping.CourseCode = data.CourseCode;

                    Mapping.IsDeleted = false;
                    Mapping.CreatedBy = UserId;
                    Mapping.CreatedDate = DateTime.UtcNow;
                    Mapping.ModifiedBy = UserId;
                    Mapping.ModifiedDate = DateTime.UtcNow;

                    await _openAICourseQuestionAssociation.Add(Mapping);
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteQuestion([FromQuery] string id)
        {
            try
            {

                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));

                OpenAIQuestion question = await _openAIRepository.Get(DecryptedId);
                if (question == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                
                question.IsDeleted = true;

                await _openAIRepository.Update(question);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
        }

        [HttpDelete("Questions/{CourseCode}")]
        public async Task<IActionResult> DeleteQuestionByCourseCode(string CourseCode,[FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int MappingId = await _openAIRepository.GetMappingId(CourseCode, DecryptedId);

                OpenAICourseQuestionAssociation Mapping = await _openAICourseQuestionAssociation.Get(MappingId);
                if (Mapping == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                await _openAICourseQuestionAssociation.Remove(Mapping);
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
