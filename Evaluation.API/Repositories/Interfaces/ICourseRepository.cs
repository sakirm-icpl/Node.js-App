using Evaluation.API.APIModel;
//using Courses.API.APIModel.NodalManagement;
//using Courses.API.APIModel.TigerhallIntegration;
using Courses.API.Common;
//using Courses.API.ExternalIntegration.EdCast;
using Evaluation.API.Model;
//using Courses.API.Model.EdCastAPI;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Evaluation.API.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
     //   Task<List<APIAllCourses>> GetAll(int? page = null, int? pageSize = null, int? categoryId = null, bool? status = null, string search = null, string filter = null);
     //   Task<int> Count(int? categoryId = null, bool? status = null, string search = null, string filter = null);
     //   Task<List<APICourseDTO>> GetCourse(int? categoryId, string search = null, string courseType = null);
     //   Task<List<APICoursesData>> GetReportCourse(int userId, string role,string search = null, string courseType = null);
     //   Task<List<APICourseDTO>> CourseTypehead(string search = null, string filter = null);
     //   Task<List<APICourseDTO>> GetILTCourse(int? categoryId, string search = null);
     //   Task<List<APICourseDTO>> GetCourseTypeahead(int? categoryid);
     //   Task<List<APICourseDTO>> ApplicableToAllCourseTypeahead(string search = null);
     //   Task<List<APICourseTypeahead>> SearchCourses(string search = null);
     //   Task<string> GetCourseCodeById(int? courseId);
     //   Task<List<APIModuleTypeAhead>> GetCourseModules(int courseId);
     //   Task<IEnumerable<object>> GetCourseNameList(string id);
     //   Task<string> GetCourseNam(int? id);
     //   Task AddModules(int courseid, int[] moduleids);
     //   Task<IEnumerable<ApiCourseModuleAssociation>> GetModules(int courseid);
     //   Task<bool> CheckCourseForAllowModification(int courseid);
     //    Task<bool> CreateCourseFromExisting(int courseid);
     //   Task<string> GetCourseId(string courseCode);
     //   Task<string> GetFeedbackConfigurationID(int? id);
     //   Task<string> GetAssessmentConfigurationID(int? id, int? CId, string orgCode, bool isPreAssessment = false, bool isContentAssessment = false);
     //   Task<List<APICourseDTO>> GetModulesAssessment();
     //   Task<string> GetModuleName(int? id);
     //   Task<List<APICourseDTO>> GetCourseForAccessibility(int? categoryId, string search);
     //   Task<bool> Exist(string title, string code, string isinstitute, int? courseId = null);
     //   Task<bool> IsAssementExist(int CourseID);
     //   Task<bool> IsFeedbackExist(int CourseID);
     //   Task<Object> GetCoursesAssessmentFeedbackName(int? assessmentId, int? feedbackId, int? preassessmentId, int? assignmentId, int? managerEvaluationId, int? ojtId );
     //   Task<Object> GetCoursesAssignment(int? assignmentId);
     //   Task<CourseCode> GetCourseCode();
     //   Task<int> SendCourseAddedNotification(int courseId, string title, string courseCode, string token);
     //   Task<Message> DeleteCourse(int courseId, string Token);
     //   Task<int> AddCourseHistory(Courses.API.Model.Course oldCourse, Courses.API.Model.Course newCourse,
     //       List<CourseModuleAssociation> oldModule, List<CourseModuleAssociation> newModule);

     //   Task<List<object>> GetAssessmentCourse(int? categoryId, string search = null, string courseType = null);
     //   Task<List<object>> GetAssessmentCourseReport(int? categoryId, string search = null, string courseType = null);
     //   Task<List<object>> CourseTypeAheadFeedback(int? categoryId = null, string search = null, string courseType = null);
     //   Task<int> UpdateCourseNotification(Courses.API.Model.Course oldCourse, int courseId, string title, string courseCode, string token);

     //   Task<int> UpdateCourseNotification(Courses.API.Model.Course oldCourse, int courseId, string title, string token);
     //   Task<IEnumerable<APICourses>> GetAllCourses(int userId, bool showallData = false, int? categoryId = null, bool? status = null, string filter = null, string search = null);
     //   Task<List<APICourseWiseEmailReminder>> GetPagination(int page, int pageSize);
     //   Task<List<APICourseWiseSMSReminder>> GetPaginationSMS(int page, int pageSize);
     //   Task<int> GetCountCourseWiseEmailReminder();
     //   Task<int> GetCountCourseWiseSMSReminder();

     //   Task<CourseWiseEmailReminder> Addcoursewise(CourseWiseEmailReminder aPICourseWiseEmail, int UserId, string OrganisationCode);
     //   Task<CourseWiseSMSReminder> AddcourseWiseSMS(CourseWiseSMSReminder aPICourseWiseSMS, int UserId, string OrganisationCode);
     //   Task<ApiAssignmentDetails> AddAssignmentDetails(ApiAssignmentDetails apiAssignmentDetails);
     //   Task<ApiAssignmentDetails> UpdateRejectedAssignmentDetails(ApiAssignmentDetails apiAssignmentDetails);
     //   Task<string> SaveFile(IFormFile uploadedFile, string fileType, string OrganizationCode);
     //   Task<List<ApiAssignmentInfo>> GetAssignmentDetails(int loginUserId, SearchAssignmentDetails searchAssignmentDetails);
     //   Task<AssignmentDetails> GetAssignmentDetail(int id);
     //   Task<int> GetAssignmentDetailsCount(int loginUserId, SearchAssignmentDetails searchAssignmentDetails);
     //   Task<ApiAssignmentDetails> GetAssignmentDetailsById(SearchAssignmentDetails searchAssignmentDetails);
     //   Task<bool> IsAssignmentSubmitted(SearchAssignmentDetails searchAssignmentDetails);
     //   Task<APICourseTypewithCount> GetCourseTypewiseCount();
     //   Task<Courses.API.Model.Course> GetCourseInfoByCourseCode(string CourseCode);
     //   Task<APICourseTypeCount> GetCourseTypewiseCountNew();
     //   Task<bool> IsEmailConfigured(string orgCode);
       Task<string> GetMasterConfigurableParameterValue(string configurationCode);
     //   Task<string> GetMasterConfigurableParameterValueOrganization(string configurationCode, string orgcode);
     //   Task<List<object>> GetAssessmentTypeCourse(int? categoryId = null, string search = null);
     //   Task<List<APIJobRole>> GetCompetencySkill(int CourseId);
     //   Task<List<APISubSubCategory>> GetSubSubCategory(int CourseId);
     //   Task<string> GetManagerAssessmentConfigurationID(int? courseId, int? moduleId);
     //   Task<List<APICourseCategoryTypeahead>> GetCourseIdByCourseCategory(int? Id = null);
     //   Task<APIJobRole> GetPrerequisiteCourseByCourseId(int CourseId);
     //   Task<APIPrerequisiteCourseStatus> GetPrerequisiteCourseStatus(APIPreRequisiteCourseStatus preRequisiteCourseStatus, int UserId);
     //   Task<List<APICourseTagged>> GetTagged(int? page = null, int? pageSize = null, string search = null);
     //   Task<List<object>> TypeAheadAuthority(int? categoryId = null, string search = null);
     //   Task<Courses.API.Model.Course> GetCourseInfoByCourseName(string courseName);
     //   Task<CourseLog> AddCourseLog(CourseLog courseLog);
     //   Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string defaultValue = "");
     //   Task<bool> IsRetrainindDaysEnable(int courseId);
     //   Task<bool> IsDependacyExist(int courseId);
     //   Task<List<string>> GetCourseName(int[] courseId);
     //   Task<APICourseResponse> GetNodalCourses(APINodalCourses aPINodalCourses, string ConnectionString);
     //   Task<APICourseResponse> GetNodalCoursesForGroupAdmin(APINodalCoursesGroupAdmin aPINodalCoursesGroupAdmin);
     //   Task<List<APINodalCourseTypeahead>> GetNodalCourseTypeahead(string Search = null);
     //   Task<List<APINodalCourseTypeahead>> GetNodalCoursesDropdown();
     //   Task InsertCourseMasterAuditLog(Model.Course course, string action);
     //   Task<APICourseDetailsTigerhall> GetCourseDetailsTigerhall(List<APITigerhallCourses> courses);
     //   Task<UserWiseCourseEmailReminder> AddUsercoursewise(UserWiseCourseEmailReminder courseWiseEmail, int UserId, string OrganisationCode);
     //   Task<AssessmentReview> assessmentReview(int courseId);
     //   Task<APIEdcastDetailsToken> GetEdCastToken(string LxpDetails = null);
     //   Task<APIEdCastTransactionDetails> PostCourseToClient(int courseID,int userId, string token = null, string LxpDetails=null);
     //   Task<APIDarwinTransactionDetails> PostCourseToDarwinbox(int courseID, int userId,string orgcode);
     //Task<bool?> IsPublishedCourse(int courseId);
     //   Task<APITotalCoursesView> GetAllV2(ApiGetCourse apiGetCourse, int userId, string userRole);
     //   Task<APIAllCourses> GetCourseDetailsById(int id);
     //   Task<APILmsCourseResponse> GetLMSCourses(APITtGrCourses aPITtGrCourses, string ConnectionString);
     //   Task<object> GetCourseVendorDetails(string Vendor_Type);
     //   Task<DarwinboxTransactionDetails> PostCourseStatusToDarwinbox(int courseID, int userId, string status, string orgcode,string connectionstring=null);
     //   Task<object> PostDevelopementPlan(int courseId, int UserId, string UserName);
     //   Task<AdditionalResourceForCourse> PostAdditionalResourceForCourse(APIAdditionalResourceForCourse data,int UserId);
     //   Task<List<AdditionalResourceForCourse>> GetAdditionalResourceForCourse(string courseCode);
     //   Task<AdditionalResourceForCourse> UpdateAdditionalResourceForCourse(APIAdditionalResourceForCourse data, int UserId);
     //   Task<AdditionalResourceForCourse> DeleteAdditionalResourceForCourse(int id,int UserId);
     //   Task<DevelopementPlanCode> DevPlanCode(bool isIdp=false,string orgcode=null);
    }
}
