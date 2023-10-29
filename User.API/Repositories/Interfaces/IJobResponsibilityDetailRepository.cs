//======================================
// <copyright file="IJobResponsibilityDetailRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IJobResponsibilityDetailRepository : IRepository<JobResponsibilityDetail>
    {
        Task<int> Count(string search = null);
        Task<IEnumerable<JobResponsibilityDetail>> GetAllJobResponsibilityDetail(int page, int pageSize, string search = null, string columnName = null);
        Task<IEnumerable<JobResponsibilityDetail>> Search(string q);
        bool Exist(int id, string search);
        Task<IEnumerable<APIJobResponsibilityDetail>> GetAllJobResponsibilityDetailRecord();
        Task<bool> ExistJob(int id, string search);

        Task<IEnumerable<APIJobResponsibilityDetail>> GetAllRoleResponsibility(int id);
    }
}
