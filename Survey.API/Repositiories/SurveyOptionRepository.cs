// ======================================
// <copyright file="SurveyOptionRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.Data;
using Survey.API.Helper;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace Survey.API.Repositories
{
    public class SurveyOptionRepository : Repository<SurveyOption>, ISurveyOptionRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SurveyOptionRepository));
        private GadgetDbContext db;
        public SurveyOptionRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
        public void Delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Gadget].[SurveyOption]");
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }

        }
        public async Task<IEnumerable<SurveyOption>> GetAllSurveyOption(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.SurveyOption> Query = this.db.SurveyOption;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.OptionText.StartsWith(search) || Convert.ToString(v.QuestionId).StartsWith(search) && v.IsDeleted == Record.NotDeleted);
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
                return await this.db.SurveyOption.Where(r => r.OptionText.Contains(search) || Convert.ToString(r.QuestionId).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.SurveyOption.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<bool> Exist(int id, string search)
        {
            int count = await this.db.SurveyOption.Where(p => (p.OptionText.ToLower() == search.ToLower()) && Convert.ToString(p.QuestionId).Contains(Convert.ToString(id)) && p.IsDeleted == Record.NotDeleted).CountAsync();
            return true;
            return false;
        }
        public async Task<IEnumerable<SurveyOption>> Search(string query)
        {
            Task<List<SurveyOption>> surveyOptionList = (from surveyOption in this.db.SurveyOption
                                                         where
                                                    (surveyOption.OptionText.StartsWith(query) ||
                                                   Convert.ToString(surveyOption.QuestionId).StartsWith(query)
                                                   )
                                                    && surveyOption.IsDeleted == false
                                                         select surveyOption).ToListAsync();
            return await surveyOptionList;
        }

        public async Task<IEnumerable<SurveyOption>> GetAllSurveyOption(int questionId)
        {
            try
            {
                IQueryable<Models.SurveyOption> Query = this.db.SurveyOption;

                Query = Query.Where(v => v.IsDeleted == Record.NotDeleted && (v.QuestionId == questionId));
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
