// ======================================
// <copyright file="PollsResultRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using PollManagement.API.Data;
using PollManagement.API.Models;
using PollManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace PollManagement.API.Repositories
{
    public class PollsResultRepository : Repository<PollsResult>, IPollsResultRepository
    {
        private GadgetDbContext db;
        public PollsResultRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }

        public async Task<PollsResult> GetUserWisePoll(int pollId, int userId)
        {
            return await this.db.PollsResult.Where(p => p.PollsId == pollId && p.UserId == userId).FirstOrDefaultAsync();
        }
    }
}
