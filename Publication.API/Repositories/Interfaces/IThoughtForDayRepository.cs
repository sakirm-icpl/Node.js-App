// ======================================
// <copyright file="IThoughtForDayRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Publication.API.Repositories.Interfaces
{
    public interface IThoughtForDayRepository : IRepository<ThoughtForDay>
    {
        Task<IEnumerable<ThoughtForDay>> GetAllThoughtForDay(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<IEnumerable<ThoughtForDay>> Search(string query);
        Task<int> CountDate(string search = null);
        Task<ThoughtForDay> GetThoughtForDate(string search = null);
        Task<bool> Exist(int userId, int thoughtForDateId);
        Task<bool> Exist(string search);
        Task<bool> Exists(DateTime date);
       
    }
}
