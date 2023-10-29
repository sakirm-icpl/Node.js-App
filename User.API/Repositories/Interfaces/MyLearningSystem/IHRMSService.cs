using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;

namespace User.API.Repositories.Interfaces
{
    public interface IHRMSService
    {
        Task<APIHRMSResponseNew> MainHRMSProcess<T>(List<T> userlist, string OrgCode, string connectionString);
        //Task<APIHRMSResponse> GetHRMSProcessStatus(string connectionstring);
    }
}
