// ======================================
// <copyright file="IPollsManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using PollManagement.API.APIModel;
using PollManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PollManagement.API.Repositories.Interfaces
{
    public interface IPollsManagementRepository : IRepository<PollsManagement>
    {
        Task<IEnumerable<PollsManagement>> GetAllPollsManagement(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        Task<bool> Exist(string search);
        Task<IEnumerable<PollsManagement>> Search(string query);
        Task<IEnumerable<APIPollsManagement>> GetAllPollsManagement(int usetId);
        Task<int> GetAllPollsManagementCount(int usetId);
        Task<bool> ExistPoll(int pollid, int userid);
        Task<JsonResult> GetTotal(int id, int userid);
        Task<int> GetCount();
        Task<bool> ExistsInResult(int pollId);
        Task<int> SendNotification(string pollQuestion, string token);
        Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId);
        Task<int> AddPollsApplicability(int pollId, string accessibilityParameter, string parameterValue, int parameterValueId);
        Task<int> UpdatePollsApplicability(int pollId, string accessibilityParameter, string parameterValue, int? parameterValueId);
        Task<APIPollsManagement> GetPollManagement(int pollId);
        Task<bool> Existquestion(string question, int? pollId);

        Task<IEnumerable<APIOpinionPollQuestion>> OpinionPollsReportTypeHead(string question);
    }
}
