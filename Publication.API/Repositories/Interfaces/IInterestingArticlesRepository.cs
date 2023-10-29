// ======================================
// <copyright file="IInterestingArticlesRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.APIModel;
using Publication.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Publication.API.Repositories.Interfaces
{
    public interface IInterestingArticlesRepository : IRepository<InterestingArticles>
    {
        Task<IEnumerable<InterestingArticles>> GetAllInterestingArticles(int UserId, string UserRole, int page, int pageSize, string search = null);
        Task<int> Count(int UserId, string UserRole, string search = null);
        Task<bool> Exist(string search);
        Task<InterestingArticles> GetInterestingArticlesObject(InterestingArticles interestingArticles, APIInterestingArticles aPIInterestingArticles, int UserId);
        Task<IEnumerable<InterestingArticles>> Search(string query);
        Task<IEnumerable<InterestingArticles>> GetAllInterestingArticlesByCategoryId(int id, int userId);
        Task<IEnumerable<InterestingArticles>> GetAllInterestingArticlesBySearch(int id, string search);
        Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId);
    }
}
