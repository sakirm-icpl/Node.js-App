using AspNet.Security.OAuth.Introspection;
using com.pakhee.common;
using MyCourse.API.APIModel;
using MyCourse.API.Common;
using MyCourse.API.Helper;
using MyCourse.API.Helper.Metadata;
using MyCourse.API.Model;
using MyCourse.API.Model.Log_API_Count;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static MyCourse.API.Common.AuthorizePermissions;
using static MyCourse.API.Common.TokenPermissions;
using static MyCourse.API.Helper.Security;

namespace MyCourse.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ScormController : IdentityController
    {
        IScormVarRepository _scormRepository;
        IScormVarResultRepository _scormVarResultRepository;
        IContentCompletionStatus _contentCompletionStatus;
       // ICourseApplicability _courseApplicability;
        private readonly ITokensRepository _tokensRepository;
        private IConfiguration _configuration;
        private INodalCourseRequestsRepository _nodalCourseRequests;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ScormController));

        public ScormController(IScormVarRepository scormRepository, IScormVarResultRepository scormVarResultRepository,
            /*ICourseApplicability courseApplicability,*/ IContentCompletionStatus contentCompletionStatus,
            IIdentityService _identitySvc, ITokensRepository tokensRepository, IConfiguration configuration,
            INodalCourseRequestsRepository nodalCourseRequests) : base(_identitySvc)
        {
            _scormRepository = scormRepository;
            _scormVarResultRepository = scormVarResultRepository;
            _contentCompletionStatus = contentCompletionStatus;
          //  _courseApplicability = courseApplicability;
            this._tokensRepository = tokensRepository;
            _configuration = configuration;
            _nodalCourseRequests = nodalCourseRequests;
        }

        [HttpGet("Mobile/{courseId}/{moduleId}")]
        public async Task<IActionResult> GetScormMobile(int courseId, int moduleId)
        {
            try
            {
                var scormvar = await _scormRepository.GetScormForMobile(UserId, courseId, moduleId);

                if (scormvar == null)
                    return NotFound();


                return Ok(scormvar);
            }
            catch (Exception Ex)
            {
                _logger.Error(Utilities.GetDetailedException(Ex));
                return BadRequest(new ResponseMessage
                {
                    Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                    Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                });
            }
        }

        [HttpGet("{varName}/{courseId}/{moduleId}/{groupId?}")]
        public async Task<IActionResult> Get(string varname, int courseId, int moduleId, string groupId = null)
        {
            try
            {
                int? groupIdNew = null;
                if (string.IsNullOrEmpty(groupId) || groupId == "null" || groupId == "undefined" || groupId == "\"null\"")
                    groupIdNew = null;
                else
                    groupIdNew = Convert.ToInt32(groupId);

                _logger.Debug("In Route HTTPGET {varName}/{courseId}/{moduleId}");
                string value;
                if (varname == "cmi.core.student_name" || varname == "cmi.learner_name" || varname == "cmi.learner_id")
                {
                    value = UserName;
                }
                else if (varname == "cmi.core.student_id")
                {
                    value = LoginId.ToString();
                }
                else if (varname == "cmi.core.lesson_mode")
                {
                    value = "normal";
                }
                else if (varname == "cmi.core.entry")
                {
                    if (groupIdNew == null || groupIdNew == 0)
                    {
                        if (await _scormRepository.Count("cmi.suspend_data", UserId, courseId, moduleId) > 0)
                            return Ok("resume");
                        else
                            return Ok("ab-initio");
                    }
                    else
                    {
                        APIScormGroup aPIScormGroup = await _nodalCourseRequests.GetUserforCompletion((int)groupIdNew);
                        if (aPIScormGroup == null)
                            return NotFound();
                        if (await _scormRepository.Count("cmi.suspend_data", aPIScormGroup.UserId, courseId, moduleId) > 0)
                            return Ok("resume");
                        else
                            return Ok("ab-initio");
                    }
                }
                else
                {
                    if (groupIdNew == null || groupIdNew == 0)
                        value = await _scormRepository.GetScorm(varname, UserId, courseId, moduleId);
                    else
                    {
                        APIScormGroup aPIScormGroup = await _nodalCourseRequests.GetUserforCompletion((int)groupIdNew);
                        if (aPIScormGroup == null)
                            return NotFound();
                        value = await _scormRepository.GetScorm(varname, aPIScormGroup.UserId, courseId, moduleId);
                    }
                }

                if (value == null)
                {
                    return NotFound();
                }
                return Ok(value);
            }
            catch (Exception Ex)
            {
                _logger.Error(Utilities.GetDetailedException(Ex));
                return BadRequest(new ResponseMessage
                {
                    Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                    Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                });
            }
        }
        [HttpDelete]
        [PermissionRequired(Permissions.DeleteScormData)]
        public async Task<IActionResult> Delete([FromQuery]string userId, string courseId, string moduleId)
        {
            try
            {
                int DecrypteduserId = Convert.ToInt32(Security.Decrypt(userId));
                int DecryptedcourseId = Convert.ToInt32(Security.Decrypt(courseId));
                int DecryptedmoduleId = Convert.ToInt32(Security.Decrypt(moduleId));

                var result = await _scormRepository.DeleteScorm(DecrypteduserId, DecryptedcourseId, DecryptedmoduleId);


                if (result == "Records deleted successfully")
                    return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(MessageType.Success) });
                else if (result == "Provided data is not valid")
                    return StatusCode(409, "Dependency Exists, Course has already completed.");
                else
                    return StatusCode(400, "No Records for Deletion.");

            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string varname, string varvalue, int courseId, int moduleId, string groupId = null)
        {
            try
            {
                _logger.Debug("In Route HTTPPOST Scorm");
                if (varname != "cmi.suspend_data")
                {
                    varname = DecryptWithKey(varname, "m1n2o3p5q5r6s7t8");
                    varvalue = DecryptWithKey(varvalue, "m1n2o3p5q5r6s7t8");
                }

                int? groupIdNew = null;
                if (string.IsNullOrEmpty(groupId) || groupId == "null" || groupId == "undefined" || groupId == "\"null\"")
                    groupIdNew = null;
                else
                    groupIdNew = Convert.ToInt32(groupId);

                if (!ModelState.IsValid)
                {
                    _logger.Debug("ModelState is Invalid");
                    return BadRequest(ModelState);
                }

                Message Result = await this.PostScorm(varname, varvalue, courseId, moduleId, false, groupIdNew);
                return Ok();
            }
            catch (Exception Ex)
            {
                _logger.Error(Utilities.GetDetailedException(Ex));
                return BadRequest(new ResponseMessage
                {
                    Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                    Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                });
            }
        }


        [HttpPost("MobileV3")]
        public async Task<IActionResult> ScormMobilePostV3([FromBody] List<ApiScormPost> ScormList)
        {
            try
            {
                if (!IsiOS)
                    ScormList.Reverse();
                if (ScormList != null)
                    _logger.Error("In Route MobileV3 :-" + ScormList.Count);
                foreach (ApiScormPost scorm in ScormList)
                {
                    await this.PostScormVars(scorm.VarName.ToString(), scorm.VarValue.ToString(), scorm.CourseId, scorm.ModuleId, true);
                }
                foreach (ApiScormPost scorm in ScormList)
                {
                    await this.PostScormVarsResult(scorm.VarName.ToString(), scorm.VarValue.ToString(), scorm.CourseId, scorm.ModuleId, true);
                }
                return Ok();
            }
            catch (Exception Ex)
            {
                _logger.Error("Exception for updating scorm data for User :-" + UserId + Utilities.GetDetailedException(Ex) + " Organisation :-" + OrganisationCode);
                try
                {
                    foreach (ApiScormPost scorm in ScormList)
                    {
                        _logger.Error(string.Format("VarName = {0} VarValue ={1} CourseId = {2} ModuleId = {3}", scorm.VarName.ToString(), scorm.VarValue.ToString(), scorm.CourseId, scorm.ModuleId));
                    }
                }
                catch (Exception ex)
                { _logger.Error(Utilities.GetDetailedException(ex)); }
                return BadRequest(new ResponseMessage
                {
                    Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                    Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                });
            }
        }


        [HttpPost("Mobile")]
        public async Task<IActionResult> ScormMobilePost([FromBody] List<ApiScormPost> ScormList)
        {
            try
            {
                if (!IsiOS)
                    ScormList.Reverse();

                foreach (ApiScormPost scorm in ScormList)
                {
                    string Key = "a1b2c3d4e5f6g7h8";
                    string _initVector = "1234123412341234";

                    if (!IsiOS)
                    {

                        await this.PostScormVars(DecryptForUI(scorm.VarName.ToString()), DecryptForUI(scorm.VarValue.ToString()), scorm.CourseId, scorm.ModuleId, true);
                    }
                    else
                        await this.PostScormVars(CryptLib.decrypt((scorm.VarName), Key, _initVector), CryptLib.decrypt((scorm.VarValue), Key, _initVector), scorm.CourseId, scorm.ModuleId, true);
                }

                foreach (ApiScormPost scorm in ScormList)
                {
                    string Key = "a1b2c3d4e5f6g7h8";
                    string _initVector = "1234123412341234";

                    if (!IsiOS)
                        await this.PostScormVarsResult(DecryptForUI(scorm.VarName.ToString()), DecryptForUI(scorm.VarValue.ToString()), scorm.CourseId, scorm.ModuleId, true);
                    else
                        await this.PostScormVarsResult(CryptLib.decrypt((scorm.VarName), Key, _initVector), CryptLib.decrypt((scorm.VarValue), Key, _initVector), scorm.CourseId, scorm.ModuleId, true);
                }
                return Ok();
            }
            catch (Exception Ex)
            {
                _logger.Error(Utilities.GetDetailedException(Ex));
                return BadRequest(new ResponseMessage
                {
                    Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                    Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                });
            }
        }

        [HttpPost("MobileV2")]
        public async Task<IActionResult> ScormMobilePostV2([FromBody] List<ApiScormPost> ScormList)
        {
            try
            {
                string Key = "a1b2c3d4e5f6g7h8";
                string _initVector = "1234123412341234";

                if (!IsiOS)
                    ScormList.Reverse();

                foreach (ApiScormPost scorm in ScormList)
                {
                    if (scorm.VarName.ToString() == "cmi.suspend_data")
                        await this.PostScormVars(scorm.VarName.ToString(), scorm.VarValue.ToString(), scorm.CourseId, scorm.ModuleId, true);
                    else
                    {
                        if (!IsiOS)
                            await this.PostScormVars(DecryptForUI(scorm.VarName.ToString()), DecryptForUI(scorm.VarValue.ToString()), scorm.CourseId, scorm.ModuleId, true);
                        else
                            await this.PostScormVars(CryptLib.decrypt((scorm.VarName), Key, _initVector), CryptLib.decrypt((scorm.VarValue), Key, _initVector), scorm.CourseId, scorm.ModuleId, true);
                    }
                }

                foreach (ApiScormPost scorm in ScormList)
                {
                    if (scorm.VarName.ToString() == "cmi.suspend_data")
                        await this.PostScormVarsResult(scorm.VarName.ToString(), scorm.VarValue.ToString(), scorm.CourseId, scorm.ModuleId, true);

                    if (!IsiOS)
                        await this.PostScormVarsResult(DecryptForUI(scorm.VarName.ToString()), DecryptForUI(scorm.VarValue.ToString()), scorm.CourseId, scorm.ModuleId, true);
                    else
                        await this.PostScormVarsResult(CryptLib.decrypt((scorm.VarName), Key, _initVector), CryptLib.decrypt((scorm.VarValue), Key, _initVector), scorm.CourseId, scorm.ModuleId, true);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage
                {
                    Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                    Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                });
            }
        }


        private async Task<Message> PostScorm(string varname, string varvalue, int courseId, int moduleId, bool isBulk = false, int? groupId = null)
        {
            if (groupId == null || groupId == 0)
            {
                APIScorm scorm = new APIScorm();
                scorm.VarName = varname;
                scorm.VarValue = varvalue;
                scorm.CourseId = courseId;
                scorm.ModuleId = moduleId;
                bool IsContentCompleted = await _contentCompletionStatus.IsContentCompleted(UserId, scorm.CourseId, scorm.ModuleId);

                ScormVars ScormVars = new ScormVars();
                ScormVars.VarName = scorm.VarName;
                ScormVars.VarValue = scorm.VarValue;
                ScormVars.UserId = UserId;
                ScormVars.ModuleId = scorm.ModuleId;
                ScormVars.CourseId = scorm.CourseId;
                ScormVars.ModifiedDate = DateTime.UtcNow;
                ScormVars.CreatedDate = DateTime.UtcNow;
                ScormVars.Id = 0;
                await _scormRepository.Add(ScormVars);

                if (!IsContentCompleted || (scorm.VarName == ScormVarName.cmi_core_score_raw || scorm.VarName == ScormVarName.cmi_score_raw))
                {
                    int Count = await _scormVarResultRepository.Count(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                    if (Count == 0)
                    {
                        ScormVarResult Result = new ScormVarResult();
                        Result.CourseId = ScormVars.CourseId;
                        Result.ModuleId = ScormVars.ModuleId;
                        Result.Result = Status.InProgress;
                        Result.NoOfAttempts = 0;
                        Result.IsDeleted = false;
                        Result.UserId = ScormVars.UserId;
                        await _scormVarResultRepository.Add(Result);

                        if (isBulk == false)
                        {
                            ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                            ContentCompletionStatus.CourseId = ScormVars.CourseId;
                            ContentCompletionStatus.ModuleId = ScormVars.ModuleId;
                            ContentCompletionStatus.UserId = ScormVars.UserId;
                            ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                            ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                            ContentCompletionStatus.Status = Status.InProgress;
                            await _contentCompletionStatus.Post(ContentCompletionStatus, null, null, OrganisationCode);
                        }
                    }
                    else if (scorm.VarName == ScormVarName.cmi_core_score_raw || scorm.VarName == ScormVarName.cmi_score_raw)
                    {
                        ScormVarResult Result = new ScormVarResult();
                        Result.CourseId = ScormVars.CourseId;
                        Result.ModuleId = ScormVars.ModuleId;
                        Result.Score = scorm.VarValue == null ? (float?)null : float.Parse(scorm.VarValue);
                        Result.UserId = ScormVars.UserId;
                        Result.ModifiedDate = DateTime.UtcNow;
                        Result.CreatedDate = DateTime.UtcNow;
                        string res = await _scormVarResultRepository.GetResult(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                        if (!string.IsNullOrEmpty(res))
                        {
                            Result.NoOfAttempts = 1;
                            var ScormResult = await _scormVarResultRepository.Get(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                            Result.Id = ScormResult.Id;
                            await _scormVarResultRepository.Update(Result);
                        }
                        else
                        {
                            Result.NoOfAttempts = 1;
                            await _scormVarResultRepository.Add(Result);
                        }
                    }
                    else if ((scorm.VarName == ScormVarName.cmi_core_lesson_status || scorm.VarName == ScormVarName.cmi_completion_status) && (scorm.VarValue != Status.Incomplete)) // || scorm.VarValue != Status.Incompleted
                    {
                        string Score = await _scormVarResultRepository.GetScore(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                        if (string.IsNullOrEmpty(Score))
                        {
                            ScormVarResult Result = new ScormVarResult();
                            Result.CourseId = ScormVars.CourseId;
                            Result.ModuleId = ScormVars.ModuleId;
                            Result.Result = scorm.VarValue;
                            Result.NoOfAttempts = 1;
                            Result.UserId = ScormVars.UserId;
                            Result.ModifiedDate = DateTime.UtcNow;
                            Result.CreatedDate = DateTime.UtcNow;
                            await _scormVarResultRepository.Add(Result);
                        }
                        else
                        {
                            var ScormResult = await _scormVarResultRepository.Get(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                            ScormVarResult res = new ScormVarResult();
                            res = await _scormVarResultRepository.Get(ScormResult.Id);
                            res.ModifiedDate = DateTime.UtcNow;
                            res.Result = varvalue;
                            res.Id = ScormResult.Id;
                            await _scormVarResultRepository.Update(res);
                        }
                        if ((scorm.VarName == ScormVarName.cmi_core_lesson_status || scorm.VarName == ScormVarName.cmi_completion_status) &&
                            scorm.VarValue == Status.Completed || scorm.VarValue == Status.Complete ||
                            scorm.VarValue == Status.Pass || scorm.VarValue == Status.Passed ||
                            scorm.VarValue == Status.Failed || scorm.VarValue == Status.Fail)
                        {
                            ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                            ContentCompletionStatus.CourseId = ScormVars.CourseId;
                            ContentCompletionStatus.ModuleId = ScormVars.ModuleId;
                            ContentCompletionStatus.UserId = ScormVars.UserId;
                            ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                            ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                            if (scorm.VarValue == Status.Failed || scorm.VarValue == Status.Fail)
                                ContentCompletionStatus.Status = Status.InProgress;
                            else
                                ContentCompletionStatus.Status = Status.Completed;
                            await _contentCompletionStatus.Post(ContentCompletionStatus, null, null, OrganisationCode);
                        }
                    }
                    else if ((scorm.VarName == ScormVarName.cmi_core_lesson_status || scorm.VarName == ScormVarName.cmi_completion_status) && (scorm.VarValue == Status.Incomplete || scorm.VarValue == Status.Incompleted))
                    {
                        if (!IsContentCompleted)
                        {
                            ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                            ContentCompletionStatus.CourseId = ScormVars.CourseId;
                            ContentCompletionStatus.ModuleId = ScormVars.ModuleId;
                            ContentCompletionStatus.UserId = ScormVars.UserId;
                            ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                            ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                            ContentCompletionStatus.Status = Status.InProgress;
                            await _contentCompletionStatus.Post(ContentCompletionStatus);
                        }
                    }
                }
            }
            else
            {
                APIScormGroup aPIScormGroups = await _nodalCourseRequests.GetUserforCompletion((int)groupId);

                APIScorm scorm = new APIScorm();
                scorm.VarName = varname;
                scorm.VarValue = varvalue;
                scorm.CourseId = courseId;
                scorm.ModuleId = moduleId;
                bool IsContentCompleted = await _contentCompletionStatus.IsContentCompleted(aPIScormGroups.UserId, scorm.CourseId, scorm.ModuleId);

                ScormVars ScormVars = new ScormVars();
                ScormVars.VarName = scorm.VarName;
                ScormVars.VarValue = scorm.VarValue;
                ScormVars.UserId = aPIScormGroups.UserId;
                ScormVars.ModuleId = scorm.ModuleId;
                ScormVars.CourseId = scorm.CourseId;
                ScormVars.ModifiedDate = DateTime.UtcNow;
                ScormVars.CreatedDate = DateTime.UtcNow;
                ScormVars.Id = 0;
                await _scormRepository.Add(ScormVars);

                if (!IsContentCompleted || (scorm.VarName == ScormVarName.cmi_core_score_raw || scorm.VarName == ScormVarName.cmi_score_raw))
                {
                    int Count = await _scormVarResultRepository.Count(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                    if (Count == 0)
                    {
                        ScormVarResult Result = new ScormVarResult();
                        Result.CourseId = ScormVars.CourseId;
                        Result.ModuleId = ScormVars.ModuleId;
                        Result.Result = Status.InProgress;
                        Result.NoOfAttempts = 0;
                        Result.IsDeleted = false;
                        Result.UserId = ScormVars.UserId;
                        await _scormVarResultRepository.Add(Result);

                        if (isBulk == false)
                        {
                            ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                            ContentCompletionStatus.CourseId = ScormVars.CourseId;
                            ContentCompletionStatus.ModuleId = ScormVars.ModuleId;
                            ContentCompletionStatus.UserId = ScormVars.UserId;
                            ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                            ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                            ContentCompletionStatus.Status = Status.InProgress;
                            await _contentCompletionStatus.Post(ContentCompletionStatus, null, null, OrganisationCode);
                        }
                    }
                    else if (scorm.VarName == ScormVarName.cmi_core_score_raw || scorm.VarName == ScormVarName.cmi_score_raw)
                    {
                        ScormVarResult Result = new ScormVarResult();
                        Result.CourseId = ScormVars.CourseId;
                        Result.ModuleId = ScormVars.ModuleId;
                        Result.Score = scorm.VarValue == null ? (float?)null : float.Parse(scorm.VarValue);
                        Result.UserId = ScormVars.UserId;
                        Result.ModifiedDate = DateTime.UtcNow;
                        Result.CreatedDate = DateTime.UtcNow;
                        string res = await _scormVarResultRepository.GetResult(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                        if (!string.IsNullOrEmpty(res))
                        {
                            Result.NoOfAttempts = 1;
                            var ScormResult = await _scormVarResultRepository.Get(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                            Result.Id = ScormResult.Id;
                            await _scormVarResultRepository.Update(Result);
                        }
                        else
                        {
                            Result.NoOfAttempts = 1;
                            await _scormVarResultRepository.Add(Result);
                        }
                    }
                    else if ((scorm.VarName == ScormVarName.cmi_core_lesson_status || scorm.VarName == ScormVarName.cmi_completion_status) && (scorm.VarValue != Status.Incomplete)) // || scorm.VarValue != Status.Incompleted
                    {
                        string Score = await _scormVarResultRepository.GetScore(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                        if (string.IsNullOrEmpty(Score))
                        {
                            ScormVarResult Result = new ScormVarResult();
                            Result.CourseId = ScormVars.CourseId;
                            Result.ModuleId = ScormVars.ModuleId;
                            Result.Result = scorm.VarValue;
                            Result.NoOfAttempts = 1;
                            Result.UserId = ScormVars.UserId;
                            Result.ModifiedDate = DateTime.UtcNow;
                            Result.CreatedDate = DateTime.UtcNow;
                            await _scormVarResultRepository.Add(Result);
                        }
                        else
                        {
                            var ScormResult = await _scormVarResultRepository.Get(ScormVars.UserId, ScormVars.CourseId, ScormVars.ModuleId);
                            ScormVarResult res = new ScormVarResult();
                            res = await _scormVarResultRepository.Get(ScormResult.Id);
                            res.ModifiedDate = DateTime.UtcNow;
                            res.Result = varvalue;
                            res.Id = ScormResult.Id;
                            await _scormVarResultRepository.Update(res);
                        }
                        if ((scorm.VarName == ScormVarName.cmi_core_lesson_status || scorm.VarName == ScormVarName.cmi_completion_status) &&
                            scorm.VarValue == Status.Completed || scorm.VarValue == Status.Complete ||
                            scorm.VarValue == Status.Pass || scorm.VarValue == Status.Passed ||
                            scorm.VarValue == Status.Failed || scorm.VarValue == Status.Fail)
                        {
                            ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                            ContentCompletionStatus.CourseId = ScormVars.CourseId;
                            ContentCompletionStatus.ModuleId = ScormVars.ModuleId;
                            ContentCompletionStatus.UserId = ScormVars.UserId;
                            ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                            ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                            //if (scorm.VarValue == Status.Failed || scorm.VarValue == Status.Fail)
                            ContentCompletionStatus.Status = Status.InProgress;
                            //else
                            //ContentCompletionStatus.Status = Status.Completed;
                            await _contentCompletionStatus.Post(ContentCompletionStatus, null, null, OrganisationCode);
                        }
                    }
                    else if ((scorm.VarName == ScormVarName.cmi_core_lesson_status || scorm.VarName == ScormVarName.cmi_completion_status) && (scorm.VarValue == Status.Incomplete || scorm.VarValue == Status.Incompleted))
                    {
                        if (!IsContentCompleted)
                        {
                            ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                            ContentCompletionStatus.CourseId = ScormVars.CourseId;
                            ContentCompletionStatus.ModuleId = ScormVars.ModuleId;
                            ContentCompletionStatus.UserId = ScormVars.UserId;
                            ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                            ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                            ContentCompletionStatus.Status = Status.InProgress;
                            await _contentCompletionStatus.Post(ContentCompletionStatus);
                        }
                    }
                }
            }
            return Message.Ok;
        }


        private async Task<Message> PostScormVarsResult(string varname, string varvalue, int courseId, int moduleId, bool isBulk = false)
        {
            bool IsContentCompleted = await _contentCompletionStatus.IsContentCompleted(UserId, courseId, moduleId);
            if (!IsContentCompleted || (varname == ScormVarName.cmi_core_score_raw || varname == ScormVarName.cmi_score_raw))
            {
                int Count = await _scormVarResultRepository.Count(UserId, courseId, moduleId);
                if (Count == 0)
                {
                    ScormVarResult Result = new ScormVarResult();
                    Result.CourseId = courseId;
                    Result.ModuleId = moduleId;
                    Result.Result = Status.InProgress;
                    Result.NoOfAttempts = 0;
                    Result.IsDeleted = false;
                    Result.UserId = UserId;
                    await _scormVarResultRepository.Add(Result);

                    if (isBulk == false)
                    {
                        ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                        ContentCompletionStatus.CourseId = courseId;
                        ContentCompletionStatus.ModuleId = moduleId;
                        ContentCompletionStatus.UserId = UserId;
                        ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        ContentCompletionStatus.Status = Status.InProgress;
                        await _contentCompletionStatus.Post(ContentCompletionStatus);
                    }
                }
                else if (varname == ScormVarName.cmi_core_score_raw || varname == ScormVarName.cmi_score_raw)
                {
                    ScormVarResult Result = new ScormVarResult();
                    Result.CourseId = courseId;
                    Result.ModuleId = moduleId;
                    Result.Score = varvalue == null ? (float?)null : float.Parse(varvalue);
                    Result.UserId = UserId;
                    Result.ModifiedDate = DateTime.UtcNow;
                    Result.CreatedDate = DateTime.UtcNow;
                    string res = await _scormVarResultRepository.GetResult(UserId, courseId, moduleId);
                    if (!string.IsNullOrEmpty(res))
                    {
                        Result.NoOfAttempts = 1;
                        var ScormResult = await _scormVarResultRepository.Get(UserId, courseId, moduleId);
                        Result.Id = ScormResult.Id;
                        await _scormVarResultRepository.Update(Result);
                    }
                    else
                    {
                        Result.NoOfAttempts = 1;
                        await _scormVarResultRepository.Add(Result);
                    }
                }
                else if ((varname == ScormVarName.cmi_core_lesson_status || varname == ScormVarName.cmi_completion_status) && (varvalue != Status.Incomplete)) //|| varvalue != Status.Incompleted
                {

                    string Score = await _scormVarResultRepository.GetScore(UserId, courseId, moduleId);
                    if (string.IsNullOrEmpty(Score))
                    {
                        ScormVarResult Result = new ScormVarResult();
                        Result.CourseId = courseId;
                        Result.ModuleId = moduleId;
                        Result.Result = varvalue;
                        Result.NoOfAttempts = 1;
                        Result.UserId = UserId;
                        Result.ModifiedDate = DateTime.UtcNow;
                        Result.CreatedDate = DateTime.UtcNow;
                        await _scormVarResultRepository.Add(Result);
                    }
                    else
                    {
                        var ScormResult = await _scormVarResultRepository.Get(UserId, courseId, moduleId);
                        ScormVarResult res = new ScormVarResult();

                        res = await _scormVarResultRepository.Get(ScormResult.Id);
                        res.ModifiedDate = DateTime.UtcNow;
                        res.Result = varvalue;
                        res.Id = ScormResult.Id;
                        await _scormVarResultRepository.Update(res);
                    }
                    if ((varname == ScormVarName.cmi_core_lesson_status || varname == ScormVarName.cmi_completion_status) &&
                        varvalue == Status.Completed || varvalue == Status.Complete ||
                        varvalue == Status.Pass || varvalue == Status.Passed ||
                        varvalue == Status.Failed || varvalue == Status.Fail)
                    {
                        ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                        ContentCompletionStatus.CourseId = courseId;
                        ContentCompletionStatus.ModuleId = moduleId;
                        ContentCompletionStatus.UserId = UserId;
                        ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        if (varvalue == Status.Failed || varvalue == Status.Fail)
                            ContentCompletionStatus.Status = Status.InProgress;
                        else
                            ContentCompletionStatus.Status = Status.Completed;
                        ContentCompletionStatus.UserId = UserId;
                        await _contentCompletionStatus.Post(ContentCompletionStatus);
                    }
                }

                else if ((varname == ScormVarName.cmi_core_lesson_status || varname == ScormVarName.cmi_completion_status) && (varvalue == Status.Incomplete || varvalue == Status.Incompleted))
                {

                    if (!IsContentCompleted)
                    {
                        ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                        ContentCompletionStatus.CourseId = courseId;
                        ContentCompletionStatus.ModuleId = moduleId;
                        ContentCompletionStatus.UserId = UserId;
                        ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        ContentCompletionStatus.Status = Status.InProgress;
                        await _contentCompletionStatus.Post(ContentCompletionStatus);
                    }

                }
            }

            return Message.Ok;
        }


        private async Task<Message> PostScormVars(string varname, string varvalue, int courseId, int moduleId, bool isBulk = false)
        {
            ScormVars ScormVars = new ScormVars();
            ScormVars.VarName = varname;
            ScormVars.VarValue = varvalue;
            ScormVars.UserId = UserId;
            ScormVars.ModuleId = moduleId;
            ScormVars.CourseId = courseId;
            ScormVars.ModifiedDate = DateTime.UtcNow;
            ScormVars.CreatedDate = DateTime.UtcNow;
            ScormVars.Id = 0;
            await _scormRepository.Add(ScormVars);
            return Message.Ok;
        }

        [HttpPost("ClearBookmarking")]
        [PermissionRequired(Permissions.ClearBookmarking)]
        public async Task<IActionResult> ClearScormData([FromBody] APIClearScorm obj)
        {
            if (ModelState.IsValid)
            {
                int UserID;
                try
                {
                    try
                    {
                        UserID = Convert.ToInt32(Security.DecryptForUI(obj.UserID.ToString()));
                    }
                    catch(Exception ex)
                    {
                        _logger.Error("Invalid UserID " + Utilities.GetDetailedException(ex));
                        return this.BadRequest(new ResponseMessage { Message = "Invalid Data", Description = "Please Enter Valid UserID", StatusCode = 400 });
                    }
                    int ModifiedBy = UserId;
                    DateTime ModifiedDate = DateTime.UtcNow;
                    var result = await _scormRepository.ClearBookmarkingData(obj, ModifiedBy, ModifiedDate, UserID);
                    if (result == "Records deleted successfully")
                        return Ok(new ResponseMessage { Message = "success", Description = "Records Saved successfully", StatusCode = 200 });
                    else if (result == "Provided data is not valid")
                        return Ok(new ResponseMessage { Message = "Conflict", Description = "Dependency Exists, Course is already completed by the user.", StatusCode = 409 });
                    else
                        return Ok(new ResponseMessage { Message = "Invalid Data", Description = "No Records for Deletion available.", StatusCode = 400 });
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
            else
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }

        [HttpGet("GetModulesByCourseID/{search?}")]
        [PermissionRequired(Permissions.ClearBookmarking)]
        public async Task<IActionResult> GetModuleName(int search)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._scormRepository.GetModules(search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("ExportBookmarkingClearData")]
        [PermissionRequired(Permissions.ClearBookmarking)]
        public async Task<IActionResult> ExportBookmarkingClearData()
        {
            try
            {
                var Data = await this._scormRepository.GetAllBookmarkingClearData();
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "ClearBookMarkingData.xlsx";
                string URL = string.Concat(DomainName, "/", sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ClearBookMarkingData");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Course Code";
                    worksheet.Cells[1, 2].Value = "Course Title";
                    worksheet.Cells[1, 3].Value = "Module Name";
                    worksheet.Cells[1, 4].Value = "UserID";
                    worksheet.Cells[1, 5].Value = "ModifiedBy";
                    worksheet.Cells[1, 6].Value = "ModifiedDate";
                    int row = 2, column = 1;
                    foreach (APIBookMarkingData aPIBookMarkingData in Data)
                    {
                        DateTime DateValue = new DateTime();

                        worksheet.Cells[row, column++].Value = aPIBookMarkingData.CourseCode;
                        worksheet.Cells[row, column++].Value = aPIBookMarkingData.CourseName;
                        worksheet.Cells[row, column++].Value = aPIBookMarkingData.ModuleName;
                        worksheet.Cells[row, column++].Value = aPIBookMarkingData.UserID;
                        worksheet.Cells[row, column++].Value = aPIBookMarkingData.ModifiedBy;
                        DateValue = Convert.ToDateTime(aPIBookMarkingData.ModifiedDate);
                        worksheet.Cells[row, column++].Value = DateValue.ToString("yyyy-MM-dd");
                        row++;
                        column = 1;
                    }
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                    }
                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetBookmarkingClearData/{page?}/{pageSize?}")]
        [Produces(typeof(List<APIBookMarkingData>))]
        [PermissionRequired(Permissions.ClearBookmarking)]
        public async Task<IActionResult> GetBookmarkingClearData(int? page = null, int? pageSize = null)
        {
            try
            {
                List<APIBookMarkingData> course = await _scormRepository.GetClearBookMarkingViewData(page, pageSize);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetBookmarkingClearDataCount")]
        [PermissionRequired(Permissions.ClearBookmarking)]
        public async Task<IActionResult> GetBookmarkingClearDataCount()
        {
            try
            {
                int count = await _scormRepository.GetClearBookMarkingViewDataCount();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}