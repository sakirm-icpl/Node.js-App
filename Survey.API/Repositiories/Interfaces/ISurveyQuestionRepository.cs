// ======================================
// <copyright file="ISurveyQuestionRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.APIModel;
using Survey.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
{
    public interface ISurveyQuestionRepository : IRepository<SurveyQuestion>
    {
        Task<List<APISurveyMergeredModel>> GetAllSurveyQuestion(int UserId, string UserRole, int page, int pageSize, string search = null);
        Task<List<APISurveyMergeredModel>> GetNestedSurveyQuestion(int UserId, string UserRole, int page, int pageSize, string search = null);
        Task<int> Count(int UserId, string UserRole, string search = null);
        Task<int> NestedCount(int UserId, string UserRole, string search = null);
        Task<bool> Exist(int id, string search);
        Task<IEnumerable<SurveyQuestion>> Search(string query);
        Task<IEnumerable<SurveyQuestion>> GetAllSurveyQuestion(int surveyId);
        Task<APIResponse> GetSurveyBySurveyId(int surveyId, int userid);
        Task<APISurveyMergeredModel> GetSurvey(int questionId);
        Task<List<APISurveyMergeredModel>> GetActiveQuestions(int page, int pageSize, string search = null);
        Task<int> GetActiveQuestionsCount(string search = null);
        Task<APIResponse> AddSurvey(List<APISurveyMergeredModel> aPISurveyMergeredModel, int userId);
        Task<bool> existsQuestion(string question, int? questionId);
        Task<bool> QuestionExist(string question, string section);
        bool QuestionExists(string question);
        Task<APIResponse> MultiDeleteSurveyQuestion(APIDeleteSurveyQuestion[] apideletemultipleques);
        Task<APIResponse> GetMultipleBySurveyId(int SurveyId, int userid, string OrgCode);
    }
}
