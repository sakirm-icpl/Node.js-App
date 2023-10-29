//======================================
// <copyright file="UserSettingsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.CacheManager;
using Microsoft.VisualBasic;

namespace User.API.Repositories
{
   
    public class UserSettingsRepository : Repository<UserSettings>, IUserSettingsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSettingsRepository));
        private UserDbContext _db;
        private readonly IConfiguration _configuration;
        public UserSettingsRepository(UserDbContext context, IConfiguration configuration) : base(context)
        {
            this._db = context;
            this._configuration = configuration;
        }

        public async Task<IEnumerable<UserSettings>> GetConfiguredColumnNames()
        {
            var configuredColumnName = (this._db.UserSettings
                .Where(s => s.IsConfigured == false)
                .OrderBy(s => s.Id)
                .Select(s =>
           new UserSettings
           {
               ConfiguredColumnName = s.ConfiguredColumnName,
               Id = s.Id
           }));

            return await configuredColumnName.ToListAsync();
        }

        public async Task<IEnumerable<object>> GetConfiguredColumnNamesOnly()
        {
            var configuredColumnName = (this._db.UserSettings
                .Where(s => s.IsConfigured == true)
                .OrderBy(s => s.Id)
                .Select(s =>
           new
           {
               ConfiguredColumnName = s.ConfiguredColumnName,
               ChangedColumnName = s.ChangedColumnName
           }));

            return await configuredColumnName.ToListAsync();
        }
        
        public async Task<IEnumerable<object>> GetColumnsForAccessibilty(string OrgCode)
        {
            var configuredColumnName = await (this._db.UserSettings
                .Where(s => s.IsConfigured == true)
                .OrderBy(s => s.Id)
                .Select(s =>
           new
           {
               Id = s.Id,
               ConfiguredColumnName = s.ConfiguredColumnName,
               ChangedColumnName = s.ChangedColumnName
           })).ToListAsync();
            configuredColumnName.Add(new
            {
                Id = 17,
                ConfiguredColumnName = "EmailId",
                ChangedColumnName = "Email Id"
            });
            configuredColumnName.Add(new
            {
                Id = 18,
                ConfiguredColumnName = "UserId",
                ChangedColumnName = "User Id"
            });
            configuredColumnName.Add(new
            {
                Id = 19,
                ConfiguredColumnName = "MobileNumber",
                ChangedColumnName = "Mobile Number"
            });
            configuredColumnName.Add(new
            {
                Id = 20,
                ConfiguredColumnName = "UserName",
                ChangedColumnName = "User Name"
            });
            if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
            {
                configuredColumnName.Add(new
                {
                    Id = 21,
                    ConfiguredColumnName = "DateOfJoining",
                    ChangedColumnName = "Date of Joining"
                });
            }

            return configuredColumnName;
        }
        public async Task<IEnumerable<object>> GetColumnsForCompetencyJobRole()
        {
            var configuredColumnName = await (this._db.UserSettings
                .Where(s => s.IsConfigured == true)
                .OrderBy(s => s.Id)
                .Select(s =>
           new
           {
               Id = s.Id,
               ConfiguredColumnName = s.ConfiguredColumnName,
               ChangedColumnName = s.ChangedColumnName
           })).ToListAsync();

            return configuredColumnName;
        }
        public async Task<Object> GetConfiguredColumnRole(string customerCode, string UserRole)
        {
            var cache = new User.API.CacheManager.CacheManager();
            string cacheKeyConfig = Helper.Constants.CacheKeyNames.SYSTEM_ROLES + "-" + customerCode.ToUpper();
            List<APIUserSettingRole> systemRoles;
            if (cache.IsAdded(cacheKeyConfig))
                systemRoles = cache.Get<List<APIUserSettingRole>>(cacheKeyConfig);
            else
            {
                systemRoles = await (this._db.UserSettings
                   .Where(s => s.IsConfigured == true && (s.ConfiguredColumnName == "Business" || s.ConfiguredColumnName == "Group" || s.ConfiguredColumnName == "Area" || s.ConfiguredColumnName == "Location"))
                   .OrderBy(s => s.Id)
                   .Select(s =>
               new APIUserSettingRole
               {
                   ChangedColumnName = s.ChangedColumnName + " Admin",
                   RoleCode = s.RoleCode

               }
              )).AsNoTracking().ToListAsync();
                cache.Add(cacheKeyConfig.ToUpper(), systemRoles, System.DateTimeOffset.Now.AddMinutes(Helper.Constants.CACHE_EXPIRED_TIMEOUT));
            }


            cacheKeyConfig = Helper.Constants.CacheKeyNames.ROLES + "-" + customerCode.ToUpper();
            List<APIUserSettingRole> roles;
            if (cache.IsAdded(cacheKeyConfig))
                roles = cache.Get<List<APIUserSettingRole>>(cacheKeyConfig);
            else
            {
                roles = await (this._db.Roles
                      .Where(s => s.IsDeleted == 0 && s.IsImplicitRole == false && s.RoleCode.ToUpper() != "PM")
                      .OrderBy(s => s.Id)
                      .Select(s =>
                  new APIUserSettingRole
                  {
                      ChangedColumnName = s.RoleName,
                      RoleCode = s.RoleCode
                  }
                  )).ToListAsync();
                cache.Add(cacheKeyConfig.ToUpper(), roles, System.DateTimeOffset.Now.AddMinutes(Helper.Constants.CACHE_EXPIRED_TIMEOUT));
            }


            //for system columns use UserSettings changed name 
            foreach (var role in roles)
            {
                var result = systemRoles.Where(A => A.RoleCode.ToLower() == role.RoleCode.ToLower()).Select(B => new { B.RoleCode, B.ChangedColumnName }).FirstOrDefault();
                if (result != null)
                {
                    role.RoleCode = role.RoleCode;
                    role.ChangedColumnName = result.ChangedColumnName;
                }

            }

            return roles;
        }
        public async Task<bool> Exist(string changedName)
        {
            changedName = Regex.Replace(changedName, @"\s+", "");
            string[] nonConfigurableColumns = this._configuration["NonConfigurableColumns"].Split(",");
            for (int i = 0; i < nonConfigurableColumns.Length; i++)
            {
                if (string.Equals(changedName.Trim(), nonConfigurableColumns[i], StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            var count = await this._db.UserSettings.Where(s => s.ChangedColumnName.ToLower() == changedName.ToLower() && s.IsConfigured == true).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> ExistColumnName(string columnName)
        {
            if (columnName == "Area")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.AreaId != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "Business")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.BusinessId != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "Group")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.GroupId != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "Location")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.LocationId != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn1")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn1 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn2")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn2 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn3")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn3 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn4")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn4 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn5")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn5 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn6")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn6 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn7")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn7 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn8")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn8 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn9")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn9 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn10")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn10 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn11")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn11 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            else
                 if (columnName == "ConfigurationColumn12")
            {
                var count = await this._db.UserMasterDetails.Where(s => s.ConfigurationColumn12 != null).CountAsync();
                if (count > 0)
                    return true;
            }
            return false;
        }
        public async Task<IEnumerable<UserSettings>> Search(string configuredColumnName)
        {

            var result = (from userSetting in this._db.UserSettings
                          where (userSetting.ConfiguredColumnName.StartsWith(configuredColumnName) || userSetting.ChangedColumnName.StartsWith(configuredColumnName))
                          select userSetting).ToListAsync();
            return await result;
        }
        public async Task<IEnumerable<APIUserSetting>> GetAllUserSetting(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                var userSetting = (from userSettings in this._db.UserSettings
                                   where (userSettings.IsDeleted == Record.NotDeleted && userSettings.IsConfigured == true)
                                   select new APIUserSetting
                                   {
                                       Id = userSettings.Id,
                                       ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                       ChangedColumnName = userSettings.ChangedColumnName,
                                       IsConfigured = userSettings.IsConfigured,
                                       IsMandatory = userSettings.IsMandatory,
                                       IsShowInReport = userSettings.IsShowInReport,
                                       IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard,
                                       FieldType = userSettings.FieldType
                                   });
                if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(columnName))
                {
                    if (columnName.ToLower().Equals("changedcolumnname"))
                        userSetting = userSetting.Where(u => u.ChangedColumnName.ToLower().StartsWith(search.ToLower()));
                    if (columnName.ToLower().Equals("configuredcolumnname"))
                        userSetting = userSetting.Where(u => u.ConfiguredColumnName.ToLower().StartsWith(search.ToLower()));

                }

                if (page != -1)
                    userSetting = userSetting.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    userSetting = userSetting.Take(pageSize);

                return await userSetting.AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return null;
        }
        public async Task<IEnumerable<APIUserSetting>> GetAllUserSettings(APIUserSearch objAPIUserSearch)
        {
            try
            {
                var userSetting = (from userSettings in this._db.UserSettings
                                   where (userSettings.IsDeleted == Record.NotDeleted && userSettings.IsConfigured == true)
                                   select new APIUserSetting
                                   {
                                       Id = userSettings.Id,
                                       ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                       ChangedColumnName = userSettings.ChangedColumnName,
                                       IsConfigured = userSettings.IsConfigured,
                                       IsMandatory = userSettings.IsMandatory,
                                       IsShowInReport = userSettings.IsShowInReport,
                                       IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard,
                                       FieldType = userSettings.FieldType
                                   });
                if (!string.IsNullOrWhiteSpace(objAPIUserSearch.Search) && !string.IsNullOrWhiteSpace(objAPIUserSearch.ColumnName))
                {
                    if (objAPIUserSearch.ColumnName.ToLower().Equals("changedcolumnname"))
                        userSetting = userSetting.Where(u => u.ChangedColumnName.ToLower().StartsWith(objAPIUserSearch.Search.ToLower()));
                    if (objAPIUserSearch.ColumnName.ToLower().Equals("configuredcolumnname"))
                        userSetting = userSetting.Where(u => u.ConfiguredColumnName.ToLower().StartsWith(objAPIUserSearch.Search.ToLower()));

                }

                if (objAPIUserSearch.Page != -1)
                    userSetting = userSetting.Skip((objAPIUserSearch.Page - 1) * objAPIUserSearch.PageSize);

                if (objAPIUserSearch.PageSize != -1)
                    userSetting = userSetting.Take(objAPIUserSearch.PageSize);

                return await userSetting.AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<IEnumerable<APIUserSetting>> GetAllSettings(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                var userSetting = (from userSettings in this._db.UserSettings
                                   where (userSettings.IsDeleted == Record.NotDeleted)
                                   select new APIUserSetting
                                   {
                                       Id = userSettings.Id,
                                       ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                       ChangedColumnName = userSettings.ChangedColumnName,
                                       IsConfigured = userSettings.IsConfigured,
                                       IsMandatory = userSettings.IsMandatory,
                                       IsShowInReport = userSettings.IsShowInReport,
                                       IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard,
                                       FieldType = userSettings.FieldType,
                                       IsConfidential = userSettings.IsConfidential,
                                       IsUnique = userSettings.IsUnique,
                                       IsShowInReportFilter = userSettings.IsShowInReportFilter
                                   });
                if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(columnName))
                {
                    if (columnName.ToLower().Equals("changedcolumnname"))
                        userSetting = userSetting.Where(u => u.ChangedColumnName.ToLower().StartsWith(search.ToLower()));
                    if (columnName.ToLower().Equals("configuredcolumnname"))
                        userSetting = userSetting.Where(u => u.ConfiguredColumnName.ToLower().StartsWith(search.ToLower()));

                }


                if (page != -1)
                    userSetting = userSetting.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    userSetting = userSetting.Take(pageSize);

                return await userSetting.AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return null;
        }
        public async Task<int> GetSettingCount(string search = null, string columnName = null)
        {
            var userSetting = (from userSettings in this._db.UserSettings
                               where (userSettings.IsDeleted == Record.NotDeleted && userSettings.IsConfigured == true)
                               select new APIUserSetting
                               {
                                   Id = userSettings.Id,
                                   ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                   ChangedColumnName = userSettings.ChangedColumnName,
                                   IsConfigured = userSettings.IsConfigured,
                                   IsMandatory = userSettings.IsMandatory,
                                   IsShowInReport = userSettings.IsShowInReport,
                                   FieldType = userSettings.FieldType
                               });
            if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(columnName))
            {
                if (columnName.ToLower().Equals("changedcolumnname"))
                    userSetting = userSetting.Where(u => u.ChangedColumnName.ToLower().StartsWith(search.ToLower()));
                if (columnName.ToLower().Equals("configuredcolumnname"))
                    userSetting = userSetting.Where(u => u.ConfiguredColumnName.ToLower().StartsWith(search.ToLower()));
            }


            return await userSetting.AsNoTracking().CountAsync();
        }
        public async Task<IEnumerable<APIUsersSetting>> GetAllSetting(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                var userSetting = (from userSettings in this._db.UserSettings
                                   where (userSettings.IsDeleted == Record.NotDeleted && userSettings.IsConfigured == true)
                                   select new APIUsersSetting
                                   {
                                       ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                       ChangedColumnName = userSettings.ChangedColumnName,
                                       IsConfigured = userSettings.IsConfigured,
                                       IsMandatory = userSettings.IsMandatory,
                                       FieldType = userSettings.FieldType,
                                       IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard,
                                       IsConfidential = userSettings.IsConfidential,
                                       IsUnique = userSettings.IsUnique
                                   });

                if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(columnName))
                {
                    if (columnName.ToLower().Equals("changedcolumnname"))
                        userSetting = userSetting.Where(u => u.ChangedColumnName.ToLower().StartsWith(search.ToLower()));
                    if (columnName.ToLower().Equals("configuredcolumnname"))
                        userSetting = userSetting.Where(u => u.ConfiguredColumnName.ToLower().StartsWith(search.ToLower()));
                }

                if (page != -1)
                    userSetting = userSetting.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    userSetting = userSetting.Take(pageSize);

                return await userSetting.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return null;
        }
        public async Task<APIUserSetting> GetUserSetting(int id)
        {
            try
            {
                var userSetting = (from userSettings in this._db.UserSettings
                                   where (userSettings.IsDeleted == Record.NotDeleted && userSettings.Id == id && userSettings.IsConfigured == true)
                                   select new APIUserSetting
                                   {
                                       Id = userSettings.Id,
                                       ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                       ChangedColumnName = userSettings.ChangedColumnName,
                                       IsConfigured = userSettings.IsConfigured,
                                       IsMandatory = userSettings.IsMandatory,
                                       IsShowInReport = userSettings.IsShowInReport,
                                       IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard,
                                       FieldType = userSettings.FieldType
                                   });
                return await userSetting.SingleAsync();
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<IEnumerable<APIUserSetting>> GetUserSetting(string OrgCode)
        {
            try
            {
                List<APIUserSetting> allUserSetting;
                var cache = new CacheManager.CacheManager();
                string allUSerSettingsKey = "UserAPIUserSetting" + OrgCode;
                if (cache.IsAdded(allUSerSettingsKey))
                {
                    allUserSetting = cache.Get<List<APIUserSetting>>(allUSerSettingsKey);
                }
                else
                {
                    var userSetting = (from userSettings in this._db.UserSettings
                                       where (userSettings.IsDeleted == Record.NotDeleted && userSettings.IsConfigured == true)
                                       select new APIUserSetting
                                       {
                                           Id = userSettings.Id,
                                           ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                           ChangedColumnName = userSettings.ChangedColumnName,
                                           IsConfigured = userSettings.IsConfigured,
                                           IsMandatory = userSettings.IsMandatory,
                                           IsShowInReport = userSettings.IsShowInReport,
                                           IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard,
                                           FieldType = userSettings.FieldType
                                       });
                    allUserSetting = await userSetting.ToListAsync();
                    if (allUserSetting == null)
                        allUserSetting = new List<APIUserSetting>();
                    cache.Add<List<APIUserSetting>>(allUSerSettingsKey, allUserSetting);
                }
                return allUserSetting;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }
        public async Task<int> GetSettingsCount(string search = null, string columnName = null)
        {
            var userSetting = (from userSettings in this._db.UserSettings
                               where (userSettings.IsDeleted == Record.NotDeleted)
                               select new APIUserSetting
                               {
                                   Id = userSettings.Id,
                                   ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                   ChangedColumnName = userSettings.ChangedColumnName,
                                   IsConfigured = userSettings.IsConfigured,
                                   IsMandatory = userSettings.IsMandatory,
                                   IsShowInReport = userSettings.IsShowInReport,
                                   FieldType = userSettings.FieldType
                               });
            if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(columnName))
            {
                if (columnName.ToLower().Equals("changedcolumnname"))
                    userSetting = userSetting.Where(u => u.ChangedColumnName.ToLower().StartsWith(search.ToLower()));
                if (columnName.ToLower().Equals("configuredcolumnname"))
                    userSetting = userSetting.Where(u => u.ConfiguredColumnName.ToLower().StartsWith(search.ToLower()));
            }


            return await userSetting.AsNoTracking().CountAsync();
        }
        public async Task<Object> GetConfiguredColumnWithImplicitRoles(string customerCode, string userRole)
        {
            string entrallCustomerCode = "enthralltech";
            string entrallCustomerCodeNew = "ent";
            var roles = new List<APIUserSettingRole>();


            {

                roles = await (this._db.UserSettings
                      .Where(s => s.IsConfigured == true && (s.ConfiguredColumnName == "Business" || s.ConfiguredColumnName == "Group" || s.ConfiguredColumnName == "Area" || s.ConfiguredColumnName == "Location"))
                      .OrderBy(s => s.Id)
                      .Select(s =>
                  new APIUserSettingRole
                  {
                      ChangedColumnName = s.ChangedColumnName + " Admin",
                      RoleCode = s.RoleCode

                  }

                 )).ToListAsync();

                roles.Add(new APIUserSettingRole
                {
                    ChangedColumnName = "Supervisor",
                    RoleCode = "SP"
                });
                roles.Add(new APIUserSettingRole
                {
                    ChangedColumnName = "Client Admin",
                    RoleCode = "CA"
                });

                if (customerCode.ToLower().Equals(entrallCustomerCode) || customerCode.ToLower().Equals(entrallCustomerCodeNew))
                {
                    if (!userRole.ToLower().Equals("ca"))
                    {
                        roles.Add(new APIUserSettingRole
                        {
                            ChangedColumnName = "Account Manager",
                            RoleCode = "AM"
                        });
                        roles.Add(new APIUserSettingRole
                        {
                            ChangedColumnName = "Product Manager",
                            RoleCode = "PM"
                        });
                    }

                }
            }
            return roles;
        }

    }
}
