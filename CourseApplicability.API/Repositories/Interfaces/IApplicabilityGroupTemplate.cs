using CourseApplicability.API.APIModel;
using CourseApplicability.API.Model;
using Courses.API.APIModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface IApplicabilityGroupTemplate : IRepository<ApplicabilityGroupTemplate>
    {
        Task<int> Count(string search = null, string filter = null);
        Task<List<object>> GetAllGroupTemplate(int page, int pageSize, string search = null, string filter = null);
        Task<APIApplicabilityGroupTemplate> GetApplicabilityGroupTemplateDetails(int TemplateId, string orgnizationCode);
        Task<List<ApplicabilityGroupRules>> Post(APIApplicabilityGroupTemplate apiApplicabilityGroupTemplate, int userId);
        Task<APIApplicabilityGroupTemplate> GetApplicabilityGroupTemplate(int TemplateId);
        Task<APIScheduleVisibilityTemplate> GetVisibilityTeamTemplate(int TemplateId, int scheduleId);
        Task<List<object>> GetAllGroupTemplate();
        Task<bool> TemplateDependancy(int TemplateId);
    }
}

