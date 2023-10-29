using WallFeed.API.Data;
using WallFeed.API.Models;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories.Interfaces
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
