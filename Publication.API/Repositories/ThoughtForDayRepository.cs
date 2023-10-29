// ======================================
// <copyright file="ThoughtForDayRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Data;
using Publication.API.Helper;
using Publication.API.Models;
using Publication.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Publication.API.Repositories
{
    public class ThoughtForDayRepository : Repository<ThoughtForDay>, IThoughtForDayRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ThoughtForDayRepository));
        private GadgetDbContext db;
        public ThoughtForDayRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
        public async Task<IEnumerable<ThoughtForDay>> GetAllThoughtForDay(int page, int pageSize, string search = null)
        {
            try
            {

                using (GadgetDbContext context = this.db)
                {
                    IQueryable<ThoughtForDay> result = (from thoughtForDay in context.ThoughtForDay
                                                        where thoughtForDay.IsDeleted == Record.NotDeleted
                                                        select new ThoughtForDay
                                                        {
                                                            Id = thoughtForDay.Id,
                                                            Date = thoughtForDay.Date,
                                                            ForDate = thoughtForDay.ForDate,
                                                            Thought = thoughtForDay.Thought,
                                                            TotalLikesForDate = thoughtForDay.TotalLikesForDate,
                                                            TotalLikes = thoughtForDay.TotalLikes

                                                        });
                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where(a => a.Thought.Contains(search));
                        result = result.OrderByDescending(v => v.ForDate);
                    }
                    if (page != -1)
                        result = result.Skip((page - 1) * pageSize);
                    result = result.OrderByDescending(v => v.ForDate);
                    if (pageSize != -1)
                        result = result.Take(pageSize);
                    result = result.OrderByDescending(v => v.ForDate);

                    return await result.OrderByDescending(v => v.ForDate).ToListAsync();
                }
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
                return await this.db.ThoughtForDay.Where(r => r.Thought.StartsWith(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.ThoughtForDay.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<IEnumerable<ThoughtForDay>> Search(string query)
        {
            Task<List<ThoughtForDay>> thoughtForDayList = (from thoughtForDay in this.db.ThoughtForDay
                                                           where
                                                           (thoughtForDay.Thought.Contains(query) ||
                                                          Convert.ToString(thoughtForDay.TotalLikes).Contains(query)
                                                          )
                                                           && thoughtForDay.IsDeleted == false
                                                           select thoughtForDay).ToListAsync();
            return await thoughtForDayList;
        }
        public async Task<int> CountDate(string search = null)
        {
            string dateString = search;
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(dateString, format, null);
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.ThoughtForDay.Where(r => r.IsDeleted == Record.NotDeleted && (DateTime.Compare(r.ForDate.Date, parsedDate.Date) == 0)).CountAsync();
            return await this.db.ThoughtForDay.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<bool> Exist(string search)
        {
            string dateString = search;
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(dateString, format, null);
            int count = await this.db.ThoughtForDay.Where(r => r.IsDeleted == Record.NotDeleted && (DateTime.Compare(r.ForDate.Date, parsedDate.Date) == 0)).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
        public async Task<ThoughtForDay> GetThoughtForDate(string search = null)
        {
            string dateString = search;
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(dateString, format, null);
            try
            {
                IQueryable<ThoughtForDay> result = (from thoughtForDay in this.db.ThoughtForDay
                                                    where (thoughtForDay.IsDeleted == Record.NotDeleted && ((DateTime.Compare(thoughtForDay.ForDate.Date, parsedDate.Date) == 0)))
                                                    orderby thoughtForDay.ModifiedDate descending
                                                    select new ThoughtForDay
                                                    {
                                                        Date = thoughtForDay.Date,
                                                        Id = thoughtForDay.Id,
                                                        Thought = thoughtForDay.Thought,
                                                        ForDate = thoughtForDay.ForDate,
                                                        TotalLikesForDate = thoughtForDay.TotalLikesForDate,
                                                        TotalLikes = thoughtForDay.TotalLikes,

                                                    });
                return await result.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<bool> Exist(int userId, int thoughID)
        {
            int count = await this.db.ThoughtForDayCounter.Where(p => ((p.UserId == userId) && (p.ThoughtForDayId == thoughID) && (p.IsDeleted == Record.NotDeleted))).CountAsync(); 
            if (count > 0)
                return true;
            return false;
        }

        public async Task<bool> Exists(DateTime date)
        {
            int count = await this.db.ThoughtForDay.Where(p => (p.ForDate==date) && (p.IsDeleted == Record.NotDeleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

       
    }
}
