﻿using Feedback.API.APIModel;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IMyCoursesRepository
    {
        Task<ApiCourseInfo> GetModuleInfo(int userId, int courseId, int? moduleId);
        Task<APIMyCoursesModule> GetModule(int userId, int courseId, string? organizationcode = null, int? groupId = null);


     
    }
}
