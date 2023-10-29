using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using log4net;
using Feedback.API.Models;
using Feedback.API.APIModel;
using Feedback.API.Model.Feedback;

namespace Feedback.API.Repositories
{
    public class FeedbackStatusDetailRepository : Repository<FeedbackStatusDetail>, IFeedbackStatusDetail
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedbackStatusDetailRepository));
        private FeedbackContext _db;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
        public FeedbackStatusDetailRepository(ICustomerConnectionStringRepository customerConnectionRepository, FeedbackContext context) : base(context)
        {
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
        }

        public async Task<bool> Exists(string search)

        {
            if (_db.FeedbackStatusDetail.Count(f => f.IsDeleted == false) > 0)
                return true;
            return false;
        }
        public async Task<int> Count(string? search = null, string? filter = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await _db.FeedbackStatusDetail.Where(f => f.IsDeleted == false).CountAsync();
            return await _db.FeedbackStatusDetail.Where(f => f.IsDeleted == false).CountAsync();
        }

        public async Task<List<FeedbackStatusDetail>> Get(int page, int pageSize, string? search = null, string? filter = null)
        {
            IQueryable<FeedbackStatusDetail> Query = _db.FeedbackStatusDetail.Where(c => c.IsDeleted == false);
            if (!string.IsNullOrEmpty(search))
            {

            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return await Query.ToListAsync();
        }
        public async Task<List<CourseFeedbackAPI>> GetFeedback(int configurationId)
        {
            List<CourseFeedbackAPI> Questions = await (from questions in this._db.FeedbackQuestion
                                                       join confige in this._db.FeedbackSheetConfigurationDetails on questions.Id equals confige.FeedbackId
                                                       where (confige.ConfigurationSheetId == configurationId && questions.IsActive == true && questions.IsDeleted == false)
                                                       select new CourseFeedbackAPI
                                                       {
                                                           Id = questions.Id,
                                                           QuestionText = questions.QuestionText,
                                                           QuestionType = questions.QuestionType,
                                                           NoOfOption = questions.AnswersCounter,
                                                           Section = questions.Section
                                                       }).ToListAsync();
            for (int i = 0; i < Questions.Count(); i++)
            {
                if (Questions[i].QuestionType.ToLower().Equals("objective"))
                {
                    Questions[i].Options = (from option in _db.FeedbackOption
                                            where option.FeedbackQuestionID == Questions[i].Id && option.IsDeleted == false
                                            select new Option
                                            {
                                                option = option.OptionText
                                            }).ToArray();
                }
            }

            return Questions;
        }

        public async Task<bool> IsDependacyExist(int feedbackId)
        {
            int count = await (from feedback in _db.FeedbackSheetConfigurationDetails
                               join conf in _db.FeedbackSheetConfiguration on feedback.ConfigurationSheetId equals conf.Id
                               join lcms in _db.LCMS on conf.Id equals lcms.FeedbackSheetConfigID
                               where (feedback.FeedbackId == feedbackId && conf.IsDeleted == false)
                               select new { feedback.Id }).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<ApiResponse> AddForFeedbackAggregationReport(SubmitFeedback submitFeedback, string UserName)
        {
            try
            {
                ApiResponse objApiResponse = new ApiResponse();
                int Rating = 0;
                Rating = await this._db.FeedbackOption.Where(a => a.Id == submitFeedback.OptionID).Select(a => a.Rating).FirstOrDefaultAsync();

                UserWiseFeedbackAggregation obj = new UserWiseFeedbackAggregation();
                string TableName = null;

                obj = await this._db.UserWiseFeedbackAggregation.Where(a => a.CourseId == submitFeedback.CourseId && a.FeedbackQuestionID == submitFeedback.QuestionID
                                                                           && a.FeedbackOptionID == submitFeedback.OptionID && a.IsDeleted == false).FirstOrDefaultAsync();

                if (obj == null)
                {
                    UserWiseFeedbackAggregation objNew = new UserWiseFeedbackAggregation();

                    objNew.CourseId = submitFeedback.CourseId;
                    objNew.FeedbackQuestionID = submitFeedback.QuestionID;
                    objNew.FeedbackOptionID = submitFeedback.OptionID;

                    this._db.UserWiseFeedbackAggregation.Add(objNew);
                    await this._db.SaveChangesAsync();
                }

                TableName = "Course.UserWiseFeedbackAggregation";

                AlterAndAddValuesToTable(TableName, UserName, Rating, submitFeedback);
                objApiResponse.Description = "success";
                objApiResponse.StatusCode = 200;
                return objApiResponse;
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        private async Task<ApiResponse> AlterAndAddValuesToTable(string TableName, string UserName, int Rating, SubmitFeedback submitFeedback)
        {
            ApiResponse objApiResponse = new ApiResponse();
            try
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "AlterAndAddValuesToTable";
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add(new SqlParameter("@TableName", SqlDbType.NVarChar) { Value = TableName });
                            cmd.Parameters.Add(new SqlParameter("@ColumnName", SqlDbType.NVarChar) { Value = UserName });
                            cmd.Parameters.Add(new SqlParameter("@Rating", SqlDbType.NVarChar) { Value = Rating });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.NVarChar) { Value = submitFeedback.CourseId });
                            cmd.Parameters.Add(new SqlParameter("@QuestionID", SqlDbType.NVarChar) { Value = submitFeedback.QuestionID });
                            cmd.Parameters.Add(new SqlParameter("@OptionID", SqlDbType.NVarChar) { Value = submitFeedback.OptionID });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                int Value = Convert.ToInt32(row["Value"].ToString());
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            objApiResponse.Description = "success";
            objApiResponse.StatusCode = 200;
            return objApiResponse;
        }

        private void AlterTable(string UserName)
        {
            Dictionary<string, string> dictParam = new Dictionary<string, string>();
            dictParam.Add("@TableName", "UserWiseFeedbackAggregation");
            dictParam.Add("@ColumnName", UserName);
            ExecSQLStatement("ALTER TABLE @TableName ALTER COLUMN @ColumnName int", dictParam);
        }

        private void AddValuesToColumn(string UserName, int Rating, int QuestionID, int? OptionID, int CourseId)
        {
            Dictionary<string, string> dictParam = new Dictionary<string, string>();
            dictParam.Add("@TableName", "UserWiseFeedbackAggregation");
            dictParam.Add("@UserName", UserName);
            dictParam.Add("@Rating", Rating.ToString());
            dictParam.Add("@QuestionID", QuestionID.ToString());
            dictParam.Add("@OptionID", OptionID.ToString());
            dictParam.Add("@CourseId", CourseId.ToString());
            ExecSQLStatement("UPDATE @TableName SET @UserName = @Rating WHERE CourseId = @CourseId AND FeedbackQuestionID = @QuestionID AND FeedbackOptionID = @OptionID", dictParam);
        }

        public void ExecSQLStatement(string Sql, Dictionary<string, string> Params)
        {
            try
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            foreach (string dictKey in Params.Keys)
                                cmd.Parameters.Add(new SqlParameter(dictKey, Params[dictKey]));
                            cmd.ExecuteNonQuery();
                            connection.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
        }
    }
}
