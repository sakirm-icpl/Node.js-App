using Competency.API.APIModel;
using Competency.API.Model.Competency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Competency.API.Model.ResponseModels;

namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface ICompetencySubSubCategoryRepository : IRepository<CompetencySubSubCategory>
    {
        Task<IApiResponse> GetAllCompetencySubSubCategory(int page, int pageSize, int categoryId = 0, int subcategoryId = 0, string search = null);

        Task<bool> Exists(string Code, string subcategory, int? categoryId = null);
        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int userId, string organisationCode, string loginId, string userName);
    }
}
