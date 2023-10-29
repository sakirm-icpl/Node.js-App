//======================================
// <copyright file="ILocationRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<IEnumerable<string>> GetLocationNames();
        Task<int> GetIdIfExist(string locationName);
        Task<int> GetLastInsertedId();
        Task<string> GetLocationNameById(int? locationId);
        Task<IEnumerable<Location>> GetAllLocations(string search);
        Task<List<Location>> GetLocations();
    }
}
