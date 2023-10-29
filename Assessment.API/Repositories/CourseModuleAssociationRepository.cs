using Assessment.API.Model;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories;
using Assessment.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Assessment.API.Repositories
{
    public class CourseModuleAssociationRepository : Repository<CourseModuleAssociation>, ICourseModuleAssociationRepository
    {
        ICustomerConnectionStringRepository _customerConnection;
        private AssessmentContext _db;
        public CourseModuleAssociationRepository(AssessmentContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
        }

        public async Task<bool> IsFeedbackExist(int CourseID, int ModuleId)
        {
            int? FeedbackId = await _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId).Select(c => c.FeedbackId).FirstOrDefaultAsync();
            if (FeedbackId == null || FeedbackId == 0)
                return false;
            return true;
        }
        public async Task<bool> IsAssementExist(int CourseID, int ModuleId)
        {
            int? AssessmentId = await _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId).Select(c => c.AssessmentId).SingleOrDefaultAsync();
            if (AssessmentId == null || AssessmentId == 0)
                return false;
            return true;
        }


    }
}