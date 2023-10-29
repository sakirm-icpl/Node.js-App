using Assessment.API.Models;
using Assessment.API.Repositories.Interface;

namespace Assessment.API.Repositories
{
    public class AsessmentQuestionOptionBankRepository : Repository<AssessmentQuestionOption>, IAsessmentQuestionOption
    {
        private AssessmentContext _db;

        public AsessmentQuestionOptionBankRepository(AssessmentContext context) : base(context)
        {
            this._db = context;
        }
    }
}
