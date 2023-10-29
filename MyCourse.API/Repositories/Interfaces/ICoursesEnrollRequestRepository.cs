using MyCourse.API.APIModel;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface ICoursesEnrollRequestRepository : IRepository<Model.CoursesEnrollRequest>
    {
        Task<bool> IsExist(int courseid, int userid, string status);
        Task<string> GetStatus(int userId, int courseId);
        Task<APITotalRequest> GetSupervisorCourseRequests(GetSupervisorData getSupervisorData, int userId);
    }
    public interface ICoursesEnrollRequestDetailsRepository : IRepository<Model.CoursesEnrollRequestDetails>
    {
    }
}
