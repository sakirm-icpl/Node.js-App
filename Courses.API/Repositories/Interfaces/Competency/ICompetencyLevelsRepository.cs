// ======================================
// <copyright file="ICompetencyLevelsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Courses.API.APIModel;
using Courses.API.APIModel.Competency;
using Courses.API.Model.Competency;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Courses.API.Model.ResponseModels;

namespace Courses.API.Repositories.Interfaces.Competency
{
    public interface ICompetencyLevelsRepository : IRepository<CompetencyLevels>
    {
        Task<IApiResponse> GetAllCompetencyLevels(int page, int pageSize, string search = null);
        Task<IApiResponse> GetNextLevel(int competencyID);
        Task<bool> Exists(string Level, int compId);
        Task<int> Count(string search = null);
        Task<IEnumerable<APICompetencyLevels>> GetCompetencyLevels();
        Task<IEnumerable<APICompetencyLevels>> GetCompetencyLevels(int id);
        Task<IEnumerable<APICompetencyLevels>> GetAllCompetencyLevelsCat(int? Catid, int? comId);
        bool IsDependacyExist(int levelId);


        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName);       
        Task<bool> CategoryExists(string category);
        Task<int> GetIdByCategory(string Category);
        Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMasterByID(int? id);
        Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMaster();

        int? competencyInCategoryCheck(IEnumerable<APICompetenciesMaster> competenciesByCatId, string comp);
        string LevelChecker(List<string> levelsList, string level);


    }
}
