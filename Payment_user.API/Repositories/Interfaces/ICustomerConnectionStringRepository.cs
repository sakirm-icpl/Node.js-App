using Payment.API.Data;
using Payment.API.Models;
using System.Threading.Tasks;

namespace Payment.API.Repositories.Interfaces
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        //Task<string> GetConnectionString(string clientId);
        //void SetDbContext(UserDbContext db);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        UserDbContext GetDbContext();
        UserDbContext GetDbContext(string ConnetionString);

        //UserDbContext GetDbContextByOrgCode(string orgCode);
    }
}
