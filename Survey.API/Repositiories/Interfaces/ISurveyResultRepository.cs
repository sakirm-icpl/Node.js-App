// ======================================
// <copyright file="ISurveyResultRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================


using Survey.API.Models;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
{
    public interface ISurveyResultRepository : IRepository<SurveyResult>
    {
        Task<int> SubmittedSurveyCount(int surveyId);
    }
}
