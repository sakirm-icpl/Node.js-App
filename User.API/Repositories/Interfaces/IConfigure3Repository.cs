//======================================
// <copyright file="IConfigure3.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IConfigure3Repository : IRepository<Configure3>
    {
        Task<IEnumerable<string>> GetConfigurationNames();
        Task<int> GetIdIfExist(string configurationName);
        Task<int> GetLastInsertedId();
        Task<string> GetConfigure3NameById(int? configureId);
        Task<IEnumerable<Configure3>> GetAllConfiguration3(string search);
        Task<List<Configure3>> GetConfiguration3();
    }
}
