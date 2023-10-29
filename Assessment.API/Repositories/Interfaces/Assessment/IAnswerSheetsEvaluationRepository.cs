// ======================================
// <copyright file="IAnswerSheetsEvaluationRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assessment.API.Repositories.Interface
{
    public interface IAnswerSheetsEvaluationRepository : IRepository<AnswerSheetsEvaluation>
    {
        Task<IEnumerable<AnswerSheetsEvaluation>> GetAllAnswerSheetsEvaluation(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<IEnumerable<AnswerSheetsEvaluation>> Search(string query);
    }

}
