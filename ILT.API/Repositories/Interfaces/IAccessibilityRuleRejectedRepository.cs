using ILT.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IAccessibilityRuleRejectedRepository : IRepository<AccessibilityRuleRejected>
    {
        void Delete();
        Task<IEnumerable<AccessibilityRuleRejected>> GetAllAccessibilityRuleReject(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
    }
}
