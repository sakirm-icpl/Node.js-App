//======================================
// <copyright file="IHrmsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IHRMSRepository : IRepository<HRMS>
    {
        Task<IEnumerable<HRMS>> GetAllHrms(int page, int pageSize, string search);
        Task<int> Count(string search);
        Task<IEnumerable<string>> GetColumnNames(string search = null);
        Task<bool> IsColumnExist(string columnName = null);
    }
}
