using System.Threading.Tasks;
using User.API.Data;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        Task<string> GetConnectionString(string clientId);
        void SetDbContext(UserDbContext db);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        UserDbContext GetDbContext();
        UserDbContext GetDbContext(string ConnetionString);

        UserDbContext GetDbContextByOrgCode(string orgCode);
    }
}
