// ======================================
// <copyright file="SurveyResultDetailRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.Data;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;

namespace Survey.API.Repositories
{
    public class SurveyResultDetailRepository : Repository<SurveyResultDetail>, ISurveyResultDetailRepository
    {
        private GadgetDbContext db;
        public SurveyResultDetailRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
    }
}
