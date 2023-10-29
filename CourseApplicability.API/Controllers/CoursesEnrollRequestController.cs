using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using log4net;
using CourseApplicability.API.Model.Log_API_Count;
using static CourseApplicability.API.Common.TokenPermissions;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Common;
using CourseApplicability.API.Helper;
using CourseApplicability.API.Services;
using CourseApplicability.API.APIModel;
using CourseApplicability.API.Model;
using Courses.API.APIModel;

namespace CourseApplicability.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/a/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CoursesEnrollRequestController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CoursesEnrollRequestController));
        private ICoursesEnrollRequestRepository _coursesEnrollRequestRepository;
        private ICoursesEnrollRequestDetailsRepository _coursesEnrollRequestDetailsRepository;
        private readonly IAccessibilityRule _accessibilityRule;
        public CoursesEnrollRequestController(ICoursesEnrollRequestRepository coursesEnrollRequestRepository, ICoursesEnrollRequestDetailsRepository coursesEnrollRequestDetailsRepository, IIdentityService identitySvc, IAccessibilityRule accessibilityRule) : base(identitySvc)
        {
            this._coursesEnrollRequestRepository = coursesEnrollRequestRepository;
            this._coursesEnrollRequestDetailsRepository = coursesEnrollRequestDetailsRepository;
            _accessibilityRule = accessibilityRule;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var CoursesEnrollRequest = await this._coursesEnrollRequestRepository.GetAll();
                if (CoursesEnrollRequest == null)
                {
                    return NotFound();
                }
                return Ok(CoursesEnrollRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{orderBy?}/{order?}")]
        public IActionResult Get(int page, int pageSize, string orderBy = null, string order = null)
        {
            try
            {
                var CoursesEnrollRequest = this._coursesEnrollRequestRepository.GetAllAsync(page, pageSize, "id", (order != null && order.ToLower() == "asc") ? Order.ASC : Order.DESC);
                if (CoursesEnrollRequest == null)
                {
                    return NotFound();
                }
                return Ok(CoursesEnrollRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetCount")]
        public IActionResult GetCount()
        {
            try
            {
                int count = this._coursesEnrollRequestRepository.Count();

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetSupervisorCourseRequests")]
        public async Task<IActionResult> GetSupervisorCourseRequests([FromBody] GetSupervisorData getSupervisorData)
        {
            try
            {
                int EU_UserId = Convert.ToInt32(Security.DecryptForUI(getSupervisorData.UserId));
                APITotalRequest CoursesEnrollRequest = await this._coursesEnrollRequestRepository.GetSupervisorCourseRequests(getSupervisorData, EU_UserId);
                if (CoursesEnrollRequest == null)
                {
                    return NotFound();
                }
                return Ok(CoursesEnrollRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getStatus/{courseID}")]
        public async Task<IActionResult> GetStatus(int courseId)
        {
            try
            {
                return Ok(await this._coursesEnrollRequestRepository.GetStatus(UserId, courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostCourseRequest")]
        public async Task<IActionResult> PostCourseRequest([FromBody] APIUserCourseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _coursesEnrollRequestRepository.IsExist(request.CourseId, UserId, "Requested"))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }

                CoursesEnrollRequest course_req = new CoursesEnrollRequest();
                course_req.UserId = UserId;
                course_req.UserName = UserName;
                course_req.CourseId = request.CourseId;
                course_req.CourseTitle = request.CourseTitle;
                course_req.Date = DateTime.UtcNow;
                course_req.Status = "Requested";

                await this._coursesEnrollRequestRepository.Add(course_req);

                int courseRequestId = course_req.Id;

                CoursesEnrollRequestDetails course_reqDetails = new CoursesEnrollRequestDetails();
                course_reqDetails.CoursesEnrollRequestId = courseRequestId;
                course_reqDetails.ActionTakenBy = UserId;
                course_reqDetails.Reason = "";
                course_reqDetails.DateCreated = DateTime.UtcNow;
                course_reqDetails.Status = "Requested";

                await this._coursesEnrollRequestDetailsRepository.Add(course_reqDetails);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("EnrollCourseRequest/{id}/{userid}")]
        public async Task<IActionResult> EnrollCourseRequest(int id, int userId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                CoursesEnrollRequest req = await _coursesEnrollRequestRepository.Get(id);

                if (await _coursesEnrollRequestRepository.IsExist(req.CourseId, req.UserId, "Enrolled"))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }

                if (req == null) return NotFound();

                CoursesEnrollRequest course_req = new CoursesEnrollRequest();
                course_req.Id = req.Id;
                course_req.UserId = req.UserId;
                course_req.UserName = req.UserName;
                course_req.CourseId = req.CourseId;
                course_req.CourseTitle = req.CourseTitle;
                course_req.Date = DateTime.UtcNow;
                course_req.Status = "Enrolled";

                await this._coursesEnrollRequestRepository.Update(course_req);

                CoursesEnrollRequestDetails course_reqDetails = new CoursesEnrollRequestDetails();
                course_reqDetails.CoursesEnrollRequestId = req.Id;
                course_reqDetails.ActionTakenBy = UserId;
                course_reqDetails.Reason = "";
                course_reqDetails.DateCreated = DateTime.UtcNow;
                course_reqDetails.Status = "Enrolled";

                await this._coursesEnrollRequestDetailsRepository.Add(course_reqDetails);

                APIAccessibility apiAccessibility = new APIAccessibility();
                AccessibilityRules objsad = new AccessibilityRules
                {
                    AccessibilityRule = "UserId",
                    Condition = "AND",
                    ParameterValue = req.UserId.ToString(),

                };
                apiAccessibility.CourseId = req.CourseId;
                apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
                apiAccessibility.AccessibilityRule[0] = objsad;
                await _accessibilityRule.SelfEnroll(apiAccessibility, userId, OrgCode);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return Ok();
            }
        }

        [HttpPost("RejectCourseRequest/{id}")]
        public async Task<IActionResult> RejectCourseRequest(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                CoursesEnrollRequest req = await _coursesEnrollRequestRepository.Get(id);

                if (await _coursesEnrollRequestRepository.IsExist(req.CourseId, req.UserId, "Rejected"))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }

                if (req == null) return NotFound();

                CoursesEnrollRequest course_req = new CoursesEnrollRequest();
                course_req.Id = req.Id;
                course_req.UserId = req.UserId;
                course_req.UserName = req.UserName; ;
                course_req.CourseId = req.CourseId;
                course_req.CourseTitle = req.CourseTitle;
                course_req.Date = DateTime.UtcNow;
                course_req.Status = "Rejected";

                await this._coursesEnrollRequestRepository.Update(course_req);

                CoursesEnrollRequestDetails course_reqDetails = new CoursesEnrollRequestDetails();
                course_reqDetails.CoursesEnrollRequestId = req.Id;
                course_reqDetails.ActionTakenBy = UserId;
                course_reqDetails.Reason = "";
                course_reqDetails.DateCreated = DateTime.UtcNow;
                course_reqDetails.Status = "Rejected";

                await this._coursesEnrollRequestDetailsRepository.Add(course_reqDetails);


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest();
            }
        }



    }
}