using ILT.API.APIModel;
using ILT.API.Model.ILT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IModuleTopicAssociation : IRepository<ModuleTopicAssociation>
    {
        Task<bool> PostAssociation(APIModuleTopicAssociation aPIModuleTopicAssociation, int UserId);
        Task<List<APIModuleDetails>> Get(int page, int pageSize, string searchText);
        Task<int> GetCount(string searchText);
        Task<List<APITopicDetialsByModuleId>> GetTopicDetailsByModuleId(int moduleId);
        Task<List<ModuleTypeAhead>> GetModuleTypeAhead(string search = null);
        Task<List<ModuleTopicAssociation>> IsExists(int moduleId);
        Task<bool> CheckInAttendance(int moduleId);
    }
}
