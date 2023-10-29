using Course.API.Model;
using Courses.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IExternalTrainingRequest : IRepository<ExternalTrainingRequest>
    {
        Task<ExternalTrainingRequest> PostExternalTrainingRequest(ExternalTrainingRequest data, int UserId);
        Task<ExternalTrainingRequestListandCount> GetExternalTrainingRequest(int page, int pageSize, string search,int UserId);

        Task<ExternalTrainingRequestListandCountAllUser> GetExternalTrainingRequestAllUser(int page, int pageSize, string searchBy, string search,int userId);

        Task<ExternalTrainingRequestEdit> GetExternalTrainingRequestEdit(int reqId,int UserId);

        string GetUserRole(int userId);
    }
}
