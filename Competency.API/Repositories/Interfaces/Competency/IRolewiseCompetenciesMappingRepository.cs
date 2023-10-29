// ======================================
// <copyright file="IRolewiseCompetenciesMappingRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Competency.API.APIModel.Competency;
using Competency.API.Model.Competency;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface IRolewiseCompetenciesMappingRepository : IRepository<RolewiseCompetenciesMapping>
    {
        Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllCompetenciesMapping(int page, int pageSize, string search = null);
        Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllCompetenciesMapping();
        Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllCompetenciesMapping(int id);
        Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllByRoleCompetenciesMapping(int roleid);
        Task<int> Count(string search = null);
        Task<bool> Exists(int roleid, int ComCatTd, int compId);
        Task<int> CountRole(int roleid);
        Task<RolewiseCompetenciesMapping> GetRecordRole(int roleid);
    }
}
