// ======================================
// <copyright file="IQuizQuestionMasterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using QuizManagement.API.APIModel;
using QuizManagement.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizManagement.API.Repositories.Interfaces
{
    public interface IQuizQuestionMasterRepository : IRepository<QuizQuestionMaster>
    {
        Task<IEnumerable<APIQuizQuestionMergered>> GetAllQuizQuestionMaster(int page, int pageSize, string search = null);
        Task<IEnumerable<APIQuizQuestionMergered>> GetAllQuizQuestionMaster();
        Task<int> Count(string search = null);
        Task<bool> Exist(int quizzId, string search);
        Task<IEnumerable<QuizQuestionMaster>> Search(string query);
        Task<IEnumerable<QuizQuestionMaster>> GetAllQuizQuestion(int quizId);
        Task<APIResponse> GetQuizByQuizId(int QuizID, int userid);
        Task<APIQuizQuestionMergered> GetQuiz(int questionId);
    }
}
