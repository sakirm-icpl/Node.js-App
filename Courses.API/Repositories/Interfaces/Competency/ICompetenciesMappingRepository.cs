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

namespace Courses.API.Repositories.Interfaces.Competency
{
    public interface ICompetenciesMappingRepository : IRepository<CompetenciesMapping>
    {
        Task<IEnumerable<APICompetenciesMapping>> GetAllCompetenciesMapping(int page, int pageSize, string search = null);
        Task<IEnumerable<APICompetenciesMapping>> GetAllCompetenciesMappingByCourse(int courseId);
        Task<IEnumerable<APICompetencyWiseCourses>> GetCompetencyWiseCourses(int? comId);
        Task<int> Count(string search = null);
        Task<bool> Exists(int courseId,  int compId, int? id = null);
        Task<IEnumerable<CompetenciesMapping>> GetCompetency(int courseId);
        Task<int> CountCourse(int courseid);
        Task<CompetenciesMapping> GetRecordCourse(int courseid);
        Task<IEnumerable<APICompetenciesMapping>> GetAllCategoryWiseCompetencies(int page, int pageSize, int? jobroleid, string search = null);
        Task<APICategorywiseCompetenciesDetails> GetCompetanciesDetail(int competencyMappingId, int courseId, int? moduleId, int userId);
        Task<int> GetAllCategoryWiseCompetenciesCount(string search = null,int? jobroleid = null);
        Task<int[]> getCompIdByCourseId(int CourseId);
        void FindElementsNotInArray(int[] CurrentCompetencies, int[] aPIOldCompetenciesId, int CourseId);
        Task CompetenciesMappingAuditlog(CompetenciesMapping competenciesMapping,string action);

        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName);
    }
}
