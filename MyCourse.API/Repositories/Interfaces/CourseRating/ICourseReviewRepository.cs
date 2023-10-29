using MyCourse.API.APIModel.CourseRating;
using MyCourse.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces.CourseRating
{
    public interface ICourseReviewRepository : IRepository<CourseReview>
    {
        Task<List<APICourseRatingAndReview>> Get(int page, int pageSize, string search = null);
        int Count(string search = null);
        Task<bool> ExistCourse(int courseid, int userid);
    }
}
