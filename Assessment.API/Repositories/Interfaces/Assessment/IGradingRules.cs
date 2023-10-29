using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;

namespace Assessment.API.Repositories.Interface
{
    public interface IGradingRules : IRepository<GradingRules>
    {
        Task<IEnumerable<APIGradingRules>> GetAllGradingRules(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<int> GetTotalGradingRulesCount();
        Task<bool> Exist(string grade, int couseId, int ModelId);
        Task<IEnumerable<GradingRules>> Search(string query);
    }
}
