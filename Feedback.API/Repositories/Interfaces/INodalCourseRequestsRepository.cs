using Feedback.API.APIModel;
using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface INodalCourseRequestsRepository : IRepository<NodalCourseRequests>
    {

        Task<APIScormGroup> GetUserforCompletion(int GroupId);

    }
}
