// ======================================
// <copyright file="QuizOptionMasterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================


using QuizManagement.API.Data;
using QuizManagement.API.Helper;
using QuizManagement.API.Models;
using QuizManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace QuizManagement.API.Repositories
{
    public class QuizOptionMasterRepository : Repository<QuizOptionMaster>, IQuizOptionMasterRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QuizOptionMasterRepository));
        private GadgetDbContext db;
        public QuizOptionMasterRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
        public async Task<IEnumerable<QuizOptionMaster>> GetAllQuizOptionMaster(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.QuizOptionMaster> Query = this.db.QuizOptionMaster;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.AnswerText.StartsWith(search) || Convert.ToString(v.QuizQuestionId).StartsWith(search) && v.IsDeleted == Record.NotDeleted);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.QuizOptionMaster.Where(r => r.AnswerText.Contains(search) || Convert.ToString(r.QuizQuestionId).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.QuizOptionMaster.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<IEnumerable<QuizOptionMaster>> Search(string query)
        {
            Task<List<QuizOptionMaster>> QuizOptionMasterList = (from quizOptionMaster in this.db.QuizOptionMaster
                                                                 where
                                                            (quizOptionMaster.AnswerText.StartsWith(query) ||
                                                           Convert.ToString(quizOptionMaster.QuizQuestionId).StartsWith(query)
                                                           )
                                                            && quizOptionMaster.IsDeleted == false
                                                                 select quizOptionMaster).ToListAsync();
            return await QuizOptionMasterList;
        }
        public async Task<IEnumerable<QuizOptionMaster>> GetAllQuizOption(int questionId)
        {
            try
            {
                IQueryable<Models.QuizOptionMaster> Query = this.db.QuizOptionMaster;


                Query = Query.Where(v => v.IsDeleted == Record.NotDeleted && (v.QuizQuestionId == questionId));
                Query = Query.OrderByDescending(v => v.Id);
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

    }
}
