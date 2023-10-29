using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assessment.API.Repositories.Interface
{
    public interface IAssessmentConfiguration : IRepository<AssessmentConfiguration>
    {
        Task<IEnumerable<AssessmentConfiguration>> GetAllAssessmentConfiguration(int page, int pageSize, string search = null);

    }
}
