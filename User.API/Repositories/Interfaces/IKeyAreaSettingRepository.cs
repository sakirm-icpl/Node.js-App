//======================================
// <copyright file="IKeyAreaSettingRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IKeyAreaSettingRepository : IRepository<KeyAreaSetting>
    {
        Task<IEnumerable<KeyAreaSetting>> GetAllKeyAreaSetting(int page, int pageSize, string search = null, string columnName = null);
        Task<IEnumerable<APIKeyAreaSetting>> GetAllKeyArea(int UserId, int page, int pageSize, string search = null);
        Task<IEnumerable<APIKeyAreaSetting>> GetKeyArea(int id);
        Task<IEnumerable<APIKeyAreaSetting>> GetAllRecordKeyArea();
        Task<int> Count(int UserId, string search = null);
        Task<bool> Exist(string rolejobDescriptionCode, int userid);
        Task<int> GetTotalKeyAreaSettingCount();
        Task<IEnumerable<KeyAreaSetting>> Search(string q);
        Task<IEnumerable<APIKeyAreaSetting>> GetAllKeyAreaSettingRecord();
        Task<IEnumerable<APIKeyAreaSetting>> GetKeyAreaSetting(int userid);

        Task<IEnumerable<APIKeyAreaSetting>> GetMyKeyAreaSetting(int userid, int page, int pageSize);
        Task<int> GetMyKeyAreaSettingCount(int userid);

    }
}
