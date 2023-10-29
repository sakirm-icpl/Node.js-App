using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IDesignationRoleRepository : IRepository<DesignationRoleMapping>
    {
        Task<IEnumerable<DesignationRoleMapping>> GetAllDesignations(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
    }
}
