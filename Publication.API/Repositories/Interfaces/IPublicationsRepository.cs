// ======================================
// <copyright file="IPublicationsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Publication.API.Repositories.Interfaces
{
    public interface IPublicationsRepository : IRepository<Publications>
    {
        Task<IEnumerable<Publications>> GetAllPublications(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        bool Exist(string search);
        Task<IEnumerable<Publications>> Search(string query);
        Task<int> GetTotalPublicationCount();
        Task<IEnumerable<Publications>> SearchTitle(string category);
        Task<IEnumerable<Publications>> GetAllPublications(int userId);
        Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId);
        Task<int> ReadToady(int pubID);
    }
}
