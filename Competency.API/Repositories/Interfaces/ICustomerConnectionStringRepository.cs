using Competency.API.Model;
using Competency.API.Models;
using System.Threading.Tasks;

namespace Competency.API.Repositories.Interfaces
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        Task<string> GetConnectionString(string clientId);
        Task<string> GetConnectionStringByCode(string code);
        Task<string> GetConnectionStringByOrgnizationCodeForExternal(string OrgnizationCode);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        CourseContext GetDbContext();
        CourseContext GetDbContext(string ConnetionString);

        Task<string> GetConnectionStringByOrgnizationCodeNew(string OrgnizationCode);
    }
}
