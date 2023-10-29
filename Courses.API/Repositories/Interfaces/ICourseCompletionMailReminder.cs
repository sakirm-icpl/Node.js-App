using Course.API.Model;
using Courses.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ICourseCompletionMailReminder : IRepository<CourseCompletionMailReminder>
    {
        Task<CourseCompletionMailReminder> PostCompletionMailReminder(CourseCompletionMailReminder data, int UserId);
        Task<CourseCompletionMailReminderListandCount> GetCompletionMailReminder(int page, int pageSize, string search);
    }
}
