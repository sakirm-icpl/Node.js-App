//======================================
// <copyright file="RolesRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using log4net;
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
    public class RolesRepository : Repository<Roles>, IRolesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RolesRepository));
        private UserDbContext _db;
        private IHttpContextAccessor _httpContext;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public RolesRepository(UserDbContext context, ICustomerConnectionStringRepository customerConnectionString, IHttpContextAccessor httpContext) : base(context)
        {
            this._db = context;
            this._httpContext = httpContext;
            this._customerConnectionString = customerConnectionString;
        }
        public async Task<IEnumerable<Roles>> Search(string q)
        {
            var result = (from role in this._db.Roles
                          where ((role.RoleCode.StartsWith(q) || role.RoleName.StartsWith(q)) && role.IsDeleted == 0)
                          select role).ToListAsync();
            return await result;
        }

        public async Task<bool> Exist(string roleCode)
        {

            var count = await this._db.Roles.Where(r => string.Equals(r.RoleCode, roleCode, StringComparison.CurrentCultureIgnoreCase)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<bool> CheckRoleAssignedToUser(string roleCode)
        {

            var count = await this._db.UserMaster.Where(u => u.UserRole == roleCode && u.IsDeleted == false).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<IEnumerable<Roles>> GetAllRoles(int page, int pageSize, string search = null)
        {
            try
            {
                var Roles = (from role in this._db.Roles
                             where (role.IsDeleted == Record.NotDeleted)
                             select role);
                if (!string.IsNullOrEmpty(search))
                    Roles = Roles.Where(u => u.RoleName.StartsWith(search) || u.RoleDescription.Contains(search));
                if (page != -1)
                    Roles = Roles.Skip((page - 1) * pageSize);
                if (pageSize != -1)
                    Roles = Roles.Take(pageSize);
                return await Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.Roles.Where(r => r.RoleName.StartsWith(search) || r.RoleDescription.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.Roles.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<int> GetRole(int id)
        {
            try
            {
                return await (from users in this._db.UserMaster
                              join roles in this._db.Roles on users.UserRole equals roles.RoleCode
                              where users.Id == id
                              select roles.Id).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return -1;
        }

        public async Task<int> GetImplicitRole(int userId, string userName)
        {
            int roleId = 0;
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetImplicitRoleForEndUser";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.VarChar) { Value = userName });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            roleId = Convert.ToInt32(dt.Rows[0]["RoleId"].ToString());

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
            return roleId;
        }
        public async Task<IEnumerable<APIRoleAuthorities>> GetRoleAuthorities(int id)
        {
            try
            {
                var result = (from roleAuthority in this._db.RoleAuthority
                              join permissionMaster in this._db.PermissionMaster on roleAuthority.PermissionId equals permissionMaster.Id
                              where (roleAuthority.RoleId == id)
                              select new APIRoleAuthorities
                              {
                                  Name = permissionMaster.Name,
                                  IsAccess = roleAuthority.IsAccess,
                                  Code = permissionMaster.Code
                              });

                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<String>> GetNotAccessModules(int roleId)
        {
            var RoleAuthority = (from roleAuthorities in _db.RoleAuthority
                                 join functionMaster in _db.FunctionsMaster
                                 on roleAuthorities.PermissionId equals functionMaster.Id
                                 where roleAuthorities.RoleId == roleId && roleAuthorities.IsAccess == true
                                 select functionMaster.ModuleCode);

            var Codes = this._db.AppModule.Select(appModule => appModule.ModuleCode).Except(RoleAuthority);
            return await Codes.ToListAsync();
        }

        public async Task<IEnumerable<String>> GetNotAccessFunctionGroups(int roleId)
        {
            var RoleAuthority = (from roleAuthorities in _db.RoleAuthority
                                 join functionMaster in _db.FunctionsMaster
                                 on roleAuthorities.PermissionId equals functionMaster.Id
                                 where roleAuthorities.RoleId == roleId && roleAuthorities.IsAccess == true
                                 select functionMaster.FunctionGroupCode);

            var Codes = this._db.FunctionsMaster.Select(appModule => appModule.FunctionGroupCode).Except(RoleAuthority);
            return await Codes.ToListAsync();
        }
    }
}
