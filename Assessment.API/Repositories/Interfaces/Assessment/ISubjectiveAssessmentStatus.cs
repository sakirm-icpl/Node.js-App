using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assessment.API.Repositories.Interface
{
    public interface ISubjectiveAssessmentStatus : IRepository<SubjectiveAssessmentStatus>
    {
        Task<IEnumerable<SubjectiveAssessmentStatus>> GetAssessmentSheetByUserId();
    }
}
