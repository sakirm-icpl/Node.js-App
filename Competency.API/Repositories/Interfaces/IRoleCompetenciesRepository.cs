using Competency.API.APIModel;
using Competency.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Competency.API.Repositories.Interfaces
{
    public interface IRoleCompetenciesRepository : IRepository<RoleCompetency>
    {
        Task<IEnumerable<APIRoleCompetency>> GetRoleCompetency(int page, int pagesize, string search = null);
        Task<int> Count(string search = null);
        Task<RoleCompetency> CheckForDuplicate(int JobRoleId, int CompetencyLevelId, int CompetencyCategoryId, int CompetencyId,string OrgCode);
        Task<IEnumerable<APICompetencySkill>> GetRoleCompetencyForSearch(string search = null);
        Task<int[]> getIdByJobRoleId(int jobroleId);
        Task<int[]> GetRoleCompetencyForJobRole(string[] skills);

        Task<string[]> GetRoleCompetencyForJobRole1(string[] skills);



        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName);


        }
}
