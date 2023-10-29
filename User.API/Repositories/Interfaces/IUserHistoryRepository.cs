//======================================
// <copyright file="IUserHistoryRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IUserHistoryRepository : IRepository<UserHistory>
    {
        Task<APIUserHistory> GetuserHistory(int userId);

    }
}
