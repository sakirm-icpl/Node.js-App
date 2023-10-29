// ======================================
// <copyright file="IQuizzesManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.APIModel;
using Survey.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
{
    public interface IQuizzesManagementRepository : IRepository<QuizzesManagement>
    {
        Task<APIResponse> GetQuizQuestion(int quizid);
        Task<List<TypeAhead>> GetTypeAHead(string search = null);
        Task<IEnumerable<QuizzesManagement>> GetAllQuizzesManagement(int page, int pageSize, string search = null);
        Task<IEnumerable<QuizzesManagement>> GetAllQuizzesManagementForEndUser(int userid);
        Task<int> GetAllQuizzesManagementForEndUserCount(int userid);
        Task<int> Count(string search = null);
        Task<bool> Exist(string search);
        Task<IEnumerable<QuizzesManagement>> Search(string query);
        Task<bool> ExistsQuiz(int quizId, int userid);
        Task<bool> ExistsQuizInResult(int? quizId);
        Task<IEnumerable<QuizzesManagement>> SearchQuizz(string album);
        Task<int> SendNotification(string quizTitle, string token,int QuizID);
        Task<bool> existQuiz(string quizTitle, int? quizId);
        Task<APIQuizzesManagement> GetQuiz(int quizId);
        Task<int> UpdateQuizApplicability(int quizId, string accessibilityParameter, string parameterValue, int? parameterValueId, string rowGuid);
        Task<int> AddQuizApplicabilityParameter(int quizId, string accessibilityParameter, string parameterValue, int parameterValueId,string GuidNo );
        Task<bool> ExistsQuizQuestionInResult(int? questionId);
        Task<List<TypeAhead>> GetTypeAHeadQuizReport(string search = null);
        Task<List<APIQuizQuestionOptionDetails>> GetQuizQuestionOptionDetails(int QuizID, int UserId);
        Task<bool> isQuestionExist(int? quizId, int? questionId, string question);
        Task<bool> isQuestionDeleted(int? questionId);
        Task<bool> isSubmittedQuiz(int? quizId, int? userID);
        Task<List<QuizQuestionMaster>> GetQuestionByQuizId(int QuizId);
        Task<int> SendNotificationForQuizAndSurvey(APINotifications apiNotification, bool IsApplicabletoall);
        Task<int> SendNotificationForQuizSurvey(APINotifications apiNotification, bool IsApplicabletoall,string GuidNo,int notificationID,int UserId);
        Task<List<APIApplicableNotifications>> SendDataForApplicableNotifications(int NotificationId, int UserId);
        Task<int> GetNotificationId(string title);
        Task<int> SendQuizApplicabilityPushNotification(int QuizId, string orgCode);
        Task<int> SendNotificationCustomizeSurvey(APINotifications apiNotification, bool IsApplicabletoall, string GuidNo, int notificationID, int UserId);
        Task<int> SendPollApplicabilityPushNotification(int PollId, string orgCode);
    }
}
