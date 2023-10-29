//======================================
// <copyright file="IAreaRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IAreaRepository : IRepository<Area>
    {
        Task<IEnumerable<string>> GetAreaNames();
        Task<int> GetIdIfExist(string areaName);
        Task<int> GetLastInsertedId();
        Task<string> GetAreaNameById(int? locationId);
        Task<IEnumerable<Area>> GetAllAreas(string search);

        Task<List<Area>> GetAreas();
    }
}
