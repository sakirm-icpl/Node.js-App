using AspNet.Security.OAuth.Introspection;

using Competency.API.Common;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Competency.API.Common.TokenPermissions;
using static Competency.API.Model.ResponseModels;

namespace Competency.API.Controllers.Competency
{


    [Route("api/v1/comp/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]

    public class CompetencyReviewParametersController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencyReviewParametersController));
        private ICompetencyReviewParametersRepository competencyReviewParametersRepository;


        public CompetencyReviewParametersController(IConfiguration configure, IHttpContextAccessor httpContextAccessor, ICompetencyReviewParametersRepository competencyReviewParametersRepository, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this.competencyReviewParametersRepository = competencyReviewParametersRepository;

        }

        [HttpPost]
        public async Task<IApiResponse> Post([FromBody] CompetencyReviewParameters competencyReview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CompetencyReviewParameters competencyReviewParameters = new CompetencyReviewParameters();
                    if (await this.competencyReviewParametersRepository.Exists(competencyReview.ReviewParameter))
                        return new APIResposeNo { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Content = EnumHelper.GetEnumDescription(MessageType.Duplicate), StatusCode = 409 };
                    else
                    {
                        competencyReviewParameters.CompetencyCategoryId = competencyReview.CompetencyCategoryId;
                        competencyReviewParameters.CompetencySubcategoryId = competencyReview.CompetencySubcategoryId;
                        competencyReviewParameters.CompetencySubSubcategoryId = competencyReview.CompetencySubSubcategoryId;
                        competencyReviewParameters.CompetencyId = competencyReview.CompetencyId;
                        competencyReviewParameters.Level = competencyReview.Level;             
                        competencyReviewParameters.LevelDescription = competencyReview.LevelDescription;             
                        competencyReviewParameters.JobRoleId = competencyReview.JobRoleId;
                        competencyReviewParameters.ReviewParameter = competencyReview.ReviewParameter;
                        competencyReviewParameters.IsDeleted = false;
                        competencyReviewParameters.IsActive = true;
                        competencyReviewParameters.CreatedBy = UserId;
                        competencyReviewParameters.CreatedDate = DateTime.UtcNow;
                        competencyReviewParameters.ModifiedDate = DateTime.UtcNow;
                        try
                        {
                            await competencyReviewParametersRepository.Add(competencyReviewParameters);

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.InnerException);
                            throw;
                        }
                        APIResposeYes aPIResposeYes = new APIResposeYes();
                        aPIResposeYes.Message = "Record Inserted Successfully";
                        aPIResposeYes.Content = "Record Inserted Successfully";
                        return aPIResposeYes;
                    }
                }
                else
                    return new APIResposeNo { Message = "Invalid Data", Content = "Invalid Data" };
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new APIResposeNo { };
            }
        }

        [HttpPost("Update")]
        public async Task<IApiResponse> Put([FromBody] CompetencyReviewParameters reviewParameters)
        {
            try
            {
                CompetencyReviewParameters competencyReviewParameters = await this.competencyReviewParametersRepository.Get(s => s.IsDeleted == false && s.Id != reviewParameters.Id && s.ReviewParameter == reviewParameters.ReviewParameter);
                if (competencyReviewParameters != null)
                {
                    return new APIResposeNo { Message = "Review Parameters Already Exists", Content = "Review Parameters Already Exists", StatusCode = 409 };
                }
                //competencyReviewParameters = await this.competencyReviewParametersRepository.Get(s => s.IsDeleted == false && s.Id != reviewParameters.Id && s.SubcategoryDescription == reviewParameters.SubcategoryDescription);

                //if (competencyReviewParameters != null)
                //{
                //    return new APIResposeNo { Message = "Subcategory Description Already Exists", Content = "Subcategory Description Already Exists", StatusCode = 409 };
                //}
                competencyReviewParameters = await this.competencyReviewParametersRepository.Get(s => s.IsDeleted == Record.NotDeleted && s.Id == reviewParameters.Id);

                if (ModelState.IsValid && competencyReviewParameters != null)
                {
                    competencyReviewParameters.CompetencyCategoryId = reviewParameters.CompetencyCategoryId;
                    competencyReviewParameters.CompetencySubcategoryId = reviewParameters.CompetencySubcategoryId;
                    competencyReviewParameters.CompetencyId = reviewParameters.CompetencyId;
                    competencyReviewParameters.Level = reviewParameters.Level;
                    competencyReviewParameters.LevelDescription = reviewParameters.LevelDescription;
                    competencyReviewParameters.JobRoleId = reviewParameters.JobRoleId;
                    competencyReviewParameters.ReviewParameter = reviewParameters.ReviewParameter;
                    competencyReviewParameters.IsDeleted = false;
                    competencyReviewParameters.IsActive = true;
                    competencyReviewParameters.CreatedBy = UserId;
                    competencyReviewParameters.ModifiedBy = UserId;
                    competencyReviewParameters.ModifiedDate = DateTime.UtcNow;
                    await this.competencyReviewParametersRepository.Update(competencyReviewParameters);

                }
                else
                {
                    return new APIResposeNo { Message = "Invalid Data", Content = "Invalid Data", StatusCode = 400 };
                }

                APIResposeYes aPIResposeYes = new APIResposeYes();
                aPIResposeYes.Message = "Record Updated Successfully";
                aPIResposeYes.Content = "Record Updated Successfully";
                return aPIResposeYes;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new APIResposeNo { StatusCode = 500, Message = "Error", Content = "Exception Occurs" + ex.Message };
            }
        }

        [HttpPost("getAllCompetencyReviewParameters")]
        public async Task<IActionResult> GetAllCompetencyReviewParameters([FromBody] ReviewParametersPostModel postModel)
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.GetAllCompetencyReviewParameters(postModel);
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                //int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CompetencyReviewParameters competencyReview = await this.competencyReviewParametersRepository.Get(id);

                if (competencyReview == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                //if (competencySubCategoryRepository.IsDependacyExist(compentencySubCat.CategoryId))
                //{
                //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                //}
                competencyReview.IsDeleted = true;
                await this.competencyReviewParametersRepository.Update(competencyReview);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("getCompetencyReviewParameters")]
        public async Task<IActionResult> GetCompetencyReviewParameters([FromBody] GetReviewParametersPostModel postModel)
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.GetCompetencyReviewParameters(postModel);
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getCompetencyReviewParametersOptions")]
        public async Task<IActionResult> getCompetencyReviewParametersOptions()
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.GetCompetencyReviewParametersOptions();
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getCompetencyReviewParametersSupervisorOptions")]
        public async Task<IActionResult> getCompetencyReviewParametersSupervisorOptions()
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.getCompetencyReviewParametersSupervisorOptions();
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetSelfRatingforSupervisor")]
        public async Task<IActionResult> GetSelfRatingforSupervisor([FromBody]SelfRatingForSupervisor selfRatingForSupervisor)
        {
            try
            {
                _logger.Error(selfRatingForSupervisor);
                if(selfRatingForSupervisor == null)
                {
                    return BadRequest("Bad Request");
                }
                if(selfRatingForSupervisor.CompetencyId == null || selfRatingForSupervisor.UserId == null)
                {
                    return BadRequest("Bad Request CompetencyID or UserID is missing");
                }
                var competencyReviewParameters = await this.competencyReviewParametersRepository.GetSelfRatingforSupervisor(selfRatingForSupervisor);
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetLastAssessmentDate")]
        public async Task<IActionResult> GetLastAssessmentDate([FromBody] UserIdPayload selfRatingForSupervisor)
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.GetLastAssessmentDate(selfRatingForSupervisor);
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("SelfAssessment")]
        public async Task<IActionResult> SaveCompetencySelfAssessmentResult([FromBody] APICompetencyReviewParametersSelfAssessment postModel)
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.SaveAssessment(postModel,UserId);
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }  
        
        [HttpPost("SelfAssessment/Update")]
        public async Task<IActionResult> SaveCompetencySelfAssessmentResultUpate([FromBody] APICompetencyReviewParametersSelfAssessment postModel)
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.SaveAssessmentUpdate(postModel,UserId);
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("SupervisorAssessment")]
        public async Task<IActionResult> SaveSupervisorAssessment([FromBody] CompetencySupervisorUpdate postModel)
        {
            try
            {
                var competencyReviewParameters = await this.competencyReviewParametersRepository.SaveSupervisorAssessment(postModel,UserId);
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUserSelfAssessment")]
        public async Task<IActionResult> GetUserSelfAssessment([FromBody] UserIdPayload userIdPayload)
        {
            try
            {
                string userid = Security.DecryptForUI(userIdPayload.Id);
                if(userid == null)
                {
                    return BadRequest(
                        new ResponseMessage
                        { 
                            Message = EnumHelper.GetEnumName(MessageType.InternalServerError), 
                            Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) 
                        });
                }
                var competencyReviewParameters = await this.competencyReviewParametersRepository.GetUserSelfAssessment(Convert.ToInt32(userid));
                return Ok(competencyReviewParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }
}
