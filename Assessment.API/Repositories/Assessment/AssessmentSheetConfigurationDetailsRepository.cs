using Assessment.API.Common;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Assessment.API.Repositories
{
    public class AssessmentSheetConfigurationDetailsRepository : Repository<AssessmentSheetConfigurationDetails>, IAssessmentSheetConfigurationDetails
    {
        private AssessmentContext _db;

        public AssessmentSheetConfigurationDetailsRepository(AssessmentContext context) : base(context)
        {
            this._db = context;
        }
        public AssessmentSheetConfigurationDetails GetByConfigid(int? assessmentSheetConfigId)
        {
            return this._db.AssessmentSheetConfigurationDetails.AsNoTracking().Where(c => c.AssessmentSheetConfigID == assessmentSheetConfigId).FirstOrDefault();
        }

        public AssessmentSheetConfigurationDetails GetConfigurationDetails(int QuestionID, int? assessmentDetailsId)
        {
            return _db.AssessmentSheetConfigurationDetails.Where(c => c.QuestionID == QuestionID && c.AssessmentSheetConfigID == assessmentDetailsId && c.IsDeleted == false).FirstOrDefault();
        }
        public List<AssessmentSheetConfigurationDetails> GetConfigurations(int? assessmentDetailsId)
        {
            return _db.AssessmentSheetConfigurationDetails.Where(c => c.AssessmentSheetConfigID == assessmentDetailsId && c.IsDeleted == false).ToList();
        }
        public int RemoveQuestions(List<AssessmentSheetConfigurationDetails> listquestion)
        {
            _db.AssessmentSheetConfigurationDetails.RemoveRange(listquestion);
            return 1;
        }

        public int GetTotalQuestion(int AssessmentSheetConfigID, string OrgCode)
        {
            var cache = new CacheManager.CacheManager();
            string cacheKeyConfig = CacheKeyNames.TOTAL_QUESTIONS_IN_ASSESSMENT + AssessmentSheetConfigID + OrgCode;
            int totalQuestions;
            if (cache.IsAdded(cacheKeyConfig))
                totalQuestions = Convert.ToInt32(cache.Get<string>(cacheKeyConfig));
            else
            {
                totalQuestions = (from a in _db.AssessmentSheetConfigurationDetails.AsNoTracking()
                                  where a.AssessmentSheetConfigID == AssessmentSheetConfigID
                                  select a.QuestionID).Count();
                cache.Add(cacheKeyConfig, totalQuestions.ToString(), System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
            }
            return totalQuestions;
        }
        public async Task<int> DeleteQuestion(int ConfigurationID, int[] QuestionsId, int UserId)
        {
            List<AssessmentSheetConfigurationDetails> Questions = new List<AssessmentSheetConfigurationDetails>();
            foreach (int QuestionId in QuestionsId)
            {
                AssessmentSheetConfigurationDetails Question = await this.Get(c => c.QuestionID == QuestionId && c.AssessmentSheetConfigID == ConfigurationID && c.IsDeleted == false);
                if (Question != null)
                {
                    Question.IsDeleted = true;
                    Question.ModifiedDate = DateTime.UtcNow;
                    Question.ModifiedBy = UserId;
                    Questions.Add(Question);
                }
            }
            if (Questions.Count > 0)
                await this.UpdateRange(Questions);
            return 1;
        }

    }
}
