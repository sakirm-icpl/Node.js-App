// ======================================
// <copyright file="IAnnouncementsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Gadget.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IAnnouncementsRepository : IRepository<Announcements>
    {
        Task<IEnumerable<Announcements>> GetAllAnnouncements(int page, int pageSize, string search = null);
        Task<IEnumerable<Announcements>> GetAllAnnouncementsForEndUser();
        Task<int> Count(string search = null);
        Task<IEnumerable<Announcements>> Search(string query);
        Task<int> GetCount();
    }
    public interface IMyAnnouncementRepository : IRepository<MyAnnouncement>
    {
    }
}
