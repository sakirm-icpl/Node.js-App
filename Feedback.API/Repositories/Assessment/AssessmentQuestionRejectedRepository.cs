using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using log4net;
using Feedback.API.Repositories;
using Feedback.API.Models;

namespace Assessment.API.Repositories
{
    public class AssessmentQuestionRejectedRepository : Repository<AssessmentQuestionRejected>, IAssessmentQuestionRejectedRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentQuestionRejectedRepository));
        private FeedbackContext _db;

        public AssessmentQuestionRejectedRepository(FeedbackContext context) : base(context)
        {
            this._db = context;
        }
    }
}