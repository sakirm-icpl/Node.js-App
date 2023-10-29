using Assessment.API.APIModel;
using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface INodalCourseRequestsRepository : IRepository<NodalCourseRequests>
    {
        Task<APIScormGroup> GetUserforCompletion(int GroupId);
    }
}
