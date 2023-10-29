using Survey.API.Data;
using Survey.API.Models;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
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
