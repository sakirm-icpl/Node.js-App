using ILT.API.APIModel;
using ILT.API.Model.ILT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IILTOnlineSetting : IRepository<ILTOnlineSetting>
    {
        Task<List<APIILTOnlineSetting>> GetAllOnlineSetting(int page, int pageSize, string searchText);
        Task<int> GetILTOnlineSettingCount(string searchText = null);

        Task<int> Exists(string name);
    }
}
