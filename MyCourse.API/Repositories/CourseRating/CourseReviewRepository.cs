using MyCourse.API.APIModel.CourseRating;
using MyCourse.API.Helper;
using MyCourse.API.Model;
using MyCourse.API.Model;
using MyCourse.API.Repositories.Interfaces.CourseRating;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.CourseRating
{
    public class CourseReviewRepository : Repository<CourseReview>, ICourseReviewRepository
    {
        private CourseContext db;
        public CourseReviewRepository(CourseContext context) : base(context)
        {
            this.db = context;
        }
        public async Task<List<APICourseRatingAndReview>> Get(int page, int pageSize, string search = null)
        {


            IQueryable<APICourseRatingAndReview> Query = (from courseReview in db.CourseReview
                                                          where courseReview.IsDeleted == false

                                                          select new APICourseRatingAndReview
                                                          {
                                                              ReviewText = courseReview.ReviewText,
                                                              ReviewRating = courseReview.ReviewRating,
                                                              CourseId = courseReview.CourseId,
                                                              Id = courseReview.Id,
                                                              UseName = courseReview.UseName
                                                          });


            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.ReviewText.Contains(search));
            }

            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            List<APICourseRatingAndReview> courseRatingAndReview = await Query.ToListAsync();
            return courseRatingAndReview;
        }

        public int Count(string search = null)
        {
            IQueryable<CourseReview> Query = null;
            if (!string.IsNullOrWhiteSpace(search))
                Query = db.CourseReview.Where(f => f.ReviewText.Contains(search) && f.IsDeleted == false);
            else
                Query = db.CourseReview.Where(r => r.IsDeleted == false);
            return Query.Count();
        }

        public async Task<bool> ExistCourse(int courseId, int userid)
        {
            var count = await this.db.CourseReview.Where(p => (p.CourseId == courseId && p.UserId == userid && p.IsDeleted == Record.NotDeleted)).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
    }
}
