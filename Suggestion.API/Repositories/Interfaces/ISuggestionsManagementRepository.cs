// ======================================
// <copyright file="ISuggestionsManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Suggestion.API.APIModel;
using Suggestion.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface ISuggestionsManagementRepository : IRepository<SuggestionsManagement>
    {
        Task<List<APISuggestionsManagement>> GetAllSuggestionsManagement(int page, int pageSize, string CreatedBy, string search = null);
        Task<List<APISuggestionsManagement>> GetUserNameById(int createdBy, int PageSize,string search=null);
        Task<int> Count(string search = null);
        Task<IEnumerable<SuggestionsManagement>> Search(string query);
        Task<List<APISuggestionsManagement>> GetAllSuggestions(int Id);

        Task<int> GetAllSuggestionsCount(int Id,string search = null, string searchText = null);
        Task<SuggestionsManagement> GetSuggestionDetail(int id);


    }
}
