using Courses.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IAccessibilityRuleRejectedRepository : IRepository<AccessibilityRuleRejected>
    {
        void Delete();
        Task<IEnumerable<AccessibilityRuleRejected>> GetAllAccessibilityRuleReject(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
    }
}
