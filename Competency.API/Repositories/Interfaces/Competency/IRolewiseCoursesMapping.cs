// ======================================
// <copyright file="IRolewiseCompetenciesMappingRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public interface IRolewiseCoursesMapping : IRepository<RolewiseCourseMapping>
    {
        Task<IEnumerable<APIRolewiseCoursesMappingDetails>> GetAllRoleCoursesMapping(int page, int pageSize, string search = null, string filter = null);
        Task<IEnumerable<APIRolewiseCoursesMappingDetails>> GetAllCoursesMapping();
        Task<int> Count(string search = null, string filter = null);
        Task<bool> Exists(int roleId, int CourseID, int? RolewiseCourseID = null);


        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName);
        
    }
}
