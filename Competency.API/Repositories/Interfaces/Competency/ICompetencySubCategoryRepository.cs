using Competency.API.Model.Competency;

using Competency.API.APIModel;
using Competency.API.APIModel.Competency;
using Competency.API.Model.Competency;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Competency.API.Model.ResponseModels;

namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface ICompetencySubCategoryRepository: IRepository<CompetencySubCategory>
    {
        Task<IApiResponse> GetAllCompetencySubCategory(int page, int pageSize,int categoryId = 0, string search = null);
        Task<bool> Exists(string code, string category);
        Task<IApiResponse> GetByCategoryId(int catID);
        bool IsDependacyExist(int categoryId);
        Task<IApiResponse> Search(int categoryId, string query);
        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int userId, string organisationCode, string loginId, string userName);
    }
}
