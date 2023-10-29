using Competency.API.APIModel;
using Competency.API.APIModel.Competency;
using Competency.API.Common;
using Competency.API.Model;

using System.Collections.Generic;
using System.Threading.Tasks;
using static Competency.API.Model.ResponseModels;

namespace Competency.API.Repositories.Interfaces
{
    public interface IMyCoursesRepository
    {
        int GetIdFromUserId(string userid);
        Task<List<APIMyCourses>> GetAllCatalogCourse(string OrgCode, int page, int pageSize, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string status = null, string provider = null);
        Task<List<APIMyCourses>> Get(int userId, int page, int pageSize, int? categoryId = null, string CourseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null, string IsActive=null);
        Task<List<APIMyCourses>> GetAllCourseDetails(APIUserCourseDetails obj);
        Task<int> GetAllCatalogCourseCount(string orgcode, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string provider = null);


        Task<int> Count(int userId, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null, string IsActive=null);
    //    Task<APIMyCoursesModule> GetModule(int userId, int courseId, string organizationcode = null, int? groupId = null);
       // Task<int> GetCourseProgressCount(int userId, string courseStatus = null, string courseType = null, int? subCategoryId = null, int? categoryId = null, string search = null);
        //Task<List<APIMyCourses>> GetCourseProgress(int userId, int page, int pageSize, string courseStatus = null, string courseType = null, int? subCategoryId = null, int? categoryId = null, string search = null);
       // Task<List<APIMyCourses>> GetNotStarted(int userId, int page, int pageSize, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null);
     //   Task<int> NotStartedCount(int userId, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null);
        Task<int> GetAllCourseCount(int userId, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string provider = null,string IsActive=null);
        Task<List<APIMyCourses>> GetAllCourse(int userId, int page, int pageSize, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string status = null, string provider = null, string IsActive=null);
        Task<List<APIMyCourses>> GetAllUserCourseData(APIUserCourseDetails obj);
       // Task<bool> IsShowCatlogue(string token);
        Task<int> GetProgressStatusDuration(int userId, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null);
        Task<int> GetNotStartedDuration(int userId, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null);
        Task<List<APIMyCourses>> GetMissionCourses(int userId, int page, int pageSize, string mission = null, int? categoryId = null, string CourseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null);
        Task<int> GetMissionCourseCount(int userId, string mission = null, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null);
        Task<int> AddEBT(EBTDetails obj);
        Task<List<APIEBTDetails>> Get();
        Task<APIEBTDetails> GetByID(int id);
        Task<int> UpdateEBT(EBTDetails obj);
        Task<APIEBTDetails> GetByUserId(int UserId, int courseId);
       // Task<ApiCourseStatitics> GetCourseStatitics(int userId);
        Task<int> PostCourseStatitics(int userId);
       // Task<APIProgressStatusCountData> GetUserProgressStatusCountData(int userId);
      //  Task<int> CheckUserApplicabilityToCourse(int userId, int CourseId, string OrgCode, int? groupId = null);
        Task<List<APICourseTypeahead>> SearchCourses(string search = null);
        Task<ApiCourseInfo> GetModuleInfo(int userId, int courseId, int? moduleId);
        //Task<Message> EnrollCourse(int userId, int CourseId,string orgCode);
        Task<List<APIMyCourses>> GetCompletedCourses(int userId, int page, int pageSize, int? categoryId = null, string CourseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null);
        Task<APICountAndDuration> CountCompletedCourses(int userId, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null);
        Task<List<APIMonthwiseCompletion>> GetMonthWiseCompletion(int UserId);
        Task<int> GetCountMonthWiseCompletion(int UserId);
        Task<int> GetUserDetailsByUserID(string userId);
       // Task<string> EnrollmentTypeForUser(string token);
        //Task<string> CheckDiLink(string token);
      //  Task<APIProgressStatusCountData> GetUserProgressStatusCountDataForDeLink(int userId);
       // Task<APIxAPICompletionDetails> checkxAPICompletion(int userId, int courseId, int moduleId, string orgnizationCode);
      //  Task<APICareerJobRoles> PostCareerJobRoles(APICareerJobRoles apiCareerJobRoles, int UserId);
        //Task<List<TypeAhead>> GetTypeAHead(string search = null);
      //  Task<List<APICareerRoles>> GetCareerJobRoles(int UserId);
        Task<APICareerRoles> GetUserJobRoleByUserId(int UserId);
       // Task<List<MultiLangualContentInfo>> GetMultiLangualModules( int courseId, string token ,int moduleId, string language = null);
       // Task<List<APINextJobRoles>> ViewPositions(int UserId);

       // Task<List<MultiLangualContentInfo>> addUserPrefferedCourseLanguage(UserPrefferedCourseLanguage obj, string token);
     //   Task<string>  GetUserPrefferedLanguage(int UserId,int  courseId,int moduleId);
        Task<bool?> GetIsmoduleMultilingual(int moduleId);
       // Task<bool> ManagerUserRelated(string userId, int managerId);
        Task<IEnumerable<APIMyTeamCompetencyMasterCourse>> GetCompetencyMasterCourse(APIUserCourseDetails obj);
        Task<APIManagerEvaluationData> GetCompetencyMasterCourse(int userId, APIGetManagerEvaluationCourses obj);
        Task<APICompetencySkillSet> GetUserCurrentJobRoleCompetencies(int UserId, int? JobRoleID);
        Task<string> GetCourseInfo(int courseId);
        Task<APICoursesDuration> GetCoursesDuration(int userId);
        //Task<List<TnaEmployeeData>> GetCourseDetails(int UserId, string OrgCode, TnaCategories tnaCategory);
        //Task<Message> PostTnaEmployeeRequest(int UserId, List<TnaEmployeeNominateRequestPayload> tnaEmployeeNominateRequestPayload);
        Task<APICoursesDuration> GetCoursesDurationByDate(int userId, CoursesDuration coursesDuration);
        Task<int> GetUserIdByEmailId(string ConnectionString,string useId = null);
        Task<int> GetCourseIdByCourseCode(string ConnectionString, string code = null);
        Task<APICompetencySkillNameV4> GetUserCurrentJobRoleCompetenciesV2(string Id,int Userid, int? JobRoleID);
       // APIResponse<MyCourseForApplicability> GetCourseDetailsForApplicability(int CourseId);
    }
}
