// ======================================
// <copyright file="AnnouncementsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Gadget.API.Data;
using Gadget.API.Helper;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace Gadget.API.Repositories
{
    public class AnnouncementsRepository : Repository<Announcements>, IAnnouncementsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AnnouncementsRepository));
        private GadgetDbContext db;
        public AnnouncementsRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }

        public async Task<IEnumerable<Announcements>> GetAllAnnouncements(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.Announcements> Query = this.db.Announcements;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.Announcement.Contains(search) && v.IsDeleted == Record.NotDeleted);
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


        public async Task<IEnumerable<Announcements>> GetAllAnnouncementsForEndUser()
        {
            try
            {

                DateTime datecurrent = DateTime.UtcNow;
                string date = datecurrent.ToString("dd-MM-yyyy");
                string format = "dd-MM-yyyy";
                DateTime parsedDate = DateTime.ParseExact(date, format, null);
                Task<List<Announcements>> announcementsList = (from announcements in this.db.Announcements
                                                               where (announcements.IsDeleted == Record.NotDeleted) &&
                                                               (((DateTime.Compare(announcements.FromDate.Date, parsedDate.Date) <= 0) && (DateTime.Compare(announcements.ToDate.Date, parsedDate.Date) >= 0))
                                                              )
                                                               orderby announcements.ModifiedDate descending

                                                               select announcements).ToListAsync();
                return await announcementsList;
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
                return await this.db.Announcements.Where(r => r.Announcement.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.Announcements.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<IEnumerable<Announcements>> Search(string query)
        {
            Task<List<Announcements>> announcementsList = (from announcements in this.db.Announcements
                                                           where (announcements.IsDeleted == Record.NotDeleted) &&
                                                           (announcements.Announcement.Contains(query) ||
                                                          Convert.ToString(announcements.TotalReadCount).Contains(query)
                                                          )

                                                           select announcements).ToListAsync();
            return await announcementsList;
        }

        public async Task<int> GetCount()
        {

            DateTime datecurrent = DateTime.UtcNow;
            string date = datecurrent.ToString("dd-MM-yyyy");
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(date, format, null);
            return await this.db.Announcements.Where(r => r.IsDeleted == Record.NotDeleted && ((DateTime.Compare(r.FromDate.Date, parsedDate.Date) <= 0) && (DateTime.Compare(r.ToDate.Date, parsedDate.Date) >= 0))).CountAsync();


        }


    }
    public class MyAnnouncementRepository : Repository<MyAnnouncement>, IMyAnnouncementRepository
    {
        private GadgetDbContext db;
        public MyAnnouncementRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
    }

}
