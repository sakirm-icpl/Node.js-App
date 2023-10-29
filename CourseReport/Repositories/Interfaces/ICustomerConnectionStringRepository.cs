using CourseReport.API.Data;
using CourseReport.API.Model;
using System.Threading.Tasks;

namespace CourseReport.API.Repositories.Interface
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        Task<string> GetConnectionString(string clientId);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        ReportDbContext GetDbContext();
        ReportDbContext GetDbContext(string ConnetionString);
    }
}
