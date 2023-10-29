using CourseApplicability.API.Model;
using Courses.API.APIModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface IAccessibilityRule : IRepository<AccessibilityRule>
    {
        List<CourseApplicableUser> GetUsersForUserTeam(int? Id);
        Task<List<AccessibilityRules>> SelfEnroll(APIAccessibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null);
    }
}