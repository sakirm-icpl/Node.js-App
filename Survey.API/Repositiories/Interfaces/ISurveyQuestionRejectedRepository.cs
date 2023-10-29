using Survey.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
{
    public interface ISurveyQuestionRejectedRepository : IRepository<SurveyQuestionRejected>
    {
        void Delete();
        Task<int> Count(string search = null);
        Task<IEnumerable<SurveyQuestionRejected>> GetAllSurveyQuestionReject(int page, int pageSize, string search = null);
    }
}
