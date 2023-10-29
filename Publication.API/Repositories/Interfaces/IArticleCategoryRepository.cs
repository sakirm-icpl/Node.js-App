// ======================================
// <copyright file="IArticleCategoryRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.APIModel;
using Publication.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Publication.API.Repositories.Interfaces
{
    public interface IArticleCategoryRepository : IRepository<InterestingArticleCategory>
    {
        Task<IEnumerable<InterestingArticleCategory>> Search(string category);
        Task<int> GetIdIfExist(string category);
        Task<int> GetLastInsertedId();
        Task<IEnumerable<APIInterestingArticleCategory>> GetArticles(int userid);

        Task<IEnumerable<InterestingArticleCategory>> GetCategoryCreatedBy(string category, int userid);
    }
}
