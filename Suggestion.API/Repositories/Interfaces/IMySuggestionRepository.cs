// ======================================
// <copyright file="IMySuggestion.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Suggestion.API.APIModel;
using Suggestion.API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface IMySuggestionRepository : IRepository<MySuggestion>
    {
        Task<IEnumerable<APIMySuggestion>> GetAllSuggestions(int userId, int page, int pageSize, string search = null);
        Task<int> Count(int userId, string search = null);
        Task<IEnumerable<MySuggestion>> Search(string query);
        Task<bool> Exists(int UserId, DateTime Create);
        Task<List<APIUpdateSuggestions>> GetAllSuggestionsData(APIUpdateSuggestions aPIMySuggestion);
        Task<MySuggestion> GetSuggestionDetails(int id);
        Task<IEnumerable<APIMySuggestion>> GetAllMySuggestions(int page, int pageSize, string search = null);
        Task<string> SaveFile(IFormFile uploadedFile, string fileType, string OrganizationCode);
        Task<MySuggestionDetail> GetSuggestion(int id);

    }
}
