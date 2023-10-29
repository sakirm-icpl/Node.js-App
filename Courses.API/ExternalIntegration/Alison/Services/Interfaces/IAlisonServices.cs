//using AlisonServicesNew;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalIntegration.MetaData;

namespace ExternalIntegration.Services.Interfaces
{
    public interface IAlisonServices
    {
        //Task<AlisonWebServicePortTypeClient> GetInstanceAsync();
        Task<string> GetLogin(RequestLogin requestLogin);
        List<APIAlisonCoursesDetails> GetAvailableCourses(int offset, int count);
        Task<List<APIAlisonCoursesDetails>> GetMyCoursesDetailed(string emailID);
        Task<List<APIAlisonCourses>> GetMyCourses(string emailID);
    }
}
