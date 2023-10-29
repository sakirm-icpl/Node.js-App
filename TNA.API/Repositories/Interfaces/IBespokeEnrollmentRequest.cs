using TNA.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using TNA.API.APIModel;

namespace TNA.API.Repositories.Interfaces
{
    public interface IBespokeEnrollmentRequest : IRepository<CourseScheduleEnrollmentRequest>
    {
        Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelTwo(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetAllBeSpokeEnrollmentRequestLevelTwoCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<ApiResponse> ActionsByAdminsHRLM(int level, APIActionsByAdmins obj, int UserId, string UserName);
        Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelThree(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetAllBeSpokeEnrollmentRequestLevelThreeCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelFour(int page, int pageSize, int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetAllBeSpokeEnrollmentRequestLevelFourCount(int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<ApiResponse> ActionsByAdminsBUHRFinal(int level, APIActionsByAdmins obj, int UserId);
        Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelFive(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null);
        Task<int> GetAllBeSpokeEnrollmentRequestLevelFiveCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null);


    }
}
