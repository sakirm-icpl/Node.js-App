//======================================
// <copyright file="KeyAreaSettingRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Microsoft.EntityFrameworkCore;
using System;
using log4net;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class KeyAreaSettingRepository : Repository<KeyAreaSetting>, IKeyAreaSettingRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(KeyAreaSettingRepository));
        private UserDbContext _db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public KeyAreaSettingRepository(UserDbContext context, ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this._customerConnectionString = customerConnectionString;
            this._db = context;
        }
        public async Task<IEnumerable<KeyAreaSetting>> GetAllKeyAreaSetting(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from keyAreaSetting in context.KeyAreaSetting
                                  where keyAreaSetting.IsDeleted == Record.NotDeleted
                                  select new KeyAreaSetting
                                  {
                                      Id = keyAreaSetting.Id,
                                      UserId = keyAreaSetting.UserId,
                                      KeyAreaUserId = keyAreaSetting.KeyAreaUserId,
                                      KeyResultAreaDescription = keyAreaSetting.KeyResultAreaDescription,
                                      AdditionalDescription = keyAreaSetting.AdditionalDescription,
                                      ModifiedBy = keyAreaSetting.ModifiedBy,
                                      ModifiedDate = keyAreaSetting.ModifiedDate
                                  });
                    if (!string.IsNullOrEmpty(search))
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            switch (columnName.ToLower())
                            {
                                case "jobdescription":
                                    result = result.Where(c => c.KeyResultAreaDescription.StartsWith(search));
                                    break;
                                case "additionaldescription":
                                    result = result.Where(c => c.AdditionalDescription.StartsWith(search));
                                    break;
                            }
                        }

                    if (page != -1)
                    {
                        result = result.Skip((page - 1) * pageSize);
                        result = result.OrderByDescending(v => v.Id);
                    }

                    if (pageSize != -1)
                    {
                        result = result.Take(pageSize);
                        result = result.OrderByDescending(v => v.Id);
                    }

                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int> Count(int UserId, string search = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[GetKeyAreaSettingCount]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            connection.Close();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }

        public async Task<bool> Exist(string jobDescription, int userId)
        {

            var count = await this._db.KeyAreaSetting.Where(r => (r.KeyResultAreaDescription.ToLower() == jobDescription.ToLower()) && r.IsDeleted == Record.NotDeleted).CountAsync();
            var countUserID = await this._db.KeyAreaSetting.Where(r => (Convert.ToString(r.KeyAreaUserId).ToLower() == Convert.ToString(userId).ToLower()) && r.IsDeleted == Record.NotDeleted).CountAsync();
            if (count > 0 && countUserID > 0)
                return true;
            return false;
        }

        public async Task<int> GetTotalKeyAreaSettingCount()
        {
            return await this._db.KeyAreaSetting.CountAsync();
        }

        public async Task<IEnumerable<KeyAreaSetting>> Search(string q)
        {

            var result = (from keyAreaSett in this._db.KeyAreaSetting
                          where ((keyAreaSett.KeyResultAreaDescription.StartsWith(q) || Convert.ToString(keyAreaSett.UserId).StartsWith(q) || keyAreaSett.AdditionalDescription.StartsWith(q) || keyAreaSett.KeyResultAreaDescription.StartsWith(q) || Convert.ToString(keyAreaSett.KeyAreaUserId).StartsWith(q)) && keyAreaSett.IsDeleted == 0)
                          select keyAreaSett).ToListAsync();
            return await result;
        }

        public async Task<IEnumerable<APIKeyAreaSetting>> GetAllKeyArea(int UserId, int page, int pageSize, string search = null)
        {

            List<APIKeyAreaSetting> keyAreaSettings = new List<APIKeyAreaSetting>();
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetKeyAreaSetting";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                var keyAreaSetting = new APIKeyAreaSetting
                                {
                                    Id = Convert.ToInt32(row["Id"].ToString()),
                                    KeyAreaUserId = Convert.ToInt32(row["KeyAreaUserId"].ToString()),
                                    KeyResultAreaDescription = row["KeyResultAreaDescription"].ToString(),
                                    AdditionalDescription = row["AdditionalDescription"].ToString(),
                                    ModifiedDate = string.IsNullOrEmpty(row["ModifiedDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["ModifiedDate"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    IsAllowEdit = Convert.ToBoolean(Convert.ToInt32(row["IsAllowEdit"].ToString())),
                                    AssignedByUserName = row["AssignedByUserName"].ToString(),
                                };
                                keyAreaSettings.Add(keyAreaSetting);
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                    return keyAreaSettings;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<IEnumerable<APIKeyAreaSetting>> GetKeyArea(int id)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from keyAreasett in context.KeyAreaSetting
                                  join userMaster in context.UserMaster on keyAreasett.UserId equals userMaster.Id
                                  where (keyAreasett.Id == id)
                                  join userMasterNew in context.UserMaster on keyAreasett.CreatedBy equals userMasterNew.Id
                                  where (keyAreasett.Id == id)
                                  select new APIKeyAreaSetting
                                  {
                                      Id = keyAreasett.Id,
                                      KeyAreaUserId = keyAreasett.KeyAreaUserId,
                                      KeyResultAreaDescription = keyAreasett.KeyResultAreaDescription,
                                      AdditionalDescription = keyAreasett.AdditionalDescription,
                                      UserName = userMaster.UserName,
                                      AssignedByUserName = userMasterNew.UserName,
                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APIKeyAreaSetting>> GetAllRecordKeyArea()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from keyAreasett in context.KeyAreaSetting
                                  join userMaster in context.UserMaster on keyAreasett.UserId equals userMaster.Id
                                  where (keyAreasett.IsDeleted == Record.NotDeleted)
                                  select new APIKeyAreaSetting
                                  {
                                      Id = keyAreasett.Id,
                                      KeyAreaUserId = keyAreasett.KeyAreaUserId,
                                      KeyResultAreaDescription = keyAreasett.KeyResultAreaDescription,
                                      AdditionalDescription = keyAreasett.AdditionalDescription,
                                      UserName = userMaster.UserName,
                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<IEnumerable<APIKeyAreaSetting>> GetAllKeyAreaSettingRecord()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from keyAreaSetting in context.KeyAreaSetting
                                  join userMaster in context.UserMaster on keyAreaSetting.ModifiedBy equals userMaster.Id
                                  where (keyAreaSetting.IsDeleted == Record.NotDeleted)
                                  select new APIKeyAreaSetting
                                  {
                                      Id = keyAreaSetting.Id,
                                      KeyResultAreaDescription = keyAreaSetting.KeyResultAreaDescription,
                                      AdditionalDescription = keyAreaSetting.AdditionalDescription,
                                      UserName = userMaster.UserName

                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APIKeyAreaSetting>> GetKeyAreaSetting(int userid)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from keyAreaSetting in context.KeyAreaSetting
                                  join userMaster in context.UserMaster on keyAreaSetting.ModifiedBy equals userMaster.Id
                                  where (keyAreaSetting.IsDeleted == Record.NotDeleted && keyAreaSetting.UserId == userid)
                                  select new APIKeyAreaSetting
                                  {
                                      Id = keyAreaSetting.Id,
                                      KeyResultAreaDescription = keyAreaSetting.KeyResultAreaDescription,
                                      AdditionalDescription = keyAreaSetting.AdditionalDescription,
                                      ModifiedDate = keyAreaSetting.ModifiedDate,
                                      KeyAreaUserId = keyAreaSetting.KeyAreaUserId,

                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;

        }

        public async Task<IEnumerable<APIKeyAreaSetting>> GetMyKeyAreaSetting(int userid, int page, int pageSize)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from keyAreaSetting in context.KeyAreaSetting
                                  join userMaster in context.UserMaster on keyAreaSetting.UserId equals userMaster.Id
                                  where (keyAreaSetting.IsDeleted == Record.NotDeleted && keyAreaSetting.UserId == userid)
                                  select new APIKeyAreaSetting
                                  {
                                      Id = keyAreaSetting.Id,
                                      KeyResultAreaDescription = keyAreaSetting.KeyResultAreaDescription,
                                      AdditionalDescription = keyAreaSetting.AdditionalDescription,
                                      ModifiedDate = keyAreaSetting.ModifiedDate,
                                      KeyAreaUserId = keyAreaSetting.KeyAreaUserId,
                                  });

                    if (page != -1)
                    {
                        result = result.Skip((page - 1) * pageSize);
                        result = result.OrderByDescending(v => v.Id);
                    }

                    if (pageSize != -1)
                    {
                        result = result.Take(pageSize);
                        result = result.OrderByDescending(v => v.Id);
                    }

                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int> GetMyKeyAreaSettingCount(int userid)
        {
            using (var context = this._db)
            {
                int count = await (from keyAreaSetting in context.KeyAreaSetting
                                   join userMaster in context.UserMaster on keyAreaSetting.UserId equals userMaster.Id
                                   where (keyAreaSetting.IsDeleted == Record.NotDeleted && keyAreaSetting.UserId == userid)
                                   select keyAreaSetting.UserId).CountAsync();

                return count;
            }
        }

    }
}
