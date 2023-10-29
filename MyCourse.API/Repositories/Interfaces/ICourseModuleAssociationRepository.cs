﻿using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface ICourseModuleAssociationRepository : IRepository<CourseModuleAssociation>
    {
        //IQueryable<CourseModuleAssociation> GetAssociationCourseModule(int CourseID, int ModuleId);
        //Task<bool> Exist(int CourseID, int ModuleId);
        //Task<int?> GetAssessmentId(int CourseID, int ModuleId);
        //Task<int?> GetFeedbackId(int CourseID, int ModuleId);
        Task<bool> IsAssementExist(int CourseID, int ModuleId);
        Task<bool> IsFeedbackExist(int CourseID, int ModuleId);
        //Task<List<TypeAhead>> GetModelTypeAhead(int courseId, string search = null);
        //Task<int> AdjustSequence(List<ApiCourseModuleSequence> apiCourseModuleList, int courseId);
        //Task CourseModuleAuditlog( List<CourseModuleAssociation> modules,string action);
        //Task CourseModuleAssociationAuditlog(IList<CourseModuleAssociation> modules, string action);
    }
}
