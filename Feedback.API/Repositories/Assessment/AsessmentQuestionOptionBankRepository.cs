using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Feedback.API.Models;
using Courses.API.Repositories;
using Feedback.API.Repositories;

namespace Assessment.API.Repositories
{
    public class AsessmentQuestionOptionBankRepository : Repository<AssessmentQuestionOption>, IAsessmentQuestionOption
    {
        private FeedbackContext _db;

        public AsessmentQuestionOptionBankRepository(FeedbackContext context) : base(context)
        {
            this._db = context;
        }
    }
}
