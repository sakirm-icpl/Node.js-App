//======================================
// <copyright file="IConfigure1.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IConfigure1Repository : IRepository<Configure1>
    {
        Task<IEnumerable<string>> GetConfigurationNames();
        Task<int> GetIdIfExist(string configurationName);
        Task<int> GetLastInsertedId();
        Task<string> GetConfigure1NameById(int? configureId);
        Task<IEnumerable<Configure1>> GetAllConfiguration1(string search);
        Task<IEnumerable<TypeHeadDto>> GetConfiguration(string columnName, string search = null, int? page = null, int? pageSize = null,string? orgCode=null);
        Task<int> GetConfigurationCount(string columnName, string search = null);
        Task<List<Configure1>> GetConfiguration1();
        Task<IEnumerable<TypeHeadDto>> GetConfigurationColumnDatails(string columnName, int Id);
        Task<string> UpdateConfigurationColumnDetails(string columnName, TypeHeadDto configurationColumn);
        Task<string> DeleteConfigurationColumnDetails(string columnName, int id);
        Task<IEnumerable<TypeHeadDto>> GetConfigurationGroup(string columnName, string search = null, int? page = null, int? pageSize = null);

        Task<string> PostConfigurationColumnDetails(ApiConfiguration1 configuration1Details);
        Task<string> PutConfigurationColumnDetails(ApiConfiguration1 configuration1Details);
        Task<string> DeleteConfiguration1Details(int id);
        Task<IEnumerable<TypeHeadDto>> GetConfiguration(int? page = null, int? pageSize = null, string search = null);
        Task<int> GetConfiguration1Count(string search = null);
    }

}
