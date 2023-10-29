using Payment.API.APIModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payment.API.Repositories.Interfaces
{
    public interface IHRMSService
    {
        Task<APIHRMSResponseNew> MainHRMSProcess<T>(List<T> userlist, string OrgCode, string connectionString);
        //Task<APIHRMSResponse> GetHRMSProcessStatus(string connectionstring);
    }
}
