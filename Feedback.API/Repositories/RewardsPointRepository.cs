using log4net;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Models;
using Feedback.API.Model;
using Microsoft.EntityFrameworkCore;
using Feedback.API.Common;
using Feedback.API.APIModel;
using Feedback.API.Helper.Metadata;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

namespace Feedback.API.Repositories
{
    public class RewardsPointRepository : IRewardsPointRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RewardsPointRepository));
        private FeedbackContext _db;
        ICustomerConnectionStringRepository _customerConnection;

        public RewardsPointRepository(FeedbackContext db,
            ICustomerConnectionStringRepository customerConnection)
        {
            this._db = db;
            this._customerConnection = customerConnection;

        }

        public async Task<int> AddCourseCompletionBaseOnDate(Model.Course course, int userId, int dateTime, string OrgCode)
        {
            try
            {
                if (course != null)
                {

                    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
                    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
                    string CourseCompletedCategory = RewardPointCategory.Normal;
                    string CourseCompletedCondition = RewardPointCategory.CourseCompletedForNormal;
                    var RewardDescription = rewardPointsCache.Where(x => x.Category == CourseCompletedCategory && x.Condition == CourseCompletedCondition).Select(x => x.RewardDescription).ToList();
                    CourseCompletionStatus courseCompletionStatus2 = await this._db.CourseCompletionStatus.Where(s => s.IsDeleted == false && s.CourseId == course.Id && s.UserId == userId).FirstOrDefaultAsync();
                    string FunctionCode1 = RewardsFunctionCode.CourseManagement;
                    string CourseDescription = this.Replace(course.Title, RewardDescription[0]);
                    int referenceId1 = courseCompletionStatus2.Id;
                    await RewardPointSave(FunctionCode1, CourseCompletedCategory, referenceId1, userId, null, CourseDescription);


                    if (dateTime == 0)
                    {
                        string Category = RewardPointCategory.Bonus;
                        string Condition = RewardPointCategory.Donesameday;

                        string FunctionCode = RewardsFunctionCode.CourseManagement;
                        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        string CourseDescriptionwithin2days = this.Replace(course.Title, RewardDescription[0]);
                        int referenceId = courseCompletionStatus2.Id;

                        await RewardPointSave(FunctionCode, Category, referenceId, userId, null, CourseDescriptionwithin2days);

                    }
                    else if (dateTime <= 2)
                    {
                        string Category = RewardPointCategory.BonusLevel1;
                        string Condition = RewardPointCategory.CourseCompletedwithin2days;

                        string FunctionCode = RewardsFunctionCode.CourseManagement;
                        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        string CourseDescriptionwithin2days = this.Replace(course.Title, RewardDescription[0]);
                        int referenceId = courseCompletionStatus2.Id;

                        await RewardPointSave(FunctionCode, Category, referenceId, userId, null, CourseDescriptionwithin2days);



                    }
                    else if (dateTime > 2 && dateTime <= 5)
                    {
                        string Category = RewardPointCategory.BonusLevel2;
                        string Condition = RewardPointCategory.CourseCompletedwithin5days;

                        string FunctionCode = RewardsFunctionCode.CourseManagement;
                        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        string Description = this.Replace(course.Title, RewardDescription[0]);
                        int referenceId = courseCompletionStatus2.Id;
                        await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);

                    }
                    else if (dateTime > 5 && dateTime <= 7)
                    {

                        string Category = RewardPointCategory.BonusLevel3;
                        string Condition = RewardPointCategory.CourseCompletedwithin7days;
                        string FunctionCode = RewardsFunctionCode.CourseManagement;
                        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        string Description = this.Replace(course.Title, RewardDescription[0]);
                        int referenceId = courseCompletionStatus2.Id;
                        await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);

                    }
                    else if (dateTime > 7 && dateTime <= 15)
                    {

                        string Category = RewardPointCategory.BonusLevel4;
                        string Condition = RewardPointCategory.CourseCompletedwithin15days;
                        Model.Course Course = await this._db.Course.Where(s => s.IsDeleted == false && s.Id == course.Id).FirstOrDefaultAsync();
                        string FunctionCode = RewardsFunctionCode.CourseManagement;
                        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        string Description = this.Replace(course.Title, RewardDescription[0]);
                        int referenceId = courseCompletionStatus2.Id;
                        await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);

                    }


                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 1;
        }

        public async Task<int> AddCourseReward(Model.Course course, int userId, string OrgCode)
        {
            try
            {
                string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
                List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);

                int? point = null;
                string FunctionCode = null;
                string Category = null;
                string Condition = null;
                string Description = null;
                if (course.Mission != null && course.Points != 0)
                {
                    point = course.Points;
                    if (String.Equals(course.Mission, RewardsPointMessage.Boss, StringComparison.OrdinalIgnoreCase))
                    {
                        FunctionCode = RewardsPointMessage.BossMissionFunctionCode;
                        Category = RewardsPointMessage.SpecialCategory;
                        Condition = RewardPointCategory.BossMission;
                        var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        Description = this.Replace(course.Title, RewardDescription[0]);
                    }
                    if (String.Equals(course.Mission, RewardsPointMessage.Mini, StringComparison.OrdinalIgnoreCase))
                    {
                        FunctionCode = RewardsPointMessage.MiniMissionFunctionCode;
                        Category = RewardsPointMessage.BonusCategory;
                        Condition = RewardPointCategory.MiniMission;
                        var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        Description = this.Replace(course.Title, RewardDescription[0]);
                    }
                    if (String.Equals(course.Mission, RewardsPointMessage.Normal, StringComparison.OrdinalIgnoreCase))
                    {
                        FunctionCode = RewardsPointMessage.NormalMissionFunctionCode;
                        Category = RewardsPointMessage.NormalCategory;
                        Condition = RewardPointCategory.NormalMission;
                        var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                        Description = this.Replace(course.Title, RewardDescription[0]);
                    }
                    await RewardPointSave(FunctionCode, Category, course.Id, userId, point, Description);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }


            return 1;
        }

        public async Task<List<APIRewardPoints>> checkcache(string cacheKeyConfig)
        {

            CacheManager.CacheManager cache = new CacheManager.CacheManager();
            List<APIRewardPoints> rewardPoints = null;
            if (cache.IsAdded(cacheKeyConfig))
            {
                _logger.Debug("cache isadded");
                rewardPoints = cache.Get<List<APIRewardPoints>>(cacheKeyConfig);
                return rewardPoints;
            }
            else
            {
                _logger.Debug("else of cache isadded");
                rewardPoints = await GetAllRewardPoints();
                cache.Add<List<APIRewardPoints>>(cacheKeyConfig, rewardPoints);
                return rewardPoints;
            }
        }

        public string Replace(string Title, string Description)
        {
            string TitleDescription;
            TitleDescription = Description.Replace("[Title]", Title);
            return TitleDescription;

        }

        public async Task<int> RewardPointSave(string functionCode, string category, int referenceId, int userId, int? point = null, string? description = null)
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "RewardPointsDaily_Upsert";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@FunctionCode", SqlDbType.NVarChar) { Value = functionCode });
                            cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar) { Value = category });
                            cmd.Parameters.Add(new SqlParameter("@ReferenceId", SqlDbType.Int) { Value = referenceId });
                            cmd.Parameters.Add(new SqlParameter("@Point", SqlDbType.Int) { Value = point });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.VarChar) { Value = description });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return 1;
        }

        public async Task<List<APIRewardPoints>> GetAllRewardPoints()
        {
            List<APIRewardPoints> apiCourseList = new List<APIRewardPoints>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        APIRewardPoints apiCourse = null;
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllRewardPoints";
                            cmd.CommandType = CommandType.StoredProcedure;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    apiCourse = new APIRewardPoints
                                    {
                                        Category = row["Category"].ToString(),
                                        Condition = row["Condition"].ToString(),
                                        RewardDescription = row["RewardDescription"].ToString()


                                    };
                                    apiCourseList.Add(apiCourse);
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                        return apiCourseList;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }

        public async Task<int> AddCourseFeedbackReward(int userId, int referenceId, int courseId, string OrgCode)
        {
            string Category = RewardPointCategory.Normal;
            string Condition = RewardPointCategory.Submitted;
            string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
            List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
            var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
            Model.Course Course = await this._db.Course.Where(s => s.IsDeleted == false && s.Id == courseId).FirstOrDefaultAsync();

            string FunctionCode = RewardsFunctionCode.ProvideFeedback;
            string Description = this.Replace(Course.Title, RewardDescription[0]);
            await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
            return 1;
        }

        public async Task<int> AddModuleFeedbackReward(int userId, int referenceId, int courseId, int moduleId, string OrgCode)
        {
            try
            {
                string Category = RewardPointCategory.Normal;
                string Condition = RewardPointCategory.SubmittedModule;
                string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;

                _logger.Debug("before rewardPointsCache");
                List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
                _logger.Debug("after rewardPointsCache");
                var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
                Model.Module module = await this._db.Module.Where(s => s.IsDeleted == false && s.Id == moduleId).FirstOrDefaultAsync();
                string FunctionCode = RewardsFunctionCode.ProvideFeedback;
                string Description = this.Replace(module.Name, RewardDescription[0]);
                await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 1;
        }
    }
}