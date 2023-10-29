//using Courses.API.APIModel;
//using Courses.API.APIModel.;
using TNA.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using TNA.API.APIModel;

namespace TNA.API.Repositories.Interfaces
{
    public interface ICourseScheduleEnrollmentRequest : IRepository<CourseScheduleEnrollmentRequest>
    {
        Task<List<APICourseScheduleEnrollmentRequest>> GetScheduleEnrollRequest(APIBeSpokeSearch beSpokeSearch, int UserId);
        Task<int> GetScheduleEnrollRequestCount(int UserId, APIBeSpokeSearch searchbespoke);
        Task<bool> CheckDuplicate(APICourseScheduleEnrollmentRequest obj, int UserId);
        Task<ApiResponse> PostRequestSchedule(APICourseScheduleEnrollmentRequest obj, int UserId, string UserName, string orgcode);
        Task<APIILTRequestRsponse> GetAllRequestDetails(int moduleId, int courseId, int userId);
        Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelTwo(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetScheduleEnrollmentRequestLevelTwoCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<APIRequestedFrom> GetCourseRequestedFrom(int CourseID, int UserId);
        Task<ApiResponse> ActionsByAdmins(int level, APIActionsByAdmins obj, int UserId, string UserName, string organisationcode);
        Task<ApiResponse> GetCourseName(string type, string search = null);
        Task<List<APITrainingNomination>> GetUsersForNomination(int Level, int scheduleID, int courseId, int moduleId, int UserId, string LoginId, int page, int pageSize, string search = null, string searchText = null);
        Task<int> GetUsersCountForNomination(int Level, int scheduleID, int courseId, int moduleId, int UserId, string LoginId, string search = null, string searchText = null);
        Task<ApiResponse> NominateUsersForAdmins(int level, NominateUsersForAdmins obj, int UserId, string orgcode);
        Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelThree(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetScheduleEnrollmentRequestLevelThreeCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelFour(int page, int pageSize, int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetScheduleEnrollmentRequestLevelFourCount(int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<ApiResponse> NominateUsersForAdminsBUHR(int level, NominateUsersForAdmins obj, int UserId, string orgcode);
        Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelFive(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetScheduleEnrollmentRequestLevelFiveCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<ApiResponse> ActionsByAdminsHR(int level, APIActionsByAdmins obj, int UserId, string orgcode);
        Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelSix(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetScheduleEnrollmentRequestLevelSixCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<APIModuleCompletionDetails> GetModuleCompletionStatus(int ModuleID, int UserId);
        Task<List<APIScheduleEnrollmentRequest>> GetAllDetailsForEndUser(int requestId, int UserId);
    }
}
