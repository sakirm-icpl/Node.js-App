using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Data;
using log4net;
using Feedback.API.APIModel;
using Feedback.API.Helper;
using Assessment.API.APIModel;
using Feedback.API.Repositories;
using Feedback.API.Models;

namespace Assessment.API.Repositories
{
    public class AssessmentQuestionRepository : Repository<AssessmentQuestion>, IAssessmentQuestion
    {
        private string url;
        private FeedbackContext _db;
        private IAsessmentQuestionOption _asessmentQuestionOption;
       

        private IAssessmentQuestionRejectedRepository _assessmentQuestionBankRejectedRepository;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentQuestionRepository));

        public AssessmentQuestionRepository(IWebHostEnvironment environment,  FeedbackContext context,  IAsessmentQuestionOption asessmentQuestionOption, IAssessmentQuestionRejectedRepository assessmentQuestionRejectedRepository) : base(context)
        {
            this._db = context;
            this._asessmentQuestionOption = asessmentQuestionOption;
            this._assessmentQuestionBankRejectedRepository = assessmentQuestionRejectedRepository;
           
        }

        public async Task<APIFQCourse> courseCodeExists(string coursecode)
        {

            var qcourse = (from course in _db.Course
                           where course.Code == coursecode && course.IsDeleted == Record.NotDeleted
                           select new APIFQCourse
                           {
                               CourseId = course.Id,
                               Tilte = course.Title
                           }).AsNoTracking();
            APIFQCourse cdata = await qcourse.FirstOrDefaultAsync();
            return cdata;
        }

        public async Task<bool> ExistQuestionOption(APIAssessmentQuestion objAPIAssessmentQuestion)

        {
            // check question and metadata exists in db
            var count = this._db.AssessmentQuestion.Where(s => string.Equals(s.QuestionText.ToLower(), objAPIAssessmentQuestion.QuestionText.ToLower()) && s.IsDeleted == false && string.Equals(Convert.ToString(s.Metadata).ToLower(), objAPIAssessmentQuestion.Metadata.ToLower())).Count();
            if (count > 0)
            {
                List<AssessmentQuestion> APIAssessmentQuestionOldlist = this._db.AssessmentQuestion.Where(s => s.QuestionText == objAPIAssessmentQuestion.QuestionText && s.IsDeleted == false).OrderByDescending(y => y.Id).ToList();

                var selectedIds = APIAssessmentQuestionOldlist.Select(x => x.Id);
                var options = _db.AssessmentQuestionOption.Where(x => x.IsDeleted == false && selectedIds.Contains(x.QuestionID))
                    .Select(x => new AssessmentQuestionOption()
                    {
                        QuestionID = x.QuestionID,
                        Id = x.Id,
                        OptionText = x.OptionText,
                        IsCorrectAnswer = x.IsCorrectAnswer
                    }).ToList();

                foreach (AssessmentQuestion oldque in APIAssessmentQuestionOldlist)
                {
                    //get Options list by OldquestionID
                    List<AssessmentQuestionOption> assoption = options.Where(x => x.QuestionID == oldque.Id).ToList();
                    //check option count
                    if (assoption.Count == objAPIAssessmentQuestion.aPIassessmentOptions.Count())
                    {
                        int matchcount = 0;

                        foreach (AssessmentOptions opt in objAPIAssessmentQuestion.aPIassessmentOptions)
                        {
                            bool exists = assoption.Where(w => w.OptionText.ToLower().Contains(opt.OptionText.ToLower())).Any();
                            if (exists)
                            {
                                matchcount++;
                            }
                            else // break if option not exists
                            {
                                break;
                            }

                            if (assoption.Count == matchcount)
                            {
                                return true;
                            }
                        }//foreach
                    } //if                 
                }//foreach

            }
            return false;
        }

    }
}