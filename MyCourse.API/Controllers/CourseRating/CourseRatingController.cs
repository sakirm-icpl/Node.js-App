using AspNet.Security.OAuth.Introspection;
using MyCourse.API.APIModel.CourseRating;
using MyCourse.API.Common;
using MyCourse.API.Helper;
using MyCourse.API.Helper.Metadata;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Repositories.Interfaces.CourseRating;
using MyCourse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MyCourse.API.Common.TokenPermissions;
using log4net;
using MyCourse.API.Model.Log_API_Count;

namespace MyCourse.API.Controllers.CourseRating
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class CourseRatingController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRatingController));
        ICourseReviewRepository _courseReviewRepository;
        ICourseRatingRepository _courseRatingRepository;
        private readonly IRewardsPointRepository _rewardsPointRepository;
        private readonly ITokensRepository _tokensRepository;
        public CourseRatingController(ICourseReviewRepository courseReviewRepository, ICourseRatingRepository courseRatingRepository, IIdentityService _identitySvc, IRewardsPointRepository rewardsPointRepository, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this._courseReviewRepository = courseReviewRepository;
            this._courseRatingRepository = courseRatingRepository;
            this._rewardsPointRepository = rewardsPointRepository;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<Model.CourseReview> courseReview = await _courseReviewRepository.GetAll(f => f.IsDeleted == false);
                return Ok(courseReview);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                    });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await _courseReviewRepository.Get(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetReviewByCourseId/{id}")]
        public async Task<IActionResult> GetReviewByCourseId(int id)
        {
            try
            {
                if (_courseRatingRepository.Exists(id))
                {
                    APICourseRatingMerged courseRating = await _courseRatingRepository.GetReviewByCourseId(id);
                    return Ok(courseRating);
                }
                else
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                    });
            }
        }


        [HttpGet("GetRatingByCourseId/{id}")]
        public async Task<IActionResult> GetRatingByCourseId(int id)
        {
            try
            {
                APICourseRatingAndReview courseRating = await _courseRatingRepository.GetRatingByCourseId(id);
                return Ok(courseRating);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                List<APICourseRatingAndReview> courseReview = await _courseReviewRepository.Get(page, pageSize, search);
                return Ok(courseReview);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getTotalRecords/{search:minlength(0)?}")]
        public IActionResult GetCount(string search = null)
        {
            try
            {
                int count = _courseReviewRepository.Count(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ExistsCourseRating/{courseId:int}")]
        public async Task<bool> ExistsPoll(int courseId)
        {
            try
            {
                return await this._courseReviewRepository.ExistCourse(courseId, UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APICourseRatingAndReview aPICourseRatingAndReview)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    bool valid1 = false;

                    if (FileValidation.CheckForSQLInjection(aPICourseRatingAndReview.ReviewText))
                        valid1 = true;

                    if (valid1 == true)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    if (_courseRatingRepository.Exists(aPICourseRatingAndReview.CourseId))
                    {
                        string UserCompletion = await this._courseRatingRepository.GetUserCompletion(aPICourseRatingAndReview.CourseId, UserId);
                        if (UserCompletion == "completed")
                        {
                            if (_courseRatingRepository.CourseReviewExists(aPICourseRatingAndReview.CourseId, UserId))
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                            Model.CourseRating courseRating = await _courseRatingRepository.GetCourseRating(aPICourseRatingAndReview.CourseId);
                            courseRating.OneStar = courseRating.OneStar + aPICourseRatingAndReview.OneStar;
                            courseRating.TwoStar = courseRating.TwoStar + aPICourseRatingAndReview.TwoStar;
                            courseRating.ThreeStar = courseRating.ThreeStar + aPICourseRatingAndReview.ThreeStar;
                            courseRating.FourStar = courseRating.FourStar + aPICourseRatingAndReview.FourStar;
                            courseRating.FiveStar = courseRating.FiveStar + aPICourseRatingAndReview.FiveStar;
                            courseRating.ModifiedBy = UserId;
                            courseRating.ModifiedDate = DateTime.UtcNow;
                            decimal totalPoint = Convert.ToDecimal((courseRating.OneStar * 1) + (courseRating.TwoStar * 2) + (courseRating.ThreeStar * 3) + (courseRating.FourStar * 4) + (courseRating.FiveStar * 5));
                            decimal totalRatingUser = Convert.ToDecimal((courseRating.OneStar) + (courseRating.TwoStar) + (courseRating.ThreeStar) + (courseRating.FourStar) + (courseRating.FiveStar));
                            decimal totalAverage = Convert.ToDecimal(totalPoint / totalRatingUser);
                            courseRating.Average = totalAverage;

                            await _courseRatingRepository.Update(courseRating);

                            Model.CourseReview courseReview = new Model.CourseReview();
                            courseReview.CourseId = aPICourseRatingAndReview.CourseId;
                            courseReview.RatingId = courseRating.Id;
                            courseReview.ReviewRating = aPICourseRatingAndReview.ReviewRating;
                            courseReview.ReviewText = aPICourseRatingAndReview.ReviewText;
                            courseReview.UserId = UserId;
                            courseReview.CreatedBy = UserId;
                            courseReview.CreatedDate = DateTime.UtcNow;
                            courseReview.ModifiedBy = UserId;
                            courseReview.ModifiedDate = DateTime.UtcNow;
                            courseReview.IsActive = true;
                            courseReview.UseName = UserName;

                            await _courseReviewRepository.Add(courseReview);
                            if (!string.IsNullOrEmpty(courseReview.ReviewText))
                            {
                                string Category = RewardPointCategory.Normal;
                                string Condition = RewardPointCategory.RatedaCourse;
                                await this._rewardsPointRepository.AddRatingSubmitReward(UserId, courseRating.Id, courseRating.CourseId, OrganisationCode, Category, Condition);

                                string CategoryForReview = RewardPointCategory.Bonus;
                                string ConditionForReview = RewardPointCategory.RatedaReview;
                                await this._rewardsPointRepository.AddReviewSubmitReward(UserId, courseReview.Id, courseReview.CourseId, OrganisationCode, CategoryForReview, ConditionForReview);

                            }
                            else
                            {
                                string Category = RewardPointCategory.Normal;
                                string Condition = RewardPointCategory.RatedaCourse;
                                await this._rewardsPointRepository.AddRatingSubmitReward(UserId, courseRating.Id, courseRating.CourseId, OrganisationCode, Category, Condition);
                            }
                        }
                        else
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotCompleted), Description = EnumHelper.GetEnumDescription(MessageType.NotCompleted) });
                        }
                    }
                    else
                    {
                        string UserCompletion = await this._courseRatingRepository.GetUserCompletion(aPICourseRatingAndReview.CourseId, UserId);
                        if (UserCompletion == "completed")
                        {
                            Model.CourseRating courseRating = new Model.CourseRating();
                            courseRating.CourseId = aPICourseRatingAndReview.CourseId;
                            courseRating.OneStar = aPICourseRatingAndReview.OneStar;
                            courseRating.TwoStar = aPICourseRatingAndReview.TwoStar;
                            courseRating.ThreeStar = aPICourseRatingAndReview.ThreeStar;
                            courseRating.FourStar = aPICourseRatingAndReview.FourStar;
                            courseRating.FiveStar = aPICourseRatingAndReview.FiveStar;
                            courseRating.CreatedBy = UserId;
                            courseRating.CreatedDate = DateTime.UtcNow;
                            courseRating.ModifiedBy = UserId;
                            courseRating.ModifiedDate = DateTime.UtcNow;
                            courseRating.IsActive = true;

                            decimal totalPoint = Convert.ToDecimal(((aPICourseRatingAndReview.OneStar * 1) + (aPICourseRatingAndReview.TwoStar * 2) + (aPICourseRatingAndReview.ThreeStar * 3) + (aPICourseRatingAndReview.FourStar * 4) + (aPICourseRatingAndReview.FiveStar * 5)));
                            decimal totalRatingUser = Convert.ToDecimal((aPICourseRatingAndReview.OneStar) + (aPICourseRatingAndReview.TwoStar) + (aPICourseRatingAndReview.ThreeStar) + (aPICourseRatingAndReview.FourStar) + (aPICourseRatingAndReview.FiveStar));
                            decimal totalAverage = Convert.ToDecimal(totalPoint / totalRatingUser);
                            courseRating.Average = totalAverage;

                            await _courseRatingRepository.Add(courseRating);

                            Model.CourseReview courseReview = new Model.CourseReview();
                            courseReview.CourseId = aPICourseRatingAndReview.CourseId;
                            courseReview.RatingId = courseRating.Id;
                            courseReview.ReviewRating = aPICourseRatingAndReview.ReviewRating;
                            courseReview.ReviewText = aPICourseRatingAndReview.ReviewText;
                            courseReview.UserId = UserId;
                            courseReview.CreatedBy = UserId;
                            courseReview.CreatedDate = DateTime.UtcNow;
                            courseReview.ModifiedBy = UserId;
                            courseReview.ModifiedDate = DateTime.UtcNow;
                            courseReview.IsActive = true;
                            courseReview.UseName = UserName;

                            await _courseReviewRepository.Add(courseReview);
                            if (!string.IsNullOrEmpty(courseReview.ReviewText))
                            {
                                string Category = RewardPointCategory.Normal;
                                string Condition = RewardPointCategory.RatedaCourse;
                                await this._rewardsPointRepository.AddRatingSubmitReward(UserId, courseRating.Id, courseRating.CourseId, OrganisationCode, Category, Condition);

                                string CategoryForReview = RewardPointCategory.Bonus;
                                string ConditionForReview = RewardPointCategory.RatedaReview;
                                await this._rewardsPointRepository.AddReviewSubmitReward(UserId, courseReview.Id, courseReview.CourseId, OrganisationCode, CategoryForReview, ConditionForReview);

                            }
                            else
                            {
                                string Category = RewardPointCategory.Normal;
                                string Condition = RewardPointCategory.RatedaCourse;
                                await this._rewardsPointRepository.AddRatingSubmitReward(UserId, courseRating.Id, courseRating.CourseId, OrganisationCode, Category, Condition);
                            }
                        }
                        else
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotCompleted), Description = EnumHelper.GetEnumDescription(MessageType.NotCompleted) });
                        }
                    }
                }
                return Ok(aPICourseRatingAndReview);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + " Message:" + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Model.CourseReview courseReview = await _courseReviewRepository.Get(DecryptedId);
                if (courseReview == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                courseReview.IsDeleted = true;
                await _courseReviewRepository.Update(courseReview);

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
