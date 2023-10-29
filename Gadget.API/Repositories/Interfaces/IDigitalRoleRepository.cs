using Gadget.API.APIModel;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IDigitalRolesList : IRepository<DigitalRole>
    {
        Task<APIDigitalRolesListandCount> GetDigitalRoles(int page, int pageSize, string search);
    }
}