using CourseApplicability.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<CourseApplicability.API.Model.Course>
    {
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
    }
}