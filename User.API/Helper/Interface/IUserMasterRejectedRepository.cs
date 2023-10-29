//======================================
// <copyright file="IUserMasterRejectedRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IUserMasterRejectedRepository : IRepository<UserMasterRejected>
    {
        Task<List<APIUserMasterRejected>> GetAllRecord();
        void delete();
        Task<IEnumerable<UserMasterRejected>> GetAllUserReject(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<List<APIUserReject>> GetAllUsersRejected();
    }
}
