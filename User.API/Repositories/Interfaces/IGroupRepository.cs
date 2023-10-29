//======================================
// <copyright file="IGroup.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<IEnumerable<string>> GetGroupNames();
        Task<int> GetIdIfExist(string groupName);
        Task<int> GetLastInsertedId();
        Task<string> GetGroupNameById(int? locationId);
        Task<IEnumerable<Group>> GetAllGroups(string search);
        Task<List<Group>> GetGroups();

    }
}
