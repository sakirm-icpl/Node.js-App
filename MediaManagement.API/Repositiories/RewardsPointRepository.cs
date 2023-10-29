using MediaManagement.API.APIModel;
using MediaManagement.API.Common;
using MediaManagement.API.Data;
using MediaManagement.API.Metadata;
using MediaManagement.API.Models;
using MediaManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using log4net;
using MediaManagement.API.Helper;
using System.Threading.Tasks;

namespace MediaManagement.API.Repositories
{
    public class RewardsPointRepository : IRewardsPointRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RewardsPointRepository));
        private GadgetDbContext _db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public RewardsPointRepository(GadgetDbContext db, ICustomerConnectionStringRepository customerConnectionString)
        {
            this._db = db;
            this._customerConnectionString = customerConnectionString;
        }
        //public async Task<int> AddSurveySubmitReward(int userId, int surveyId, int IsFirstSurvey,string SurveySubject,DateTime CreatedDate, string OrgCode)
        //{

        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.SurveysManagement;
        //    string Condition = RewardPointCategory.Response;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(SurveySubject, RewardDescription[0]);
        //    int referenceId = surveyId;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    DateTime CurrentDate = DateTime.Now;
        //    DateTime SurveyCreatedDate = CreatedDate;
        //    int DateDifference = Convert.ToInt32((SurveyCreatedDate - CurrentDate).TotalDays);
        //    if (DateDifference == 0)
        //    {
        //        Category = RewardPointCategory.Bonus;
        //        Condition = RewardPointCategory.ResponsOnSameDay;
        //        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //        Description = this.Replace(SurveySubject, RewardDescription[0]);
        //        await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    }
        //    if (IsFirstSurvey == 1)
        //    {
        //        Category = RewardPointCategory.Special;
        //        Condition = RewardPointCategory.FirstResponse;
        //        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //        Description = this.Replace(SurveySubject, RewardDescription[0]);
        //        await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    }

        //    return 1;
        //}
        public async Task<int> AlbumReadRewardPoint(int userId, int albumId, string OrgCode)
        {

            string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
            List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
            MediaLibraryAlbum Album = await this._db.MediaLibraryAlbum.Where(m => m.Id == albumId && m.IsDeleted == false).FirstOrDefaultAsync();
            string Category = RewardPointCategory.Normal;
            string FunctionCode = RewardsFunctionCode.MediaLibrary;
            string Condition = RewardPointCategory.ObservedAnObject;
            var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
            string Description = this.Replace(Album.AlbumName, RewardDescription[0]);
            int referenceId = 0;
            referenceId = albumId;
            await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
            return 1;
        }
        //public async Task<int> AddNewsUpdateReadReward(int id, int userId,string SubHead, string OrgCode)
        //{

        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.NewsUpdates;
        //    string Condition = RewardPointCategory.Read;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(SubHead, RewardDescription[0]);
        //    int referenceId = id;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}
        //public async Task<int> MySuggestionSubmitRewardPoint(int userId , int referenceId,string ContextualAreaofBusiness, string OrgCode)
        //{

        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.MySuggestions;
        //    string Condition = RewardPointCategory.Suggestion;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(ContextualAreaofBusiness, RewardDescription[0]);
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}

        public async Task<int> AlbumLikeDislikeRewardPoint(int userId, int albumId)
        {
            MediaLibraryAlbum Album = await this._db.MediaLibraryAlbum.Where(m => m.Id == albumId).FirstOrDefaultAsync();
            string Category = RewardPointCategory.Bonus;
            string FunctionCode = RewardsFunctionCode.MediaLibrary;
            string Description = "You have Liked/Disliked album " + Album.AlbumName;
            int referenceId = albumId;
            await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
            return 1;
        }

        //public async Task<int> PublicationReadRewardPoint(int userId, int publicationId,string OrgCode)
        //{

        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    int ReferenceId = publicationId;
        //    string FunctionCode = RewardsFunctionCode.Publications;
        //    string Category = RewardPointCategory.Normal;
        //    string Condition = RewardPointCategory.ReadAnArticle;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    Publications Publications = await this._db.Publications.Where(publication => publication.Id == publicationId && publication.IsDeleted == false).FirstOrDefaultAsync();
        //    string Description = this.Replace(Publications.Publication, RewardDescription[0]);
        //    await RewardPointSave(FunctionCode, Category, ReferenceId, userId, null, Description);
        //    if (Publications != null)
        //    {
        //        DateTime CurrentDate = DateTime.Now;
        //        DateTime PublicationCreatedDate = Publications.CreatedDate;
        //        int DateDifference = Convert.ToInt32((PublicationCreatedDate - CurrentDate).TotalDays);
        //        if (DateDifference == 0)
        //        {
        //            Category = RewardPointCategory.Bonus;
        //            Condition = RewardPointCategory.ReadSameDay;
        //            RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //            Description = this.Replace(Publications.Publication, RewardDescription[0]);
        //            await RewardPointSave(FunctionCode, Category, ReferenceId, userId, null, Description);
        //        }
        //    }
        //    //await RewardPointSave(FunctionCode, Category, ReferenceId, userId, null, Description);
        //    return 1;
        //}
        //public async Task<int> InterestingArticalReadRewardPoint(int id, int userId,string OrgCode)
        //{

        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    InterestingArticles Artical = await this._db.InterestingArticles.Where(m => m.CategoryId == id).FirstOrDefaultAsync();
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.InterestingArticles;
        //    string Condition = RewardPointCategory.Readarticle;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(Artical.Article, RewardDescription[0]);
        //    int referenceId = id;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}

        //public async Task<int> InterestingArticalRewardPoint(int id, int userId, string OrgCode)
        //{

        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    InterestingArticles Artical = await this._db.InterestingArticles.Where(m => m.Id == id).FirstOrDefaultAsync();
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.InterestingArticles;
        //    string Condition = RewardPointCategory.Readarticle;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(Artical.Article, RewardDescription[0]);
        //    int referenceId = id;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}
        //public async Task<int> InterestingArticalLikeDislikeRewardPoint(int userId, int articalId)
        //{
        //    InterestingArticles Artical = await this._db.InterestingArticles.Where(m => m.Id == articalId).FirstOrDefaultAsync();
        //    string Category = RewardPointCategory.Bonus;
        //    string FunctionCode = RewardsFunctionCode.InterestingArticles;
        //    string Description = "You have read the liked/Disliked interesting article " + Artical.Article;
        //    int referenceId = articalId;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}
        //public async Task<int> PollsResponseRewardPoint(int userId, int pollId,string Question, string OrgCode)
        //{
        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.OpinionPolls;
        //    string Condition = RewardPointCategory.ResponseOk;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(Question, RewardDescription[0]);
        //    int referenceId = pollId;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}
        //public async Task<int> QuizAttemptedRewardPoint(int userId, int quizId, bool IsFullMarks, string QuizTitle, DateTime CreatedDate, string OrgCode)
        //{
        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.QuizzesManagement;
        //    string Condition = RewardPointCategory.Attempted;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description =this.Replace(QuizTitle,RewardDescription[0]);
        //    int referenceId = quizId;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //        DateTime CurrentDate = DateTime.Now;
        //        DateTime QuizCreatedDate = CreatedDate;
        //        int DateDifference = Convert.ToInt32((QuizCreatedDate - CurrentDate).TotalDays);
        //        if (DateDifference == 0)
        //        {
        //            Category = RewardPointCategory.Bonus;
        //            Condition = RewardPointCategory.AttemptedSameDay;
        //            RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //            Description = this.Replace(QuizTitle, RewardDescription[0]);
        //        }

        //    if (IsFullMarks)
        //    {
        //        Category = RewardPointCategory.Special;
        //        Condition = RewardPointCategory.FullMarks;
        //        RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //        Description = this.Replace(QuizTitle, RewardDescription[0]);
        //    }
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}
        public async Task<List<APIRewardPoints>> checkcache(string cacheKeyConfig)
        {
            CacheManager.CacheManager cache = new CacheManager.CacheManager();
            List<APIRewardPoints> rewardPoints = null;
            if (cache.IsAdded(cacheKeyConfig))
            {
                rewardPoints = cache.Get<List<APIRewardPoints>>(cacheKeyConfig);
                return rewardPoints;
            }
            else
            {
                rewardPoints = await GetAllRewardPoints();
                cache.Add<List<APIRewardPoints>>(cacheKeyConfig, rewardPoints);
                return rewardPoints;
            }
        }
        //public async Task<int> SuggestionManagementRewardPoint(int userId, int suggestionId,string Suggestion,string OrgCode)
        //{
        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.MySuggestions;
        //    string Condition = RewardPointCategory.AddedSuggestions;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(Suggestion, RewardDescription[0]);
        //    int referenceId = suggestionId;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}

        //public async Task<int> MySuggestionRewardPoint(int userId, int suggestionId,string DetailedDescription, string OrgCode)
        //{
        //    string cacheKeyConfig = Constants.RATING_REWARD + "-" + OrgCode;
        //    List<APIRewardPoints> rewardPointsCache = await this.checkcache(cacheKeyConfig);
        //    string Category = RewardPointCategory.Normal;
        //    string FunctionCode = RewardsFunctionCode.MySuggestions;
        //    string Condition = RewardPointCategory.AddedMySuggestion;
        //    var RewardDescription = rewardPointsCache.Where(x => x.Category == Category && x.Condition == Condition).Select(x => x.RewardDescription).ToList();
        //    string Description = this.Replace(DetailedDescription, RewardDescription[0]);
        //    int referenceId = suggestionId;
        //    await RewardPointSave(FunctionCode, Category, referenceId, userId, null, Description);
        //    return 1;
        //}
        public async Task<int> RewardPointSave(string functionCode, string category, int referenceId, int userId, int? point = null, string description = null)
        {
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "RewardPointsDaily_Upsert";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FunctionCode", SqlDbType.NVarChar) { Value = functionCode });
                        cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar) { Value = category });
                        cmd.Parameters.Add(new SqlParameter("@ReferenceId", SqlDbType.Int) { Value = referenceId });
                        cmd.Parameters.Add(new SqlParameter("@Point", SqlDbType.Int) { Value = point });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.VarChar) { Value = description });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            return 1;
        }
        public string Replace(string Title, string Description)
        {
            string TitleDescription;
            TitleDescription = Description.Replace("[Title]", Title);
            return TitleDescription;

        }
        public async Task<string> RewardPointsDescription(string Category,string Condition, string FunctionCode)
        {
            try
            {
                string RewardDescription = null;
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[GetRewardDescriptionByCategory]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Condition", SqlDbType.NVarChar) { Value = Condition });
                        cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar) { Value = Category });
                        cmd.Parameters.Add(new SqlParameter("@FunctionCode", SqlDbType.NVarChar) { Value = FunctionCode });

                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            RewardDescription = (row["RewardDescription"].ToString());
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
                return RewardDescription;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
             //return RewardDescription;
        }
        public async Task<List<APIRewardPoints>> GetAllRewardPoints()
        {
            List<APIRewardPoints> apiCourseList = new List<APIRewardPoints>();
            try
            {
                using (var dbContext = this._customerConnectionString.GetDbContext())
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
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }

        }
    }
}
