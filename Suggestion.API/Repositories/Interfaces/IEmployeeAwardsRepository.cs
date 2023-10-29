using Suggestion.API.APIModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface IEmployeeAwards : IRepository<EmployeeAwards>
    {
        Task<APIEmployeeAwardsListandCount> GetEmployeeAwards(int page, int pageSize, string search);
        Task<List<EmployeeAwardsGet>> GetEmployeeAwardsByUserId(int userId);
        Task<List<EmployeeAwardsGet>> BestAwardDashboard(APIFilter filterData);
        string CheckAwardExist(APIEmployeeAwards data);

    }
}