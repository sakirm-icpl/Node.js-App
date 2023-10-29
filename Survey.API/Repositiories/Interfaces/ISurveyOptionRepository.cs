// ======================================
// <copyright file="ISurveyOptionRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Survey.API.Repositories.Interfaces
{
    public interface ISurveyOptionRepository : IRepository<SurveyOption>
    {
        Task<IEnumerable<SurveyOption>> GetAllSurveyOption(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<bool> Exist(int id, string search);
        Task<IEnumerable<SurveyOption>> Search(string query);
        Task<IEnumerable<SurveyOption>> GetAllSurveyOption(int questionId);
        void Delete();
    }
}
