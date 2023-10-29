// ======================================
// <copyright file="IThoughtForDayCounterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Publication.API.Repositories.Interfaces
{
    public interface IThoughtForDayCounterRepository : IRepository<ThoughtForDayCounter>
    {
        Task<IEnumerable<ThoughtForDayCounter>> GetAllThoughtForDayCounter(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId);
    }
}
