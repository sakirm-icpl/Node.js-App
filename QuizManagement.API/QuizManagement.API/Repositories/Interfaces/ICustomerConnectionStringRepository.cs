using QuizManagement.API.Data;
using QuizManagement.API.Models;
using System.Threading.Tasks;

namespace QuizManagement.API.Repositories.Interfaces
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        Task<string> GetConnectionString(string clientId);
        void SetDbContext(GadgetDbContext db);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        GadgetDbContext GetDbContext();
        GadgetDbContext GetDbContext(string ConnetionString);
    }
}
