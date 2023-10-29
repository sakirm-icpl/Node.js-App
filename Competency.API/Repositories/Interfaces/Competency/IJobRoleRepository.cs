using Competency.API.APIModel;
using Competency.API.Model.Competency;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface IJobRoleRepository : IRepository<CompetencyJobRole>
    {
        Task<bool> ExistsRecord(int? id, string Code, string Name);

        Task<IQueryable<APIViewCompetencyJobRole>> GetCompetencyJobRole(string orgcode, int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<bool> IsDependacyExist(int jobroleid);
        Task<IEnumerable<APIJobRole>> GetAllJobRoles();
        Task<bool> Exists(int JobRoleId, int CompetencyId);
        Task<List<CompetenciesMaster>> GetTypeAheadForJobRole(string jobroleId);
        Task<int> GetIdByValue(string Value);
        Task<string> GetValueById(int Value);
        Task<NextJobRoles> PostNextJobRoleDetails(NextJobRoles apiNextJobRoles);
        Task<List<APIRoles>> GetTypeAhead();
        Task<bool> UpdateNextJobrole(int[] NextJobrole, int jobroleId, int UserId);
        Task<IEnumerable<APIMasterTestCourse>> GetMasterTestCourseDetails(int userId, int? jobRoleId = null);
        Task<APICompetencyJobRole> GetCompetencyJobRoleById(string orgcode, int JobRoleId);
        Task<bool> DeleteUserCarrerJobRole(int id, int userid);
        Task<bool> SendNotificationForManagerApproval();
        Task<bool> ExistsForJobRole(string name);
    }

}
