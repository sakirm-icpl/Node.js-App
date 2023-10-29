using Newtonsoft.Json.Linq;
using System.Data;
using log4net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Assessment.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data.SqlClient;
using Assessment.API.Models;
using Assessment.API.APIModel;
using Assessment.API.Common;
using Assessment.API.Helper;
using Assessment.API.Model;
using Newtonsoft.Json;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;

namespace Assessment.API.Repositories
{
    public class MyCoursesRepository : IMyCoursesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MyCoursesRepository));
        private AssessmentContext _db;
        private readonly IConfiguration _configuration;
        private readonly List<TimeZoneList> _tzList;


        ICustomerConnectionStringRepository _customerConnection;
        IAccessibilityRule _accessibilityRule;
        INodalCourseRequestsRepository _nodalCourseRequests;
        IAzureStorage _azurestorage;
        public MyCoursesRepository(AssessmentContext context,
            IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnection, IAccessibilityRule AccessibilityRule, IAzureStorage azurestorage,
        INodalCourseRequestsRepository nodalCourseRequests)
        {
            this._db = context;
            this._configuration = configuration;
            this._customerConnection = customerConnection;
            _accessibilityRule = AccessibilityRule;
            _nodalCourseRequests = nodalCourseRequests;
            _azurestorage = azurestorage;
            // get system time zone
            var tzs = TimeZoneInfo.GetSystemTimeZones();
            _tzList = tzs.Select(tz => new TimeZoneList
            {
                Text = tz.DisplayName,
                Value = tz.Id
            }).ToList();
        }

        public async Task<int> GetUserDetailsByUserID(string userId)
        {
            int UserId = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetUserDetailsByUserID";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            UserId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return UserId;
        }
        public async Task<APIMyCoursesModule> GetModule(int userId, int courseId, string orgcode, int? groupId = null)
        {
            string Status = string.Empty;
            if (groupId != null && groupId != 0)
            {
                APIScormGroup aPIScormGroup = await _nodalCourseRequests.GetUserforCompletion((int)groupId);
                if (aPIScormGroup == null)
                    return null;
                userId = aPIScormGroup.UserId;
                Status = aPIScormGroup.Status;
            }

            APIMyCoursesModule CourseInfo = await GetModulesFromDb(userId, courseId, orgcode);
            List<int> ModuleId = CourseInfo.Modules.Select(a => a.ModuleId).ToList();
            #region "Code added to add information related to BigBlueButton Meeting details"
            try
            {
                if (CourseInfo.ModuleSequence != null)
                {
                    #region "Code to handle multiple schedules"
                    try
                    {
                        SqlParameter[] spParameters = new SqlParameter[2];
                        spParameters[0] = new SqlParameter("@USERID", SqlDbType.Int) { Value = userId };
                        spParameters[1] = new SqlParameter("@COURSEID", SqlDbType.Int) { Value = courseId };


                        DataTable dataTable = await ExecuteStoredProcedure("[dbo].[uspGetBigBlueMeetingSchedule]", spParameters);
                        int index = 0;
                        if (dataTable != null)
                        {
                            foreach (var item in CourseInfo.ModuleSequence)
                            {
                                DataRow[] dataRow = dataTable.Select("ModuleId=" + item.ModuleId);
                                if (dataRow != null)
                                {
                                    CourseInfo.ModuleSequence[index].BBBmeetingDetails = new List<BBMeetingDetails>();
                                    for (int i = 0; i < dataRow.Length; i++)
                                    {

                                        int elapsedMinutesInDay = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalMinutes);
                                        int meetingDuration = Convert.ToInt32(DateTime.Parse((string)dataRow[i]["MeetingTime"]).TimeOfDay.TotalMinutes + (int)dataRow[i]["Duration"]);
                                        bool isMeetingExpired = true;
                                        if (Convert.ToDateTime(dataRow[i]["MeetingDate"]).Date == DateTime.Now.Date)
                                        {
                                            if (meetingDuration >= elapsedMinutesInDay)
                                                isMeetingExpired = false;
                                        }
                                        else if (Convert.ToDateTime(dataRow[i]["MeetingDate"]) > DateTime.Now.Date)
                                            isMeetingExpired = false;
                                        CourseInfo.ModuleSequence[index].BBBmeetingDetails.Add(new BBMeetingDetails { MeetingID = Convert.ToString(dataRow[i]["MeetingID"]), MeetingName = Convert.ToString(dataRow[i]["MeetingName"]), MeetingTime = Convert.ToString(dataRow[i]["MeetingTime"]), MeetingDate = Convert.ToDateTime(dataRow[i]["MeetingDate"]), CourseId = Convert.ToInt32(dataRow[i]["CourseId"]), MeetingExpired = isMeetingExpired, Duration = Convert.ToInt32(dataRow[i]["Duration"]) });

                                    }
                                }

                                index++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            #endregion
            if (CourseInfo.CourseType == "vilt")
            {
                try
                {
                    if (CourseInfo.ModuleSequence != null)
                    {
                        #region "Code to handle multiple schedules"
                        try
                        {
                            int index = 0;
                            for (int j = 0; j < ModuleId.Count; j++)
                            {

                                SqlParameter[] spParameters = new SqlParameter[3];
                                spParameters[0] = new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId };
                                spParameters[1] = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
                                spParameters[2] = new SqlParameter("@ModuleId", SqlDbType.Int) { Value = ModuleId[j] };

                                DataTable dataTable = await ExecuteStoredProcedure("[dbo].[GetZoomMeetingSchedule]", spParameters);

                                if (dataTable != null)
                                {
                                    CourseInfo.ModuleSequence[index].apizooms = new List<APIZoom>();
                                    for (int i = 0; i < dataTable.Rows.Count; i++)
                                    {
                                        APIZoom zoom = new APIZoom();
                                        CourseInfo.ModuleSequence[index].apizooms.Add(new APIZoom
                                        {
                                            is_zoom_created = Convert.ToString(dataTable.Rows[i]["is_zoom_created"]),
                                            zoom_link = Convert.ToString(dataTable.Rows[i]["zoom_link"]),
                                            zoom_name = Convert.ToString(dataTable.Rows[i]["zoom_name"]),
                                            ID = Convert.ToInt32(dataTable.Rows[i]["ID"]),
                                            ScheduleCode = Convert.ToString(dataTable.Rows[i]["ScheduleCode"]),
                                            StartDate = Convert.ToString(dataTable.Rows[i]["StartDate"]),
                                            StartTime = Convert.ToString(dataTable.Rows[i]["StartTime"]),
                                            EndDate = Convert.ToString(dataTable.Rows[i]["EndDate"]),
                                            EndTime = Convert.ToString(dataTable.Rows[i]["EndTime"]),
                                            ScheduleTime = Convert.ToString(dataTable.Rows[i]["ScheduleTime"]),
                                            EnableSchedule = Convert.ToBoolean(dataTable.Rows[i]["EnableSchedule"])

                                        });// index++;
                                    }
                                }
                                index++;
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                #endregion
                try
                {
                    if (CourseInfo.ModuleSequence != null)
                    {
                        try
                        {
                            int index = 0;
                            for (int j = 0; j < ModuleId.Count; j++)
                            {

                                SqlParameter[] spParameters = new SqlParameter[3];
                                spParameters[0] = new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId };
                                spParameters[1] = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
                                spParameters[2] = new SqlParameter("@ModuleId", SqlDbType.Int) { Value = ModuleId[j] };

                                DataTable dataTable = await ExecuteStoredProcedure("[dbo].[GetTeamsMeetingSchedule]", spParameters);

                                if (dataTable != null)
                                {
                                    CourseInfo.ModuleSequence[index].apiteams = new List<APITeams>();
                                    for (int i = 0; i < dataTable.Rows.Count; i++)
                                    {
                                        APITeams zoom = new APITeams();
                                        CourseInfo.ModuleSequence[index].apiteams.Add(new APITeams
                                        {
                                            is_teams_created = Convert.ToString(dataTable.Rows[i]["is_teams_created"]),
                                            teams_link = Convert.ToString(dataTable.Rows[i]["teams_link"]),
                                            teams_name = Convert.ToString(dataTable.Rows[i]["teams_name"]),
                                            ID = Convert.ToInt32(dataTable.Rows[i]["ID"]),
                                            ScheduleCode = Convert.ToString(dataTable.Rows[i]["ScheduleCode"]),
                                            StartDate = Convert.ToString(dataTable.Rows[i]["StartDate"]),
                                            StartTime = Convert.ToString(dataTable.Rows[i]["StartTime"]),
                                            EndDate = Convert.ToString(dataTable.Rows[i]["EndDate"]),
                                            EndTime = Convert.ToString(dataTable.Rows[i]["EndTime"]),
                                            ScheduleTime = Convert.ToString(dataTable.Rows[i]["ScheduleTime"]),
                                            EnableSchedule = Convert.ToBoolean(dataTable.Rows[i]["EnableSchedule"])

                                        });// index++;
                                    }
                                }
                                index++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                try
                {
                    if (CourseInfo.ModuleSequence != null)
                    {
                        try
                        {
                            int index = 0;
                            for (int j = 0; j < ModuleId.Count; j++)
                            {

                                SqlParameter[] spParameters = new SqlParameter[3];
                                spParameters[0] = new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId };
                                spParameters[1] = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
                                spParameters[2] = new SqlParameter("@ModuleId", SqlDbType.Int) { Value = ModuleId[j] };

                                DataTable dataTable = await ExecuteStoredProcedure("[dbo].[GetGsuitMeetingSchedule]", spParameters);

                                if (dataTable != null)
                                {
                                    CourseInfo.ModuleSequence[index].apigsuit = new List<APIGoogleMeet>();
                                    for (int i = 0; i < dataTable.Rows.Count; i++)
                                    {
                                        APIGoogleMeet gsuit = new APIGoogleMeet();
                                        CourseInfo.ModuleSequence[index].apigsuit.Add(new APIGoogleMeet
                                        {
                                            is_gsuit_created = Convert.ToString(dataTable.Rows[i]["is_gsuit_created"]),
                                            gsuit_link = Convert.ToString(dataTable.Rows[i]["gsuit_link"]),
                                            gsuit_name = Convert.ToString(dataTable.Rows[i]["gsuit_name"]),
                                            ID = Convert.ToInt32(dataTable.Rows[i]["ID"]),
                                            ScheduleCode = Convert.ToString(dataTable.Rows[i]["ScheduleCode"]),
                                            StartDate = Convert.ToString(dataTable.Rows[i]["StartDate"]),
                                            StartTime = Convert.ToString(dataTable.Rows[i]["StartTime"]),
                                            EndDate = Convert.ToString(dataTable.Rows[i]["EndDate"]),
                                            EndTime = Convert.ToString(dataTable.Rows[i]["EndTime"]),
                                            ScheduleTime = Convert.ToString(dataTable.Rows[i]["ScheduleTime"]),
                                            EnableSchedule = Convert.ToBoolean(dataTable.Rows[i]["EnableSchedule"])

                                        });// index++;
                                    }
                                }
                                index++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }

            if (CourseInfo == null)
                return null;

            if (groupId != null && groupId != 0)
            {
                if (string.Equals(orgcode, _configuration["IAAOrgCode"], StringComparison.CurrentCultureIgnoreCase) && string.Equals(Status, "inprogress", StringComparison.CurrentCultureIgnoreCase))
                    CourseInfo.Status = Status;
                CourseInfo.LearningApproach = true;
            }
            return CourseInfo;
        }

        public async Task<ApiCourseInfo> GetModuleInfo(int userId, int courseId, int? moduleId)
        {
            ApiCourseInfo CourseInfo = await GetModuleInfoFromDb(userId, courseId, moduleId);
            if (CourseInfo == null)
                return null;
            return CourseInfo;
        }

        public async Task<APIMyCoursesModule> GetModulesFromDb(int userId, int courseId, string orgcode)
        {
            int IsCourseApplicable = 1;
            APIMyCoursesModule CourseInfo = new APIMyCoursesModule();
            List<object> ModuleList = new List<object>();
            try
            {

                string ToTimeZone = _db.UserMasterDetails.Where(u => u.UserMasterId == userId).Select(u => u.TimeZone).FirstOrDefault();


                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCourseModuleInfo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        List<APIModulesofCourses> CourseModules = new List<APIModulesofCourses>();
                        List<CourseSection> Sections = new List<CourseSection>();
                        if (dt.Rows.Count == 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return null;
                        }
                        if (dt.Rows.Count > 0)
                        {
                            string ModuleStatus = "Defaultcompleted";
                            foreach (DataRow row in dt.Rows)
                            {
                                APIModulesofCourses Modules = new APIModulesofCourses();
                                CourseInfo.CourseTitle = row["CourseTitle"].ToString();
                                CourseInfo.CourseType = row["CourseType"].ToString();
                                CourseInfo.CategoryName = row["CourseCategory"].ToString();
                                CourseInfo.Description = row["CourseDescription"].ToString();
                                CourseInfo.CourseCreditPoints = string.IsNullOrEmpty(row["CourseCreditPoints"].ToString()) ? 0 : float.Parse(row["CourseCreditPoints"].ToString());
                                CourseInfo.language = row["Courselanguage"].ToString();
                                CourseInfo.LearningApproach = string.IsNullOrEmpty(row["CourseLearningApproach"].ToString()) ? false : bool.Parse(row["CourseLearningApproach"].ToString());
                                CourseInfo.CourseCode = row["CourseCode"].ToString();
                                CourseInfo.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : Convert.ToInt32(row["CourseId"].ToString());
                                CourseInfo.CompletionPeriodDays = string.IsNullOrEmpty(row["CourseCompletionPeriodDays"].ToString()) ? 0 : Convert.ToInt32(row["CourseCompletionPeriodDays"].ToString());
                                CourseInfo.CourseFee = string.IsNullOrEmpty(row["CourseFees"].ToString()) ? 0 : float.Parse(row["CourseFees"].ToString());
                                CourseInfo.Currency = row["CourseCurrency"].ToString();
                                CourseInfo.ThumbnailPath = row["CourseThumbnailPath"].ToString();
                                CourseInfo.IsAssessment = string.IsNullOrEmpty(row["CourseIsAssessment"].ToString()) ? false : bool.Parse(row["CourseIsAssessment"].ToString());
                                CourseInfo.IsFeedback = string.IsNullOrEmpty(row["CourseIsFeedback"].ToString()) ? false : bool.Parse(row["CourseIsFeedback"].ToString());
                                CourseInfo.IsFeedbackOptional = string.IsNullOrEmpty(row["IsFeedbackOptional"].ToString()) ? false : bool.Parse(row["IsFeedbackOptional"].ToString());
                                CourseInfo.IsAssignment = string.IsNullOrEmpty(row["IsAssignment"].ToString()) ? false : bool.Parse(row["IsAssignment"].ToString());
                                CourseInfo.IsPreAssessment = string.IsNullOrEmpty(row["CourseIsPreAssessment"].ToString()) ? false : bool.Parse(row["CourseIsPreAssessment"].ToString());
                                CourseInfo.IsCertificateIssued = string.IsNullOrEmpty(row["CourseIsCertificateIssued"].ToString()) ? false : bool.Parse(row["CourseIsCertificateIssued"].ToString());
                                CourseInfo.AssessmentId = string.IsNullOrEmpty(row["CourseAssessmentId"].ToString().ToString()) ? 0 : Convert.ToInt32(row["CourseAssessmentId"].ToString().ToString());

                                CourseInfo.PreAssessmentId = string.IsNullOrEmpty(row["CoursePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["CoursePreAssessmentId"].ToString());
                                CourseInfo.FeedbackId = string.IsNullOrEmpty(row["CourseFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["CourseFeedbackId"].ToString());
                                CourseInfo.AssignmentId = string.IsNullOrEmpty(row["AssignmentId"].ToString()) ? 0 : Convert.ToInt32(row["AssignmentId"].ToString());
                                CourseInfo.AssessmentStatus = string.IsNullOrEmpty(row["CourseAssessmentStatus"].ToString()) ? null : row["CourseAssessmentStatus"].ToString();
                                CourseInfo.FeedbackStatus = string.IsNullOrEmpty(row["CourseFeedbackStatus"].ToString()) ? null : row["CourseFeedbackStatus"].ToString();
                                CourseInfo.Status = row["CourseStatus"].ToString();
                                CourseInfo.AdminName = row["AdminName"].ToString();
                                CourseInfo.AssignmentStatus = row["AssignmentStatus"].ToString();
                                CourseInfo.PreAssessmentStatus = row["PreAssessmentStatus"].ToString();
                                CourseInfo.Duration = string.IsNullOrEmpty(row["DurationInMinutes"].ToString()) ? 0 : Convert.ToInt32(row["DurationInMinutes"].ToString());
                                CourseInfo.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                                CourseInfo.CourseRatingCount = string.IsNullOrEmpty(row["CourseRatingCount"].ToString()) ? 0 : Convert.ToInt32(row["CourseRatingCount"].ToString());
                                CourseInfo.IsAdaptiveLearning = string.IsNullOrEmpty(row["IsAdaptiveLearning"].ToString()) ? false : bool.Parse(row["IsAdaptiveLearning"].ToString());
                                CourseInfo.CourseAssignedDate = Convert.ToString(row["CourseAssignedDate"].ToString());
                                CourseInfo.LastActivityDate = Convert.ToString(row["LastActivityDate"].ToString());
                                CourseInfo.IsDilinkingILT = string.IsNullOrEmpty(row["IsDilinkingILT"].ToString()) ? false : bool.Parse(row["IsDilinkingILT"].ToString());
                                CourseInfo.IsModuleHasAssFeed = string.IsNullOrEmpty(row["IsModuleHasAssFeed"].ToString()) ? false : bool.Parse(row["IsModuleHasAssFeed"].ToString());
                                CourseInfo.IsManagerEvaluation = string.IsNullOrEmpty(row["IsManagerEvaluation"].ToString()) ? false : bool.Parse(row["IsManagerEvaluation"].ToString());
                                CourseInfo.ManagerEvaluationStatus = Convert.ToString(row["ManagerEvaluationStatus"].ToString());
                                CourseInfo.IsPreRequisiteCourse = string.IsNullOrEmpty(row["PreRequisiteStatus"].ToString()) ? false : bool.Parse(row["PreRequisiteStatus"].ToString());
                                CourseInfo.ExpiryMessage = string.IsNullOrEmpty(row["ExpiryMessage"].ToString()) ? null : row["ExpiryMessage"].ToString();
                                CourseInfo.IsOJT = string.IsNullOrEmpty(row["IsOJT"].ToString()) ? false : bool.Parse(row["IsOJT"].ToString());
                                CourseInfo.OJTStatus = Convert.ToString(row["OJTStatus"].ToString());
                                CourseInfo.OJTId = string.IsNullOrEmpty(row["OJTId"].ToString()) ? 0 : Convert.ToInt32(row["OJTId"].ToString());
                                CourseInfo.isVisibleAssessmentDetails = string.IsNullOrEmpty(row["isVisibleAssessmentDetails"].ToString()) ? true : bool.Parse(row["isVisibleAssessmentDetails"].ToString());
                                CourseInfo.StartDate = Convert.ToString(row["CourseStartDate"].ToString());
                                CourseInfo.EndDate = Convert.ToString(row["CourseEndDate"].ToString());
                                if (dt.Columns.Contains("ExternalProvider"))
                                {
                                    if (!String.IsNullOrEmpty(row["ExternalProvider"].ToString()))
                                    {
                                        CourseInfo.ExternalProvider = row["ExternalProvider"].ToString();
                                        LCMS lCMS = (from Course in _db.Course
                                                     join
                          CourseModuleAssociation in _db.CourseModuleAssociation on Course.Id equals CourseModuleAssociation.CourseId
                                                     join M in _db.Module on CourseModuleAssociation.ModuleId equals M.Id
                                                     join L in _db.LCMS on M.LCMSId equals L.Id
                                                     where Course.Id == CourseInfo.CourseId && CourseModuleAssociation.ModuleId == M.Id && M.LCMSId == L.Id
                                                     select new LCMS
                                                     {
                                                         InternalName = L.InternalName
                                                     }).FirstOrDefault();



                                        CourseInfo.ExternalProviderCategory = lCMS.InternalName;
                                    }
                                }
                                if (!DBNull.Value.Equals(row["IsCourseExpired"]))
                                    CourseInfo.IsCourseExpired = Convert.ToBoolean(row["IsCourseExpired"]);
                                CourseInfo.RetrainingDate = Convert.ToString(row["RetrainingDate"].ToString());

                                APIModulesofCourses Module = new APIModulesofCourses();
                                Module.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleId"].ToString());
                                Module.IsAssessment = string.IsNullOrEmpty(row["ModuleIsAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsAssessment"].ToString());
                                Module.IsPreAssessment = string.IsNullOrEmpty(row["ModuleIsPreAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsPreAssessment"].ToString());
                                Module.IsFeedback = string.IsNullOrEmpty(row["ModuleIsFeedback"].ToString()) ? false : bool.Parse(row["ModuleIsFeedback"].ToString());
                                Module.AssessmentId = string.IsNullOrEmpty(row["ModuleAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleAssessmentId"].ToString());
                                Module.PreAssessmentId = string.IsNullOrEmpty(row["ModulePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModulePreAssessmentId"].ToString());
                                Module.FeedbackId = string.IsNullOrEmpty(row["ModuleFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleFeedbackId"].ToString());
                                Module.ModuleName = row["ModuleModuleName"].ToString();
                                Module.ModuleType = row["ModuleModuleType"].ToString();

                                if (!String.IsNullOrEmpty(row["ModulePath"].ToString()))
                                    Module.Path = Security.EncryptForUI(row["ModulePath"].ToString());
                                else
                                    Module.Path = row["ModulePath"].ToString();
                                Module.Thumbnail = row["ModuleThumbnail"].ToString();
                                Module.Description = row["ModuleDescription"].ToString();
                                Module.IsMobileCompatible = string.IsNullOrEmpty(row["ModuleIsMobileCompatible"].ToString()) ? false : bool.Parse(row["ModuleIsMobileCompatible"].ToString());
                                Module.CreditPoints = string.IsNullOrEmpty(row["ModuleCreditPoints"].ToString()) ? 0 : Convert.ToInt32(row["ModuleCreditPoints"].ToString());
                                Module.LCMSId = string.IsNullOrEmpty(row["ModuleLCMSId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleLCMSId"].ToString());
                                Module.YoutubeVideoId = row["ModuleYoutubeVideoId"].ToString();
                                Module.AssessmentStatus = string.IsNullOrEmpty(row["ModuleAssessmentStatus"].ToString()) ? null : row["ModuleAssessmentStatus"].ToString();
                                Module.PreAssessmentStatus = string.IsNullOrEmpty(row["ModulePreAssessmentStatus"].ToString()) ? null : row["ModulePreAssessmentStatus"].ToString();
                                Module.FeedbackStatus = string.IsNullOrEmpty(row["ModuleFeedbackStatus"].ToString()) ? null : row["ModuleFeedbackStatus"].ToString();
                                Module.ContentStatus = string.IsNullOrEmpty(row["ModuleContentStatus"].ToString()) ? Status.Incompleted : row["ModuleContentStatus"].ToString();
                                Module.Status = string.IsNullOrEmpty(row["ModuleStatus"].ToString()) ? Status.Incompleted : row["ModuleStatus"].ToString();
                                Module.Duration = string.IsNullOrEmpty(row["ModuleDuration"].ToString()) ? (float?)null : float.Parse(row["ModuleDuration"].ToString());
                                Module.ZipPath = Convert.ToString(row["ZipPath"]);
                                Module.MimeType = row["MimeType"].ToString();

                                Module.ActualModuleType = (from am in _db.AuthoringMaster
                                                           where am.LCMSId == Module.LCMSId
                                                           select am.ModuleType).FirstOrDefault();
                                if (dt.Columns.Contains("ExternalLCMSId"))
                                {
                                    if (!String.IsNullOrEmpty(row["ExternalLCMSId"].ToString()))
                                    {
                                        Module.ExternalLCMSId = Convert.ToString(row["ExternalLCMSId"]);
                                    }
                                }
                                //ILT Classroom

                                if (!DBNull.Value.Equals(row["StartDate"]))
                                    Module.StartDate = Convert.ToDateTime(Convert.ToString(row["StartDate"]));

                                if (!DBNull.Value.Equals(row["EndDate"]))
                                    Module.EndDate = Convert.ToDateTime(Convert.ToString(row["EndDate"]));

                                if (!DBNull.Value.Equals(row["RegistrationEndDate"]))
                                    Module.RegistrationEndDate = Convert.ToDateTime(Convert.ToString(row["RegistrationEndDate"]));

                                if (!DBNull.Value.Equals(row["StartTime"]) && !string.IsNullOrEmpty(Convert.ToString(row["StartTime"])))
                                    Module.StartTime = TimeSpan.Parse(Convert.ToString(row["StartTime"])).ToString(@"hh\:mm");

                                if (!DBNull.Value.Equals(row["EndTime"]) && !string.IsNullOrEmpty(Convert.ToString(row["EndTime"])))
                                    Module.EndTime = TimeSpan.Parse(Convert.ToString(row["EndTime"])).ToString(@"hh\:mm");



                                Module.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : Convert.ToInt32(row["BatchId"].ToString());
                                Module.BatchCode = Convert.ToString(row["BatchCode"]);
                                Module.BatchName = Convert.ToString(row["BatchName"]);
                                Module.ScheduleCode = Convert.ToString(row["ScheduleCode"]);
                                Module.ScheduleID = string.IsNullOrEmpty(row["ScheduleId"].ToString()) ? 0 : Convert.ToInt32(row["ScheduleId"].ToString());
                                Module.TrainingRequestStatus = Convert.ToString(row["TrainingRequestStatus"]);
                                Module.PlaceName = Convert.ToString(row["PlaceName"]);
                                Module.Address = Convert.ToString(row["PostalAddress"]);


                                Module.City = Convert.ToString(row["City"]);
                                Module.SectionId = string.IsNullOrEmpty(row["SectionId"].ToString()) ? (int?)null : Convert.ToInt32(row["SectionId"].ToString());
                                Module.SequenceNo = string.IsNullOrEmpty(row["SequenceNo"].ToString()) ? 0 : Convert.ToInt32(row["SequenceNo"].ToString());
                                Module.Location = row["Location"].ToString();
                                Module.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : Convert.ToInt32(row["CompletionPeriodDays"].ToString());
                                Module.FinalDate = Convert.ToString(row["FinalDate"].ToString());
                                Module.IsEnableModule = true;
                                Module.ActivityID = string.IsNullOrEmpty(row["ActivityID"].ToString()) ? null : Convert.ToString(row["ActivityID"].ToString());
                                Module.IsMultilingual = string.IsNullOrEmpty(row["IsMultilingual"].ToString()) ? false : bool.Parse(row["IsMultilingual"].ToString());
                                Module.selectedLanguageCode = string.IsNullOrEmpty(row["selectedLanguageCode"].ToString()) ? null : Convert.ToString(row["selectedLanguageCode"].ToString());
                                Module.IsEmbed = string.IsNullOrEmpty(row["IsEmbed"].ToString()) ? false : bool.Parse(row["IsEmbed"].ToString());
                                Module.AttendanceStatus = row["AttendanceStatus"].ToString();
                                Module.WAIVERStatus = row["WAIVERStatus"].ToString();
                                Module.ScheduleCreatedBy = Convert.ToInt32(row["ScheduleCreatedBy"]);

                                if (!string.IsNullOrEmpty(Module.FinalDate))
                                {
                                    DateTime finaldt = Convert.ToDateTime(row["FinalDate"].ToString());
                                    if (CourseInfo.LearningApproach == true)
                                    {
                                        if (ModuleStatus == "Defaultcompleted")
                                            Module.IsEnableModule = true;
                                        else
                                        {
                                            if (finaldt.Date <= DateTime.UtcNow.Date)
                                            {
                                                if (ModuleStatus == "completed")
                                                    Module.IsEnableModule = true;
                                                else
                                                    Module.IsEnableModule = false;
                                            }
                                            else
                                                Module.IsEnableModule = false;
                                        }
                                    }
                                    else
                                        Module.IsEnableModule = true;
                                }

                                ModuleStatus = Convert.ToString(row["ModuleStatus"]);

                                string ContentType = row["ContentType"].ToString();
                                if (!string.IsNullOrEmpty(ContentType))
                                {
                                    if (ContentType.ToLower().Contains("scorm"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        Module.ZipPath = string.Concat(DomainName, Module.ZipPath);
                                        Module.ZipPath = Security.EncryptForUI(Module.ZipPath);
                                    }
                                    else if (ContentType.ToLower().Contains("xapi"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        string endpoint = this._configuration[APIHelper.xAPIEndPoint];
                                        string basic = this._configuration[APIHelper.xAPIBasic];

                                        string UserUrl = _configuration[APIHelper.UserAPI];
                                        string NameById = "GetNameById";
                                        string ColumnName = "username";
                                        int Value = userId;
                                        HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgcode + "/" + ColumnName + "/" + Value);
                                        xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                                        if (response.IsSuccessStatusCode)
                                        {
                                            var username = await response.Content.ReadAsStringAsync();
                                            _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                                        }
                                        dynamic xAPIPatah = new JObject();
                                        dynamic account = new JObject();
                                        account.accountServiceHomePage = DomainName;
                                        account.accountName = _xAPIUserDetails.EmailId;
                                        xAPIPatah.name = new JArray(_xAPIUserDetails.Name);
                                        xAPIPatah.account = new JArray(account);

                                        Module.Path = row["ModulePath"].ToString() + "?actor=" + JsonConvert.SerializeObject(xAPIPatah) + "&endpoint=" + endpoint + "&auth=Basic " + basic + "&activity_id=" + Module.ActivityID + "&content_token=" + courseId + "-" + Module.ModuleId;

                                        Module.Path = Security.EncryptForUI(Module.Path);


                                    }
                                    else if (ContentType.ToLower().Contains("cmi5"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        string endpoint = this._configuration[APICMI5Helper.cmi5EndPoint];
                                        string fetch = this._configuration[APICMI5Helper.fetch];
                                        string token = this._configuration[APICMI5Helper.token];

                                        string UserUrl = _configuration[APIHelper.UserAPI];
                                        string NameById = "GetNameById";
                                        string ColumnName = "username";
                                        int Value = userId;
                                        HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgcode + "/" + ColumnName + "/" + Value);
                                        xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                                        if (response.IsSuccessStatusCode)
                                        {
                                            var username = await response.Content.ReadAsStringAsync();
                                            _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                                        }
                                        dynamic xAPIPatah = new JObject();
                                        dynamic account = new JObject();
                                        account.accountServiceHomePage = DomainName;
                                        account.accountName = _xAPIUserDetails.EmailId;
                                        xAPIPatah.name = new JArray(_xAPIUserDetails.Name);
                                        xAPIPatah.account = new JArray(account);

                                        Module.Path = row["ModulePath"].ToString() + "?actor=" + JsonConvert.SerializeObject(xAPIPatah) + "&endpoint=" + endpoint + "&fetch=" + fetch + token + "&activity_id=" + Module.ActivityID + "&registration=" + courseId + "-" + Module.ModuleId;

                                        Module.Path = Security.EncryptForUI(Module.Path);


                                    }

                                    try  // block added to downlod video at local server issue bookmarking
                                    {
                                        var EnableBlobStorage = await _accessibilityRule.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                                        if (Module.ModuleType == "Video" && !string.IsNullOrEmpty(orgcode) && !(Security.DecryptForUI(Module.Path)).Contains("https://vimeo.com"))
                                        {
                                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "yes")
                                            {
                                                if (!string.IsNullOrEmpty(Module.Path))
                                                {
                                                    string coursesPathExists = this._configuration["CoursePath"];
                                                    string filePathExists = string.Empty;
                                                    coursesPathExists = Path.Combine(coursesPathExists, orgcode, Record.Courses);
                                                    filePathExists = Path.Combine(coursesPathExists, FileType.Video, Path.GetFileName(Security.DecryptForUI(Module.Path)));
                                                    filePathExists = string.Concat(filePathExists.Split(' '));
                                                    if (!System.IO.File.Exists(filePathExists))
                                                    {
                                                        string filename = Path.Combine(orgcode, Record.Courses, FileType.Video, Path.GetFileName(Security.DecryptForUI(Module.Path)));
                                                        BlobDto imgres = await _azurestorage.DownloadAsync(filename);
                                                        if (imgres != null)
                                                        {
                                                            if (!string.IsNullOrEmpty(imgres.Name))
                                                            {
                                                                string filePath = string.Empty;
                                                                string coursesPath = this._configuration["CoursePath"];
                                                                coursesPath = Path.Combine(coursesPath, orgcode, Record.Courses);
                                                                if (!Directory.Exists(coursesPath))
                                                                {
                                                                    Directory.CreateDirectory(coursesPath);
                                                                }
                                                                filePath = Path.Combine(coursesPath, FileType.Video);
                                                                if (!Directory.Exists(filePath))
                                                                {
                                                                    Directory.CreateDirectory(filePath);
                                                                }
                                                                filePath = Path.Combine(coursesPath, FileType.Video, Path.GetFileName(Security.DecryptForUI(Module.Path)));
                                                                filePath = string.Concat(filePath.Split(' '));
                                                                if (!System.IO.File.Exists(filePath))
                                                                {
                                                                    using (FileStream outputFileStream = new FileStream(filePath, FileMode.Create))
                                                                    {
                                                                        imgres.Content.CopyTo(outputFileStream);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _logger.Error(imgres.ToString());
                                                                // return null;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _logger.Error("File not exists");
                                                            //return null;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                        //throw ex;
                                    }

                                }

                                if (IsCourseApplicable == 0 && (CourseInfo.CourseType.ToLower() != "classroom" || CourseInfo.CourseType.ToLower() != "vilt"))
                                {
                                    Module.Path = null;
                                    Module.LCMSId = null;
                                    Module.YoutubeVideoId = null;
                                    Module.ZipPath = null;

                                }
                                CourseModules.Add(Module);

                                int? SectionId = string.IsNullOrEmpty(row["SectionId"].ToString()) ? (int?)null : Convert.ToInt32(row["SectionId"].ToString());
                                if (SectionId != null)
                                {
                                    CourseSection Section = new CourseSection();
                                    Section.SectionTitle = row["SectionTitle"].ToString();
                                    Section.SectionDescription = row["SectionDescription"].ToString();
                                    Section.SectionNumber = string.IsNullOrEmpty(row["SectionNumber"].ToString()) ? 0 : Convert.ToInt32(row["SectionNumber"].ToString());
                                    Section.SectionId = SectionId;
                                    Sections.Add(Section);
                                }

                            }
                            CourseInfo.ModuleSequence = CourseModules;


                            // Adding Modules without Sections
                            CourseInfo.Modules = CourseModules.Where(m => m.SectionId == null).ToList();

                            //CourseInfo.Modules = CourseModules;
                            //Removing Duplicate sections
                            Sections = Sections.GroupBy(s => s.SectionId).Select(g => g.FirstOrDefault()).ToList();
                            //Adding modules with section
                            foreach (CourseSection Section in Sections)
                            {
                                Section.Modules = CourseModules.Where(m => m.SectionId == Section.SectionId).ToList();
                            }
                            CourseInfo.Sections = Sections.OrderBy(s => s.SectionNumber).ToList();


                            int CompletedModuleCount = (from module in CourseModules
                                                        where module.Status.ToLower().Equals(Status.Completed)
                                                        select module.ModuleId).Count();
                            int TotalModules = CourseModules.Count();
                            CourseInfo.ContentStatus = CompletedModuleCount == TotalModules && TotalModules != 0 ? Status.Completed : Status.InProgress;
                            CourseInfo.NumberofModules = TotalModules;

                            if (CourseInfo.IsAssessment || CourseInfo.IsFeedback)
                            {
                                TotalModules = TotalModules + 1;
                                if (CourseInfo.IsAssessment && !CourseInfo.IsFeedback && CourseInfo.AssessmentStatus != null)
                                {
                                    if (CourseInfo.AssessmentStatus.Equals("completed"))
                                    {
                                        CompletedModuleCount += 1;
                                    }
                                }
                                if (!CourseInfo.IsAssessment && CourseInfo.IsFeedback && CourseInfo.FeedbackStatus != null)
                                {
                                    if (CourseInfo.FeedbackStatus.Equals("completed"))
                                    {
                                        CompletedModuleCount += 1;
                                    }
                                }
                                if (CourseInfo.IsAssessment && CourseInfo.IsFeedback && CourseInfo.FeedbackStatus != null && CourseInfo.AssessmentStatus != null)
                                {
                                    if (CourseInfo.AssessmentStatus.Equals("completed") && CourseInfo.FeedbackStatus.Equals("completed"))
                                    {
                                        CompletedModuleCount += 1;
                                    }
                                }
                            }

                            CourseInfo.ProgressPercentage = (int)(((decimal)CompletedModuleCount / TotalModules) * 100);
                            if (CourseInfo.Status == "inprogress" && CourseInfo.ProgressPercentage == 0)
                            {
                                CourseInfo.ProgressPercentage = 5;
                            }

                            var result = (from cm in _db.Course
                                          join cma in _db.CourseModuleAssociation on cm.Id equals cma.CourseId
                                          join mm in _db.Module on cma.ModuleId equals mm.Id
                                          where
                                            cm.Id == courseId && cm.IsDeleted == false && cma.Isdeleted == false
                                            && mm.IsDeleted == false &&
                                            (cm.CourseType.ToLower() == "classroom" || cm.CourseType == "vilt" ||
                                             (cm.CourseType.ToLower() == "blended" && (mm.ModuleType.ToLower() == "classroom" || mm.ModuleType.ToLower() == "vilt"))
                                            )
                                          select cm.Id);
                            if (await result.CountAsync() > 0)
                                CourseInfo.IsShowViewBatches = true;
                            else
                                CourseInfo.IsShowViewBatches = false;

                            if (!string.IsNullOrEmpty(ToTimeZone) && (CourseInfo.CourseType.ToLower() == "classroom" || CourseInfo.CourseType.ToLower() == "vilt"))
                            {
                                foreach (APIModulesofCourses item in CourseInfo.Modules)
                                {
                                    string FromTimeZone = _db.UserMasterDetails.Where(u => u.UserMasterId == item.ScheduleCreatedBy).Select(u => u.TimeZone).FirstOrDefault();

                                    if (!string.IsNullOrEmpty(FromTimeZone))
                                    {
                                        if (FromTimeZone != ToTimeZone)
                                        {
                                            FromTimeZone = _tzList.Where(a => a.Text == FromTimeZone).Select(a => a.Value).FirstOrDefault().ToString();
                                            //  FromTimeZone = TZConvert.WindowsToIana(FromTimeZone);
                                            ToTimeZone = _tzList.Where(a => a.Text == ToTimeZone).Select(a => a.Value).FirstOrDefault().ToString();
                                            // ToTimeZone = TZConvert.WindowsToIana(ToTimeZone); 
                                            item.Tz_StartDt = TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.StartDate + TimeSpan.Parse(item.StartTime)), FromTimeZone), ToTimeZone);
                                            item.Tz_EndDt = TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.EndDate + TimeSpan.Parse(item.EndTime)), FromTimeZone), ToTimeZone);

                                            item.StartDate = Convert.ToDateTime(Convert.ToString(item.Tz_StartDt));
                                            item.EndDate = Convert.ToDateTime(Convert.ToString(item.Tz_EndDt));
                                            item.StartTime = TimeSpan.Parse(Convert.ToString(Convert.ToString(item.Tz_StartDt.TimeOfDay))).ToString(@"hh\:mm");
                                            item.EndTime = TimeSpan.Parse(Convert.ToString(Convert.ToString(item.Tz_EndDt.TimeOfDay))).ToString(@"hh\:mm");
                                        }
                                    }
                                }
                            }

                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return CourseInfo;
        }

        public async Task<ApiCourseInfo> GetModuleInfoFromDb(int userId, int courseId, int? moduleId)
        {
            ApiCourseInfo CourseInfo = new ApiCourseInfo();
            List<object> ModuleList = new List<object>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCourseModuleInfo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = moduleId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        if (dt.Rows.Count == 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return null;
                        }
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {

                                CourseInfo.CourseTitle = row["CourseTitle"].ToString();
                                CourseInfo.CourseType = row["CourseType"].ToString();
                                CourseInfo.CategoryName = row["CourseCategory"].ToString();
                                CourseInfo.LearningApproach = string.IsNullOrEmpty(row["CourseLearningApproach"].ToString()) ? false : bool.Parse(row["CourseLearningApproach"].ToString());
                                CourseInfo.CourseCode = row["CourseCode"].ToString();
                                CourseInfo.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : Convert.ToInt32(row["CourseId"].ToString());
                                CourseInfo.ThumbnailPath = row["CourseThumbnailPath"].ToString();
                                CourseInfo.IsAssessment = string.IsNullOrEmpty(row["CourseIsAssessment"].ToString()) ? false : bool.Parse(row["CourseIsAssessment"].ToString());
                                CourseInfo.IsFeedback = string.IsNullOrEmpty(row["CourseIsFeedback"].ToString()) ? false : bool.Parse(row["CourseIsFeedback"].ToString());
                                CourseInfo.IsAssignment = string.IsNullOrEmpty(row["IsAssignment"].ToString()) ? false : bool.Parse(row["IsAssignment"].ToString());
                                CourseInfo.IsPreAssessment = string.IsNullOrEmpty(row["CourseIsPreAssessment"].ToString()) ? false : bool.Parse(row["CourseIsPreAssessment"].ToString());
                                CourseInfo.AssessmentId = string.IsNullOrEmpty(row["CourseAssessmentId"].ToString().ToString()) ? 0 : Convert.ToInt32(row["CourseAssessmentId"].ToString().ToString());
                                CourseInfo.PreAssessmentId = string.IsNullOrEmpty(row["CoursePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["CoursePreAssessmentId"].ToString());
                                CourseInfo.FeedbackId = string.IsNullOrEmpty(row["CourseFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["CourseFeedbackId"].ToString());
                                CourseInfo.AssignmentId = string.IsNullOrEmpty(row["AssignmentId"].ToString()) ? 0 : Convert.ToInt32(row["AssignmentId"].ToString());
                                CourseInfo.AssessmentStatus = string.IsNullOrEmpty(row["CourseAssessmentStatus"].ToString()) ? null : row["CourseAssessmentStatus"].ToString();
                                CourseInfo.FeedbackStatus = string.IsNullOrEmpty(row["CourseFeedbackStatus"].ToString()) ? null : row["CourseFeedbackStatus"].ToString();
                                CourseInfo.Status = row["CourseStatus"].ToString();
                                CourseInfo.PreAssessmentStatus = row["PreAssessmentStatus"].ToString();
                                CourseInfo.Duration = string.IsNullOrEmpty(row["DurationInMinutes"].ToString()) ? 0 : Convert.ToInt32(row["DurationInMinutes"].ToString());

                                APIModuleInfo Module = new APIModuleInfo();
                                Module.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleId"].ToString());
                                Module.IsAssessment = string.IsNullOrEmpty(row["ModuleIsAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsAssessment"].ToString());
                                Module.IsPreAssessment = string.IsNullOrEmpty(row["ModuleIsPreAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsPreAssessment"].ToString());
                                Module.IsFeedback = string.IsNullOrEmpty(row["ModuleIsFeedback"].ToString()) ? false : bool.Parse(row["ModuleIsFeedback"].ToString());
                                Module.AssessmentId = string.IsNullOrEmpty(row["ModuleAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleAssessmentId"].ToString());
                                Module.PreAssessmentId = string.IsNullOrEmpty(row["ModulePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModulePreAssessmentId"].ToString());
                                Module.FeedbackId = string.IsNullOrEmpty(row["ModuleFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleFeedbackId"].ToString());
                                Module.ModuleName = row["ModuleModuleName"].ToString();
                                Module.ModuleType = row["ModuleModuleType"].ToString();
                                Module.Path = row["ModulePath"].ToString();
                                Module.Thumbnail = row["ModuleThumbnail"].ToString();
                                Module.Description = row["ModuleDescription"].ToString();
                                Module.IsMobileCompatible = string.IsNullOrEmpty(row["ModuleIsMobileCompatible"].ToString()) ? false : bool.Parse(row["ModuleIsMobileCompatible"].ToString());
                                Module.CreditPoints = string.IsNullOrEmpty(row["ModuleCreditPoints"].ToString()) ? 0 : Convert.ToInt32(row["ModuleCreditPoints"].ToString());
                                Module.LCMSId = string.IsNullOrEmpty(row["ModuleLCMSId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleLCMSId"].ToString());
                                Module.YoutubeVideoId = row["ModuleYoutubeVideoId"].ToString();
                                if (dt.Columns.Contains("ExternalLCMSId"))
                                {
                                    if (!String.IsNullOrEmpty(row["ExternalLCMSId"].ToString()))
                                    {
                                        Module.ExternalLCMSId = Convert.ToString(row["ExternalLCMSId"]);
                                    }
                                }
                                Module.AssessmentStatus = string.IsNullOrEmpty(row["ModuleAssessmentStatus"].ToString()) ? null : row["ModuleAssessmentStatus"].ToString();
                                Module.PreAssessmentStatus = string.IsNullOrEmpty(row["ModulePreAssessmentStatus"].ToString()) ? null : row["ModuleAssessmentStatus"].ToString();
                                Module.FeedbackStatus = string.IsNullOrEmpty(row["ModuleFeedbackStatus"].ToString()) ? null : row["ModuleFeedbackStatus"].ToString();
                                Module.ContentStatus = string.IsNullOrEmpty(row["ModuleContentStatus"].ToString()) ? Status.Incompleted : row["ModuleContentStatus"].ToString();
                                Module.Status = string.IsNullOrEmpty(row["ModuleStatus"].ToString()) ? Status.Incompleted : row["ModuleStatus"].ToString();
                                Module.Duration = string.IsNullOrEmpty(row["ModuleDuration"].ToString()) ? (float?)null : float.Parse(row["ModuleDuration"].ToString());
                                Module.ZipPath = Convert.ToString(row["ZipPath"]);
                                Module.MimeType = row["MimeType"].ToString();
                                //ILT Classroom
                                Module.StartDate = Convert.ToString(row["StartDate"]);
                                Module.EndDate = Convert.ToString(row["EndDate"]);
                                Module.RegistrationEndDate = Convert.ToString(row["RegistrationEndDate"]);
                                Module.StartTime = Convert.ToString(row["StartTime"]);
                                Module.EndTime = Convert.ToString(row["EndTime"]);
                                Module.ScheduleCode = Convert.ToString(row["ScheduleCode"]);
                                Module.ScheduleID = string.IsNullOrEmpty(row["ScheduleId"].ToString()) ? 0 : Convert.ToInt32(row["ScheduleId"].ToString());
                                Module.TrainingRequestStatus = Convert.ToString(row["TrainingRequestStatus"]);
                                Module.PlaceName = Convert.ToString(row["PlaceName"]);
                                Module.Address = Convert.ToString(row["PostalAddress"]);
                                Module.City = Convert.ToString(row["City"]);
                                Module.InternalName = row["InternalName"].ToString();
                                Module.SectionId = string.IsNullOrEmpty(row["SectionId"].ToString()) ? (int?)null : Convert.ToInt32(row["SectionId"].ToString());
                                Module.SequenceNo = string.IsNullOrEmpty(row["SequenceNo"].ToString()) ? 0 : Convert.ToInt32(row["SequenceNo"].ToString());
                                Module.Location = row["Location"].ToString();
                                string ContentType = row["ContentType"].ToString();
                                Module.IsEmbed = string.IsNullOrEmpty(row["IsEmbed"].ToString()) ? false : bool.Parse(row["IsEmbed"].ToString());


                                if (!string.IsNullOrEmpty(ContentType))
                                {
                                    if (ContentType.ToLower().Contains("scorm"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        Module.ZipPath = string.Concat(DomainName, Module.ZipPath);
                                    }

                                }
                                CourseInfo.Modules = Module;


                            }

                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return CourseInfo;
        }

        public async Task<DataTable> ExecuteStoredProcedure(string spName, SqlParameter[] sqlParameters)
        {
            DataTable dt = null;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;

                    foreach (var parameter in sqlParameters)
                    {
                        cmd.Parameters.Add(new SqlParameter(parameter.ParameterName, parameter.SqlDbType) { Value = parameter.Value });
                    }

                    await dbContext.Database.OpenConnectionAsync();
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    dt = new DataTable();
                    dt.Load(reader);
                    reader.Dispose();
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
            return dt;
        }



    }
}