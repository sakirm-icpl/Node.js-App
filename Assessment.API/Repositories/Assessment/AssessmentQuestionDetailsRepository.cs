using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using log4net;
using Assessment.API.Helper;

namespace Assessment.API.Repositories
{
    public class AssessmentQuestionDetailsRepository : Repository<AssessmentQuestionDetails>, IAssessmentQuestionDetails
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentQuestionDetailsRepository));
        private AssessmentContext _db;
        public AssessmentQuestionDetailsRepository(AssessmentContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<APIAssessmentSubjectiveQuestion>> GetPostAssessmentQuestionDetailsById(int Id)
        {
            try
            {
                IQueryable<APIAssessmentSubjectiveQuestion> Query = from subjectiveAssessmentStatus in this._db.SubjectiveAssessmentStatus
                                                                    join postAssessmentResult in _db.PostAssessmentResult on subjectiveAssessmentStatus.AssessmentResultID equals postAssessmentResult.Id
                                                                    join assessmentQuestionDetails in _db.AssessmentQuestionDetails on subjectiveAssessmentStatus.Id equals assessmentQuestionDetails.AssessmentResultID
                                                                    join assessmentQuestion in _db.AssessmentQuestion on assessmentQuestionDetails.ReferenceQuestionID equals assessmentQuestion.Id

                                                                    where postAssessmentResult.IsDeleted == false && subjectiveAssessmentStatus.Id == Id
                                                                    orderby postAssessmentResult.Id descending
                                                                    select new APIAssessmentSubjectiveQuestion
                                                                    {
                                                                        ReferenceQuestionID = assessmentQuestionDetails.ReferenceQuestionID,
                                                                        SelectedAnswer = assessmentQuestionDetails.SelectedAnswer,
                                                                        Question = assessmentQuestion.QuestionText,
                                                                        MediaFile = assessmentQuestion.MediaFile,
                                                                        ModelAnswer = assessmentQuestion.ModelAnswer,
                                                                        AssignedMarks = assessmentQuestion.Marks,
                                                                        Id = assessmentQuestionDetails.Id
                                                                    };


                Query = Query.OrderByDescending(v => v.Id);

                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> Count(int id)
        {

            return await this._db.AssessmentQuestionDetails.Where(r => r.IsDeleted == Record.NotDeleted && r.AssessmentResultID == id).CountAsync();

        }
    }
}
