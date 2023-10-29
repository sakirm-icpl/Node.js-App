//====================================
// <copyright file="IConfigure8.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IConfigure8Repository : IRepository<Configure8>
    {
        Task<IEnumerable<string>> GetConfigurationNames();
        Task<int> GetIdIfExist(string configurationName);
        Task<int> GetLastInsertedId();
        Task<string> GetConfigure8NameById(int? configureId);
        Task<IEnumerable<Configure8>> GetAllConfiguration8(string search);
        Task<List<Configure8>> GetConfiguration8();
    }
}
