//using Courses.API.APIModel;
using TNA.API.Common;
using TNA.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using TNA.API.APIModel;

namespace TNA.API.Repositories.Interfaces
{
    public interface ITNACourseRequest : IRepository<CourseRequest>
    {
        Task<List<TNAYear>> GetTNAYear();
        Task<List<APICourseRequest>> GetAllCourseRequestLevelOne(int page, int pageSize, int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetAllCourseRequestLevelOneCount(int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<List<APICourseRequest>> GetAllCourseRequestLevelTwo(int page, int pageSize, int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetAllCourseRequestLevelTwoCount(int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<List<APICourseRequest>> GetAllCourseRequestLevelThree(int page, int pageSize, int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetAllCourseRequestLevelThreeCount(int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<List<APICourseRequest>> GetAllCourseRequestLevelFour(int page, int pageSize, int TNAYearId, int UserId, string userName, string LoginId, string search, string searchText, string status);
        Task<int> GetAllCourseRequestLevelFourCount(int TNAYearId, int UserId, string userName, string LoginId, string search, string searchText, string status);
        Task<ApiResponse> DeleteCourseRequest(int courseRequestId, int UserId, bool IsNominate);
        Task<List<APITNAYear>> GetAllTNAYear();
        Task<int> PostTNAYear(string Year, int UserId);
        Task<List<TnaRequestData>> GetAllRequestedBatches(int UserId, int page, int pageSize, string searchUser = null);
        Task<int> GetAllRequestedBatchesCount(int UserId, string searchUser = null);
        Task<List<TnaRequestsDetails>> GetTnaRequestsDetails(int UserId, int page, int pageSize, string searchData = null);
        Task<int> GetTnaRequestsDetailsCount(int UserId, string searchData = null);
        Task<List<TnaEmployeeNominateRequestPayload>> GetTnaRequestsByUser(int UserId);
        Task<Message> PostTnaTnaSupervisedRequest(int SelectedUserId, int UserId, List<TnaEmployeeNominateRequestPayload> TnaSupervisedRequest);
    }
}
