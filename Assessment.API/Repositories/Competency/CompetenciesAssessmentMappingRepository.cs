using Assessment.API.Helper;
using Assessment.API.Model.Competency;
using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories.Interfaces.Competency;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace Assessment.API.Repositories.Competency
{
    public class CompetenciesAssessmentMappingRepository : Repository<AssessmentCompetenciesMapping>, ICompetenciesAssessmentMappingRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesAssessmentMappingRepository));
        private AssessmentContext db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;

        public CompetenciesAssessmentMappingRepository(AssessmentContext context, ICustomerConnectionStringRepository customerConnectionStringRepository) : base(context)
        {
            this.db = context;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
        }

        public async Task<bool> Exists(int AssessmentQuestionId, int comId, int? id = null)
        {
            var count = await this.db.AssessmentCompetenciesMapping.Where(p => ((p.AssessmentQuestionId == AssessmentQuestionId) && (p.CompetencyId == comId) && (p.IsDeleted == Record.NotDeleted) && (p.Id != id || id == null))).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

    }
}