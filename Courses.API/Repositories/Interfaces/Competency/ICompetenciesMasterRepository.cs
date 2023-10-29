// ======================================
// <copyright file="ICompetenciesMasterRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public interface ICompetenciesMasterRepository : IRepository<CompetenciesMaster>
    {
        Task<IEnumerable<CompetenciesMaster>> GetAllCompetenciesMaster(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<bool> Exists(string code, string category, int categoryid);
        Task<bool> ExistsRecord(int? id, string code);
        Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMaster();
        Task<IApiResponse> GetCompetenciesMaster(int page, int pageSize, string search = null);

        Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMaster(int id);
        Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMasterByID(int? id);
        bool IsDependacyExist(int compId);
        Task<bool> Exists(string CompetencyName);
         void FindElementsNotInArray(int[] roleCompetencies, int[] aPIJobRoleForIds,int jobroleId);
        Task<string> GetIdByCompetencyName(string CompetencyName);
        Task<int> ExistsRecordForJobrole(string Code);
        Task CompetenciesMasterAuditlog(CompetenciesMaster competenciesMaster,string action);


        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName);
    }
}
