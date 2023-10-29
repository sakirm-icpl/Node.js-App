using Assessment.API.Model;
using Assessment.API.Models;

namespace Assessment.API.Repositories.Interfaces
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        Task<string> GetConnectionString(string clientId);
        Task<string> GetConnectionStringByCode(string code);
        Task<string> GetConnectionStringByOrgnizationCodeForExternal(string OrgnizationCode);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        AssessmentContext GetDbContext();
        AssessmentContext GetDbContext(string ConnetionString);

        Task<string> GetConnectionStringByOrgnizationCodeNew(string OrgnizationCode);
    }
}
