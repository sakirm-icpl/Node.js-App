using Course.API.Model;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using Courses.API.Helper;
using Newtonsoft.Json;
using Courses.API.Common;

namespace Courses.API.Repositories
{

    public class CourseCompletionMailReminderRepository : Repository<CourseCompletionMailReminder>, ICourseCompletionMailReminder
    {
        private CourseContext _db;
        private IConfiguration _configuration;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseCompletionMailReminderRepository));
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
        public CourseCompletionMailReminderRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _db = context;
            _configuration = configuration;
            _customerConnectionRepository = customerConnectionRepository;

        }

        public async Task<CourseCompletionMailReminder> PostCompletionMailReminder(CourseCompletionMailReminder data, int UserId)
        {
            var Query = _db.CourseCompletionMailReminder.Where(a => (a.CourseId == data.CourseId));
            CourseCompletionMailReminder duplicate = Query.FirstOrDefault();
            if (duplicate == null)
            {
                data.ModifiedBy = UserId;
                data.CreatedBy = UserId;
                data.CreatedDate = DateTime.Now;
                data.ModifiedDate = DateTime.Now;
                data.IsActive = true;
                data.IsDeleted = false;

                await this._db.CourseCompletionMailReminder.AddAsync(data);
                await this._db.SaveChangesAsync();

                return data;
            }
            else
            {
                return null;
            }
        }

        public async Task<CourseCompletionMailReminderListandCount> GetCompletionMailReminder(int page, int pageSize, string search)
        {
            var Query = (from x in _db.CourseCompletionMailReminder
                         join course in _db.Course on  x.CourseId equals course.Id
                         join user in _db.UserMaster on x.CreatedBy equals user.Id
                         orderby x.CreatedDate descending
                         select new CourseCompletionMailReminderGet
                         {
                             Id = x.Id,
                             CreatedDate = x.CreatedDate,
                             CourseCode = course.Code,
                             CourseName = course.Title,
                             CourseId = x.CourseId,
                             FirstRemDays = x.FirstRemDays,
                             FirstRemTemplate = x.FirstRemTemplate,
                             SecondRemDays = x.SecondRemDays,
                             SecondRemTemplate = x.SecondRemTemplate,
                             ThirdRemDays = x.ThirdRemDays,
                             ThirdRemTemplate = x.ThirdRemTemplate,
                             FourthRemDays = x.FourthRemDays,
                             FourthRemTemplate = x.FourthRemTemplate,
                             FifthRemDays = x.FifthRemDays,
                             FifthRemTemplate = x.FifthRemTemplate,
                             UserName = user.UserName

                         }).AsNoTracking();


            if (!string.IsNullOrEmpty(search))
            {
               
                    Query = Query.Where(r => ((r.CourseCode.Contains(search))|| (r.CourseName.Contains(search)))); 
            }
            CourseCompletionMailReminderListandCount ListandCount = new CourseCompletionMailReminderListandCount();
            ListandCount.Count = Query.Distinct().Count();

            ListandCount.CompletionMailReminder = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();

            return ListandCount;
        }
    }
}
