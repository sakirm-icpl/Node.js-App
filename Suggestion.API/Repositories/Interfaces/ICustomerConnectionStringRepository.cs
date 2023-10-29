using Suggestion.API.Data;
using Suggestion.API.Models;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
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
