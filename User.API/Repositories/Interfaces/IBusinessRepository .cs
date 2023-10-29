//======================================
// <copyright file="IBusinessRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IBusinessRepository : IRepository<Business>
    {
        Task<IEnumerable<string>> GetBusinessNames();
        Task<int> GetIdIfExist(string businessName);
        Task<int> GetLastInsertedId();
        Task<string> GetBuisnessNameById(int? locationId);
        Task<IEnumerable<Business>> GetAllBusiness(string search);
        Task<List<object>> GetAll(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);

        Task<bool> Exists(string Name, string Code, int? businessid = null);
        Task<List<Business>> GetBusiness();
        Task<bool> IsDependacyExist(int BusinessId);
    }
}
