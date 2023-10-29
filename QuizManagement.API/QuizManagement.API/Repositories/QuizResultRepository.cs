// ======================================
// <copyright file="QuizResultRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using QuizManagement.API.Data;
using QuizManagement.API.Models;
using QuizManagement.API.Repositories.Interfaces;

namespace QuizManagement.API.Repositories
{
    public class QuizResultRepository : Repository<QuizResult>, IQuizResultRepository
    {
        private GadgetDbContext db;
        public QuizResultRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
    }
}
