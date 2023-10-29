using MyCourse.API.APIModel.CourseRating;
using MyCourse.API.Helper;
using MyCourse.API.Model;
using MyCourse.API.Repositories.Interfaces.CourseRating;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
namespace MyCourse.API.Repositories.CourseRating
{
    public class CourseRatingRepository : Repository<Model.CourseRating>, ICourseRatingRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRatingRepository));
        private CourseContext db;
        private ICourseReviewRepository courseReviewRepository;
        public CourseRatingRepository(CourseContext context, ICourseReviewRepository courseReviewRepositoryController) : base(context)
        {
            this.db = context;
            this.courseReviewRepository = courseReviewRepositoryController;
        }

        public bool Exists(int courseId)
        {
            if (this.db.CourseRating.Count(x => (x.CourseId == courseId && x.IsDeleted == Record.NotDeleted)) > 0)
                return true;

            return false;
        }

        public bool CourseReviewExists(int courseId, int UserId)
        {
            if (this.db.CourseReview.Count(x => (x.CourseId == courseId && x.UserId == UserId && x.IsDeleted == Record.NotDeleted)) > 0)
                return true;

            return false;
        }

        public async Task<APICourseRatingAndReview> GetRatingByCourseId(int id)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from courseRating in context.CourseRating
                                  where courseRating.CourseId == id
                                  select new APICourseRatingAndReview
                                  {
                                      Id = courseRating.Id,
                                      CourseId = courseRating.CourseId,
                                      OneStar = courseRating.OneStar,
                                      TwoStar = courseRating.TwoStar,
                                      ThreeStar = courseRating.ThreeStar,
                                      FourStar = courseRating.FourStar,
                                      FiveStar = courseRating.FiveStar,
                                      Average = courseRating.Average
                                  });
                    return await result.OrderByDescending(x => x.Id).SingleOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<APICourseRatingMerged> GetReviewByCourseId(int courseid)
        {
            Model.CourseRating courseRating = db.CourseRating.Where(c => c.CourseId == courseid).SingleOrDefault();
            APICourseRatingMerged aPICourseRatingMergedsData = new APICourseRatingMerged();
            aPICourseRatingMergedsData.CourseId = courseRating.CourseId;
            aPICourseRatingMergedsData.Id = courseRating.Id;
            aPICourseRatingMergedsData.OneStar = courseRating.OneStar;
            aPICourseRatingMergedsData.TwoStar = courseRating.TwoStar;
            aPICourseRatingMergedsData.ThreeStar = courseRating.ThreeStar;
            aPICourseRatingMergedsData.FourStar = courseRating.FourStar;
            aPICourseRatingMergedsData.FiveStar = courseRating.FiveStar;
            aPICourseRatingMergedsData.Average = courseRating.Average;
            aPICourseRatingMergedsData.OneStarPercentage = courseRating.OneStar == 0 ? 0 : (courseRating.OneStar * 100) / (courseRating.OneStar + courseRating.TwoStar + courseRating.ThreeStar + courseRating.FourStar + courseRating.FiveStar);
            aPICourseRatingMergedsData.TwoStarPercentage = courseRating.TwoStar == 0 ? 0 : (courseRating.TwoStar * 100) / (courseRating.OneStar + courseRating.TwoStar + courseRating.ThreeStar + courseRating.FourStar + courseRating.FiveStar);
            aPICourseRatingMergedsData.ThreeStarPercentage = courseRating.ThreeStar == 0 ? 0 : (courseRating.ThreeStar * 100) / (courseRating.OneStar + courseRating.TwoStar + courseRating.ThreeStar + courseRating.FourStar + courseRating.FiveStar);
            aPICourseRatingMergedsData.FourStarPercentage = courseRating.FourStar == 0 ? 0 : (courseRating.FourStar * 100) / (courseRating.OneStar + courseRating.TwoStar + courseRating.ThreeStar + courseRating.FourStar + courseRating.FiveStar);
            aPICourseRatingMergedsData.FiveStarPercentage = courseRating.FiveStar == 0 ? 0 : (courseRating.FiveStar * 100) / (courseRating.OneStar + courseRating.TwoStar + courseRating.ThreeStar + courseRating.FourStar + courseRating.FiveStar);

            Model.Course course = db.Course.Where(c => c.Id == aPICourseRatingMergedsData.CourseId).SingleOrDefault();
            aPICourseRatingMergedsData.CourseTitle = course.Title;

            int noReview = db.CourseReview.Where(c => c.CourseId == courseid).Count();

            int numberOfRecord = 0;
            List<APICourseReview> userReviews = new List<APICourseReview>();
            List<Model.CourseReview> courseReviewMasters = await courseReviewRepository.GetAll(o => o.RatingId == courseRating.Id);
            foreach (Model.CourseReview option in courseReviewMasters)
            {
                DateTime dateValue = new DateTime();
                DateTime outputDateTimeValue;
                numberOfRecord++;
                APICourseReview opt = new APICourseReview();
                opt.Id = option.Id;
                opt.ReviewRating = option.ReviewRating;
                opt.ReviewText = option.ReviewText;
                opt.CourseId = option.CourseId;
                opt.RatingId = option.RatingId;
                opt.UserId = option.UserId;
                opt.UseName = option.UseName;
                opt.Date = option.CreatedDate.ToString();

                if (DateTime.TryParse(opt.Date.ToString(), out outputDateTimeValue))
                {
                    dateValue = outputDateTimeValue;

                    if (dateValue != DateTime.MinValue)
                        opt.Date = dateValue.ToString("MMM dd, yyyy");
                }


                userReviews.Add(opt);
                

            }

            aPICourseRatingMergedsData.aPICourseReview = userReviews.ToArray();

            return aPICourseRatingMergedsData;
        }
        public async Task<Model.CourseRating> GetCourseRating(int courseId)
        {
            Model.CourseRating courseRating = await (from CourseRating in this.db.CourseRating
                                                                  where CourseRating.CourseId == courseId
                                                                  select CourseRating).FirstOrDefaultAsync();
            return courseRating;
        }

        public async Task<string> GetUserCompletion(int CourseId, int UserId)
        {
            string UserCompletion = null;
           UserCompletion = await this.db.CourseCompletionStatus.Where(a => a.CourseId == CourseId && a.UserId == UserId && a.IsDeleted == Record.NotDeleted).Select(a => a.Status).FirstOrDefaultAsync();
            return UserCompletion;
        }
    }
}
