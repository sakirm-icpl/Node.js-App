using Courses.API.APIModel;
using Courses.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task<bool> Exists(string name, int? moduleId = null);
        Task<List<APIModuleInput>> Get(int page, int pageSize, string search_string = null, string columnName = null);
        Task<List<Module>> GetForCourse(int page, int pageSize, string moduletype = null, string columnName = null, string searchstring = null);
        Task<List<Tuple<Module, APICourseDTO>>> GetModulewithCourseAsync(int page, int pageSize);
        Task<int> count(string search = null, string columnName = null);
        Task<int> coursesmodule_count(string moduletype = null, string columnName = null, string searchstring = null);
        Task<string> Upload(string filepath);
        Task<object> GetFeedbackConfigurationId(int courseId, int feedbackId, int? moduleId = null);
        Task<bool> IsDependacyExist(int moduleId);
        Task<List<TypeAhead>> GetModelTypeAhead(string search, string searchText);
         Task<List<TypeAhead>> GetModuleByCourses(int CourseId);
        Task<List<TypeAhead>> GetTopicByModules(int CourseId);
        Task<List<APIModule>> GetForAssessmentCourse(int page, int pageSize, string search = null);
        Task<List<ILTCourseTypeAhead>> GetModuleByCourse(int CourseId, int UserId, string OrganisationCode);

        Task<object> AddModuleLcmsData(ModuleLcmsAssociation moduleLcms);
        Task<ModuleLcmsAssociation> GetModuleLcmsData(int moduleid);
        Task<bool> UpdateModuleLcmsAssociation(int[] MultilingualLCMSId, int moduleId);
        Task<APITotalModuleData> GetV2(ApiCourseModule apiCourseModule, int userId, string userRole);
        Task<LCMS> getLcmsById(int  lcmsId,string metadata);
        Task<List<TypeAhead>> GetModulesForAssessmentCourses(int CourseId);
    }
}
