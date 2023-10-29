// ======================================
// <copyright file="SurveyResultRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.Data;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.Repositories
{
    public class SurveyResultRepository : Repository<SurveyResult>, ISurveyResultRepository
    {
        private GadgetDbContext db;
        public SurveyResultRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
        public async Task<int> SubmittedSurveyCount(int surveyId)
        {
            return await this.db.SurveyResult.Where(s => s.SurveyId == surveyId).CountAsync();
        }
    }
}
