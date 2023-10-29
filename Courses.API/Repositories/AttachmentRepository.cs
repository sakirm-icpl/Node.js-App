using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Courses.API.Helper;
using log4net;
namespace Courses.API.Repositories
{
    public class AttacmentRepository : Repository<Attachment>, IAttachmentRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AttacmentRepository));
        private CourseContext _db;
        public AttacmentRepository(CourseContext context) : base(context)
        {
            _db = context;
        }
        public async Task<bool> Exists(string name)
        {
            if (await _db.Attachment.CountAsync(y => y.OriginalFileName == name) > 0)
                return true;
            return false;
        }
        public async Task<List<Attachment>> Get(int page, int pageSize, string search = null, string filter = null)
        {
            IQueryable<Courses.API.Model.Attachment> Query = _db.Attachment;
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.OriginalFileName.Contains(search) || r.OriginalFileName.StartsWith(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return await Query.ToListAsync();
        }
        public async Task<int> count(string search = null, string filter = null)
        {
            if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(filter))
                return await _db.Attachment.Where(r => r.OriginalFileName.Contains(search)).CountAsync();
            return await _db.Attachment.CountAsync();
        }
        public async Task<int> AddAttachment(Attachment attach)
        {
            try
            {
                await this.Add(attach);
                return attach.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }

        }

    }
}
