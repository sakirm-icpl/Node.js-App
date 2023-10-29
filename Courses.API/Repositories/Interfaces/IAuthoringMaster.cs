using Courses.API.APIModel;
using Courses.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IAuthoringMaster : IRepository<AuthoringMaster>
    {
        Task<int> Count(string search = null);
        Task<List<object>> GetAll(int page, int pageSize, string search = null);
        Task<List<object>> GetDetailsByAuthoringId(int id, int page, int pageSize, int userId, int courseId = 0, string search = null);
        Task<ApiAuthoringMasterDetails> PostAuthoringDetails(ApiAuthoringMasterDetails apiAuthoringMasterDetails, int UserId);
        Task<object> DeleteAuthoringMasterDetails(AuthoringMasterDetails authoringMasterDetails);
        Task<object> DeleteAuthoringInteractiveVideoPopups(int id, int UserId);
        Task<object> UpdateAuthoringMasterDetails(AuthoringMasterDetails authoringMasterDetails, ApiAuthoringMasterDetailsUpdate apiAuthoringMasterDetails, int UserId);
        Task<int> GetPageNumber(int id);
        Task<AuthoringMasterDetails> GetAuthoringMasterDetails(int id);
        Task<bool> Exists(string Name, string Skills, int? Authoringid = null);
        Task<ApiResponse> PostAuthoringDetailsToLcms(ApiAuthoringMaster apiAuthoringMaster, int UserId);
        Task<int?> GetAuthoringMasterIdByModuleID(int ModuleId);
        Task<bool> IsDependacyExist(int authoringid);
        Task<bool> NameExists(string name, int? Authoringid = null);
        Task<bool> SkillExists(string skills, int? Authoringid = null);
        Task<List<AuthoringMasterDetails>> UpdatePageSequence(int authoringMasterId, List<ApiAuthoringMasterDetails> authoringMasterDetailsList, int userId);
        Task<AuthoringInteractiveVideoPopupsHistory> PostAuthoringInteractiveVideoPopupsHistory(ApiAuthoringInteractiveVideoPopupsHistory apiAuthoringInteractiveVideoPopups, int userId);
    }
}
