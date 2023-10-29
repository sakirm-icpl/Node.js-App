// ======================================
// <copyright file="AnswerSheetsEvaluationRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Assessment.API.Helper;
using Assessment.API.Models;
using Assessment.API.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Assessment.API.Helper;

namespace Assessment.API.Repositories
{
    public class AnswerSheetsEvaluationRepository : Repository<AnswerSheetsEvaluation>, IAnswerSheetsEvaluationRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AnswerSheetsEvaluationRepository));
        private AssessmentContext _db;

        public AnswerSheetsEvaluationRepository(AssessmentContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<AnswerSheetsEvaluation>> GetAllAnswerSheetsEvaluation(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.AnswerSheetsEvaluation> Query = this._db.AnswerSheetsEvaluation;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => Convert.ToString(v.AnswerSheetId).StartsWith(search) || Convert.ToString(v.QuestionId).StartsWith(search) && v.IsDeleted == Record.NotDeleted);
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
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.AnswerSheetsEvaluation.Where(r => Convert.ToString(r.QuestionId).Contains(search) || Convert.ToString(r.AnswerSheetId).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.AnswerSheetsEvaluation.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<IEnumerable<AnswerSheetsEvaluation>> Search(string query)
        {
            var mediaLibraryList = (from answerSheetsEvaluation in this._db.AnswerSheetsEvaluation
                                    where
                                    (Convert.ToString(answerSheetsEvaluation.QuestionId).StartsWith(query) ||
                                   Convert.ToString(answerSheetsEvaluation.AnswerSheetId).StartsWith(query)
                                   )
                                    && answerSheetsEvaluation.IsDeleted == false
                                    select answerSheetsEvaluation).ToListAsync();
            return await mediaLibraryList;
        }
    }
}
