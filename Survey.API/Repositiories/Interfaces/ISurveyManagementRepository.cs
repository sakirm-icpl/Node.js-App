// ======================================
// <copyright file="ISurveyManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.APIModel;
using Survey.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
{
    public interface ISurveyManagementRepository : IRepository<SurveyManagement>
    {
        Task<APIResponse> GetSurveyQuestionTypeAhead(int surveyid);
        Task<IEnumerable<SurveyManagement>> GetAllSurveyManagement(int UserId, string UserRole, int page, int pageSize, string search = null);
        Task<int> Count(int UserId, string UserRole, string search = null);
        Task<bool> Exist(string search);
        Task<IEnumerable<SurveyManagement>> Search(string query);
        Task<IEnumerable<SurveyManagement>> GetAllSurveyManagement(int userId);
        Task<int> GetAllSurveyManagementCount(int userId);
        Task<bool> ExistsSurvey(int surveyId, int userid);
        Task<List<SurveyConfiguration>> GetDetailsFromSuveryID(int LcmsId);
        Task<IEnumerable<SurveyManagement>> SearchSurvey(string Survey);
        Task<IEnumerable<SurveyManagement>> SearchSurveyTODO(string Survey);
        Task<bool> ExistsInResult(int surveyId);
        Task<int> SendNotification(string surveySubject, string token, int surveyID);
        Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId);
        Task<int> ReadToady(int pubID);
        Task<int> FirstResponse(int surveyID);
        Task<int> AddSurvey(ApiSurveyLcms apiSurveyLcms, int userId);
        Task<GetApiSurveyLcms> GetLcmsSurvey(int lcmsId);
        Task<int> UpdateSurvey(ApiSurveyLcms apiSurveyLcms, int userId);
        Task<APISurveyManagement> GetSurveyMangementLcms(int surveyId);
        Task<int> AddSurveyApplicability(int surveyId, string accessibilityParameter, string parameterValue, int parameterValueId);
        Task<int> UpdateSurveyApplicability(int surveyId, string accessibilityParameter, string parameterValue, int? parameterValueId);
        Task<bool> existSurvey(string surveySubject, int? surveyID);
        Task<IEnumerable<SurveyManagement>> SurveyReportTypeHead(string Survey);
        Task<IEnumerable<SurveyManagement>> SurveyNotApplicableTypeAhead(string Survey);

        Task<bool> IsQuestionUsed(int questionID);
        Task<string> ProcessImportFile(FileInfo file, ISurveyManagementRepository surveyManagementRepository, ISurveyQuestionRepository surveyQuestionRepository, ISurveyOptionRepository surveyOptionRepository, ISurveyQuestionRejectedRepository _surveyQuestionRejectedRepository, int UserId);
        Task<List<SurveyQuestionRejected>> GetAllSurvey();
        Task<SurveyManagement> CheckForDuplicate(string SurveySubject);

        //Task<string> ProcessRecordsAsync(FileInfo file, ISurveyManagementRepository surveyManagementRepository, ISurveyQuestionRepository surveyQuestionRepository, ISurveyOptionRepository surveyOptionRepository, int UserId);
        Task<List<AccessibilitySurveyRules>> Post(APISurveyAccessibility apiAccessibility, int userId, string orgnizationCode = null,string token=null);
        Task<int> SendNotificationForSurvey(APINotifications apiNotification, bool IsApplicabletoall);
        Task<int> SendNotificationCustomizeSurvey(APINotifications apiNotification, bool IsApplicabletoall, string GuidNo, int notificationID, int UserId);
        Task<SurveyManagementAccessibilityRule> AddAccessibilityRule(SurveyManagementAccessibilityRule courseWiseEmail);
        Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int SurveyManagementId);

        Task<List<APISurveyAccessibilityRules>> GetAccessibilityRules(int courseId, string orgnizationCode, string token, int Page, int PageSize);

        Task<int> GetAccessibilityRulesCount(int SurveyManagementId);
        Task<List<SurveyApplicableUser>> GetSurveyApplicableUserList(int courseId);
        //FileInfo GetSurveyApplicableExcel(List<APISurveyAccessibilityRules> surveyApplicableUsers, List<SurveyApplicableUser> applicableUsers, string OrgCode);
        Task<List<object>> GetSurveyAccessibility(int page, int pageSize, string search = null, string filter = null);
        Task<int> count(string search = null, string filter = null);
        Task<int> DeleteRule(int roleId);
        Task<int> SendSurveyApplicabilityPushNotification(int CourseId, string orgCode);
        Task<List<APISurveyAccessibilityRules>> GetAccessibilityRulesForExport(int courseId, string orgnizationCode, string token);
        FileInfo GetSurveyApplicableExcel(List<APISurveyAccessibilityRules> surveyApplicableUsers, List<SurveyApplicableUser> applicableUsers, string surveysubject, string OrgCode);
        Task<string> GetSurveySubject(int surveymanagementId);

        Task<List<APINestedSurveyQuestions>> GetSurveyQuestionsByLcmsId(int lcmsId);
        Task<APINestedSurveyQuestions> GetQuestionByQuestionId(int questionId);
        Task<List<APINestedSurveyOptions>> GetOptionsByLcmsQuestion(int lcmsId, int questionId);
        Task<List<APINestedSurveyOptions>> GetOptionByLcmsOption(int lcmsId, int optionId);
        Task<List<APINestedSurveyQuestions>> GetRootQuestion(int lcmsId);
        Task SubmitNestedSurvey(APINestedSurveyResult apiNestedSurveyResult, int userId);
        Task<bool?> IsSurveyNested(int lcmsId);
        Task<bool> IsSurveyActive(int SurveyId);
    }

    public interface ISurveyConfigurationRepository : IRepository<SurveyConfiguration>
    {

    }
    public interface ISurveyOptionNestedRepository : IRepository<SurveyOptionNested>
    {

    }
}
