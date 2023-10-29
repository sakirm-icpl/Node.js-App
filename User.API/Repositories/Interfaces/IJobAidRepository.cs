//======================================
// <copyright file="IJobAidRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IJobAidRepository : IRepository<JobAid>
    {
        Task<IEnumerable<JobAid>> GetAllJobAidSetting(int page, int pageSize, string search = null, string columnName = null);
        Task<int> Count(string search = null);
        Task<bool> Exist(string title);
        Task<int> GetTotalJobAidCount();
        Task<IEnumerable<JobAid>> Search(string q);
        Task<IEnumerable<APIJobAid>> GetAllRecordJobAid();
        Task<IEnumerable<APIJobAid>> GetAllJobAid(int page, int pageSize, string search = null, string columnName = null);
        Task<IEnumerable<APIJobAid>> GetJobAid(int id);
    }
}
