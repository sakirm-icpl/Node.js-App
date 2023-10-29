using CourseApplicability.API.Model;
using CourseApplicability.API.Models;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        Task<string> GetConnectionString(string clientId);
        Task<string> GetConnectionStringByCode(string code);
        Task<string> GetConnectionStringByOrgnizationCodeForExternal(string OrgnizationCode);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        CoursesApplicabilityContext GetDbContext();
        CoursesApplicabilityContext GetDbContext(string ConnetionString);

        Task<string> GetConnectionStringByOrgnizationCodeNew(string OrgnizationCode);
    }
}
