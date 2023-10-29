using Assessment.API.APIModel;
using Assessment.API.Models;
using Courses.API.APIModel;
using Feedback.API.APIModel;
using Feedback.API.Repositories.Interfaces;

namespace Assessment.API.Repositories.Interface
{
    public interface IAssessmentQuestion : IRepository<AssessmentQuestion>
    {
        Task<APIFQCourse> courseCodeExists(string coursecode);
        Task<bool> ExistQuestionOption(APIAssessmentQuestion objAPIAssessmentQuestion);

        
    }


}
