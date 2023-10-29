//======================================
// <copyright file="IConfigure6.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using Saml.API.Models;

namespace Saml.API.Repositories.Interfaces
{
    public interface IConfigure6Repository : IRepository<Configure6>
    {
        Task<IEnumerable<string>> GetConfigurationNames();
        Task<int> GetIdIfExist(string configurationName);
        Task<int> GetLastInsertedId();
        Task<string> GetConfigure6NameById(int? configureId);
        Task<IEnumerable<Configure6>> GetAllConfiguration6(string search);
        Task<List<Configure6>> GetConfiguration6();
    }
}
