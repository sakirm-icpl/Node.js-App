// ======================================
// <copyright file="IPollsResultRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using PollManagement.API.Models;
using System.Threading.Tasks;

namespace PollManagement.API.Repositories.Interfaces
{
    public interface IPollsResultRepository : IRepository<PollsResult>
    {
        Task<PollsResult> GetUserWisePoll(int pollId, int userId);
    }
}
