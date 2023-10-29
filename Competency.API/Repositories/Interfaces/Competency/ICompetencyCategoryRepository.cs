// ======================================
// <copyright file="ICompetencyCategoryRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Competency.API.APIModel;
using Competency.API.APIModel.Competency;
using Competency.API.Model.Competency;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface ICompetencyCategoryRepository : IRepository<CompetencyCategory>
    {
        Task<IEnumerable<CompetencyCategory>> GetAllCompetencyCategory(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<bool> Exists(string code, string category, int? categoryId = null);
        Task<IEnumerable<CompetencyCategory>> Search(string query);
        bool IsDependacyExist(int categoryId);

        Task<List<APICompetencyChart>> GetCompetencySpiderchart(int userID);

        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName);
    }
}
