//======================================
// <copyright file="IRolesRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public interface IRolesRepository : IRepository<Roles>
    {
        Task<IEnumerable<Roles>> Search(string q);
        Task<bool> Exist(string roleCode);
        Task<IEnumerable<Roles>> GetAllRoles(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<IEnumerable<APIRoleAuthorities>> GetRoleAuthorities(int id);
        Task<IEnumerable<String>> GetNotAccessModules(int roleId);
        Task<IEnumerable<String>> GetNotAccessFunctionGroups(int roleId);
        Task<int> GetRole(int id);
        Task<int> GetImplicitRole(int userId, string UserName);

        Task<bool> CheckRoleAssignedToUser(string roleCode);
    }
}
