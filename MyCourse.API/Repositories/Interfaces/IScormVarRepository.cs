using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IScormVarRepository : IRepository<ScormVars>
    {
        Task<bool> Exists(string name);
        Task<string> GetScorm(string varName, int UserId, int courseId, int moduleId);
        Task<int> Count(string varName, int UserId, int courseId, int moduleId);
        Task<ScormVars> Get(string varName, int UserId, int courseId, int moduleId);
        Task<List<ApiScormPost>> GetScormForMobile(int UserId, int courseId, int moduleId);
        Task<string> DeleteScorm(int UserId, int courseId, int moduleId);
        Task<string> ClearBookmarkingData(APIClearScorm obj, int ModifiedBy, DateTime ModifiedDate,int UserID);
        Task<ApiResponse> GetModules(int search);
        Task<IEnumerable<APIBookMarkingData>> GetAllBookmarkingClearData();
        Task<List<APIBookMarkingData>> GetClearBookMarkingViewData(int? page, int? pageSize);
        Task<int> GetClearBookMarkingViewDataCount();

    }
}
