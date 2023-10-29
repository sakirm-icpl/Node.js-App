//======================================
// <copyright file="IRoleResponsibilityRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;


namespace User.API.Repositories.Interfaces
{
    public interface IJobResponsibilityRepository : IRepository<JobResponsibility>
    {
        Task<IEnumerable<JobResponsibility>> GetAllRoleResponsibility(int page, int pageSize, string search = null, string columnName = null);
        Task<IEnumerable<APIJobResponsibility>> GetAllRecordJobResponsibility(int page, int pageSize, string search = null);
        Task<IEnumerable<APIJobResponsibility>> GetAllRecordJobResponsibilityDescription(int userid);
        Task<int> Count(string search = null);
        Task<bool> Exist(string rolejobDescriptionCode);
        Task<int> GetTotalRoleResponsibilityCount();
        Task<IEnumerable<JobResponsibility>> Search(string q);
        Task<IEnumerable<APIJobResponsibility>> GetAllJobResponsibility();
        Task<IEnumerable<APIJobResponsibility>> GetKeyRoleResponsibility(int id);
        Task<APIJobResponsibility> JobResponsibilityUser(int id);

        Task<IEnumerable<APIJobResponsibility>> GetKeyRoleResponsibilityByUserId(int id);
    }
}
