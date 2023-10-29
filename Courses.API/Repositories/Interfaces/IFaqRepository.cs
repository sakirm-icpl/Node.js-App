using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IFaqRepository : IRepository<Faq>
    {
        Task<ApiFaq> GetApiFaqByLcmsId(int lcmsId);
        Task<Message> PostFaq(ApiFaq apiFaq, int userId);
        Task<Message> PutFaq(ApiFaq apiFaq, int userId);
    }
}
