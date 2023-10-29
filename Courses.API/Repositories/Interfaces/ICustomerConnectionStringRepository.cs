using Courses.API.Model;
using Courses.API.Models;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
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
