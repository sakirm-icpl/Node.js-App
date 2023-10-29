
using CourseApplicability.API.APIModel;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface ICoursesEnrollRequestRepository : IRepository<CourseApplicability.API.Model.CoursesEnrollRequest>
    {
        Task<bool> IsExist(int courseid, int userid, string status);
        Task<string> GetStatus(int userId, int courseId);
        Task<APITotalRequest> GetSupervisorCourseRequests(GetSupervisorData getSupervisorData, int userId);
    }
    public interface ICoursesEnrollRequestDetailsRepository : IRepository<CourseApplicability.API.Model.CoursesEnrollRequestDetails>
    {
    }
}

