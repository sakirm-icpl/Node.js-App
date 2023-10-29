using TNA.API.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TNA.API.APIModel;

namespace TNA.API.Repositories.Interfaces
{
    public interface ICourseRequest : IRepository<CourseRequest>
    {
        Task<List<APICourseRequest>> GetAllRequestDetails(int TNAYearId, int UserId);
        Task<int> GetAllRequestCount(int TNAYearId, int UserId);
        Task<ApiResponse> PostRequest(int[] CourseID, int UserId, string UserName);
        Task<ApiResponse> PostOtherCourses(string CourseName, string CourseDescription, int UserId);
        Task<List<APICourseRequest>> GetAllRequest(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null, string LoginId = null, string RoleName = null);
        Task<int> GetAllCourseRequestCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null, string LoginID = null, string RoleName = null);
        Task<List<APICourseRequest>> GetAllCourseDetailsByUserID(int UserID, int UserId, int Role);
        Task<ApiResponse> Put(int id, APICoursesRequestDetails obj, int UserId, string UserName, int Role);
        Task<ApiResponse> PostForNominate(APICourseRequest obj, int UserId, string UserName);
        Task<ApiResponse> RequestForCourse(int CourseID, int UserId);
        Task<string> GetCourseStatus(int courseID, int? UserId);
        Task<List<APICourseRequest>> GetHistoryData(int userID);
        Task<List<APICourseRequest>> GetDataFromHRToBUHead(int TNAYearId, string userName = null, string search = null, string searchText = null);
        Task<ApiResponse> PostFromHRToBUHead(int[] CourseRequestId, int UserId, string UserName, string search = null, string searchText = null);
        Task<ApiResponse> PostForAmendment(APIAmendment obj, int UserId, string UserName, int Role);
        Task<FileInfo> GetDataMigrationReport(int UserId, string LoginId, int Role, string userName = null, string search = null, string searchText = null);
        FileInfo GetDataMigrationReportExcel(IEnumerable<APICourseRequest> obj);
        Task<int> DuplicateCourseRequestCheck(int[] CourseID, int UserId);
        Task<List<TypeAhead>> GetSubordinateUsers(int UserId, string search);
        Task<List<APICourseRequest>> GetAllDetailsForEndUser(int courseRequestId, int UserId);
        Task<ApiResponse> PostCourseRequestIds(int[] courseRequestId, int UserId, string UserName, string OrgCode,int isAccept);
        Task<ApiResponse> EnrollAll(int UserId, string UserName);
        Task<int> SendCourseAddedNotification(int courseId, string title, string token, int ReportsToID, string Message, string type, int? CourseIdD = null);
        Task<ApiResponse> PostCourseRequest(int[] data, int UserId, string UserName, string orgCode);
        Task<ApiResponse> PostCourseRequestFromLMToHR(int[] data, int UserId, string UserName, string orgCode);
        Task<int> isCourseRequestSendByUser(int UserId);
        Task<int> isCourseRequestSend(APICourseRequest obj, int UserId, string UserName);
        Task<int> DuplicateOtherCourseRequestCheck(string CourseName, int UserId);
        Task<int> CheckForSystemCourse(string CourseName);
        Task<ApiResponse> DeleteCourseRequest(int courseRequestId, int UserId);
        Task<List<TypeAhead>> GetAllDepartmentUnderBU(string UserId);
        Task<int> GetAllCourseRequestForTACount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<List<APICourseRequest>> GetAllRequestForTA(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<string> GetMultipleRoleForUser(int UserId, string UserName);
        Task<ApiResponse> PostCourseRequestFromHRToBU(int[] data, int UserId, string UserName);
        Task<int> CheckForTNAYearExpiry();
    }
}
