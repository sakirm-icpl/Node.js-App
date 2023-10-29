using Gadget.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IOrganizationMessagesRepository : IRepository<OrganizationMessages>
    {
        Task<List<OrganizationMessages>> GetAllOrganizationMessages(int page, int pageSize, string search = null);
        Task<int> CountOrganizationMessages(string search = null);
        Task<bool> ExistOrganizationMessage(string Description);


    }

}