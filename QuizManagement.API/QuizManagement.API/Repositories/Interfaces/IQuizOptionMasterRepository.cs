// ======================================
// <copyright file="IQuizOptionMasterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using QuizManagement.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizManagement.API.Repositories.Interfaces
{
    public interface IQuizOptionMasterRepository : IRepository<QuizOptionMaster>
    {
        Task<IEnumerable<QuizOptionMaster>> GetAllQuizOptionMaster(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<IEnumerable<QuizOptionMaster>> Search(string query);
        Task<IEnumerable<QuizOptionMaster>> GetAllQuizOption(int questionId);
    }
}
