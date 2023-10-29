//======================================
// <copyright file="IConfigure7.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IConfigure7Repository : IRepository<Configure7>
    {
        Task<IEnumerable<string>> GetConfigurationNames();
        Task<int> GetIdIfExist(string configurationName);
        Task<int> GetLastInsertedId();
        Task<string> GetConfigure7NameById(int? configureId);
        Task<IEnumerable<Configure7>> GetAllConfiguration7(string search);
        Task<List<Configure7>> GetConfiguration7();
    }
}
