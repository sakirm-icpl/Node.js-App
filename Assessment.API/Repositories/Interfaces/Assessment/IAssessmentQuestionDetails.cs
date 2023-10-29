using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;

namespace Assessment.API.Repositories.Interface
{
    public interface IAssessmentQuestionDetails : IRepository<AssessmentQuestionDetails>
    {
        Task<IEnumerable<APIAssessmentSubjectiveQuestion>> GetPostAssessmentQuestionDetailsById(int AssesmentResultId);
        Task<int> Count(int id);
    }
}
