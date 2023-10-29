using Courses.API.APIModel;
using Courses.API.Model;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ILcmsQuestionAssociation : IRepository<LcmsQuestionAssociation>
    {
        Task<ApiAssesment> GetAssesment(int id);
        Task<int> DeleteQuestions(int lcmsId);
        Task<int> Delete(int lcmsId);
    }
}
