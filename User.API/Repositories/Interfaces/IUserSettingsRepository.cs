//======================================
// <copyright file="IUserSettingsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IUserSettingsRepository : IRepository<UserSettings>
    {
        Task<IEnumerable<UserSettings>> GetConfiguredColumnNames();
        Task<IEnumerable<object>> GetConfiguredColumnNamesOnly();
        Task<object> GetConfiguredColumnRole(string customerCode, string UserRole);
        Task<bool> ExistColumnName(string columnName);
        Task<IEnumerable<UserSettings>> Search(string suggestedName);
        Task<IEnumerable<APIUserSetting>> GetAllUserSetting(int page, int pageSize, string search = null, string columnName = null);
        Task<bool> Exist(string changedName);
        Task<int> GetSettingsCount(string search = null, string columnName = null);
        Task<APIUserSetting> GetUserSetting(int id);
        Task<IEnumerable<APIUserSetting>> GetUserSetting(string OrgCode);
        Task<int> GetSettingCount(string search = null, string columnName = null);
        Task<IEnumerable<APIUserSetting>> GetAllUserSettings(APIUserSearch objAPIUserSearch);
        Task<IEnumerable<APIUserSetting>> GetAllSettings(int page, int pageSize, string search = null, string columnName = null);
        Task<IEnumerable<APIUsersSetting>> GetAllSetting(int page, int pageSize, string search = null, string columnName = null);
        Task<IEnumerable<object>> GetColumnsForAccessibilty(string OrgCode);
        Task<IEnumerable<object>> GetColumnsForCompetencyJobRole();
        Task<Object> GetConfiguredColumnWithImplicitRoles(string customerCode, string userRole);
    }
}
