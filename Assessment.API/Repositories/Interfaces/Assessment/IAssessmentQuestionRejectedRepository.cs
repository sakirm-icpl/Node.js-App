using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assessment.API.Repositories.Interface
{
    public interface IAssessmentQuestionRejectedRepository : IRepository<AssessmentQuestionRejected>
    {
        void Delete();
        Task<IEnumerable<AssessmentQuestionRejected>> GetAllAssessmentsQuestionReject(int page, int pageSize, string? search = null);
        Task<int> Count(string? search = null);
    }
}
