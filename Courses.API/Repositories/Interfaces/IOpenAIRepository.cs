using Courses.API.APIModel;
using Courses.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IOpenAIRepository : IRepository<OpenAIQuestion>
    {
        Task<List<APIOpenAIQuestion>> GetQuestions();
        Task<List<APIOpenAIQuestion>> GetQuestionsByCourseId(string CourseCode);
        Task<int> GetMappingId(string CourseCode,int QuestionId);

    }
    public interface IOpenAICourseQuestionAssociationRepository : IRepository<OpenAICourseQuestionAssociation>
    {

    }
}
