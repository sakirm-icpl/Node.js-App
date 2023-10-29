using Competency.API.APIModel;
using Competency.API.Model.Competency;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface IJdUploadRepository : IRepository<CompetencyJdUpload>
    {
        Task<CompetencyJdUpload> GetCompetencyJdUpload(int JobRoleId);
        Task<IEnumerable<APICompetencyJdUpload>> GetAllJdUpload(int page, int pageSize, string search, string columnName);

        public Task<int> GetAllJdCount();
    }
}
