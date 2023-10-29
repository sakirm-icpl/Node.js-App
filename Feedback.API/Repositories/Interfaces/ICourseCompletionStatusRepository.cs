using Courses.API.APIModel;
using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Feedback.API.Repositories.Interfaces
{
    public interface ICourseCompletionStatusRepository : IRepository<CourseCompletionStatus>
    {
        Task<int> Post(CourseCompletionStatus courseCompletionStatus, string? OrgCode = null);

    }
}

