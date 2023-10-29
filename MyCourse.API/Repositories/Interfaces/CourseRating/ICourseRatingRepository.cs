using MyCourse.API.APIModel.CourseRating;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces.CourseRating
{
    public interface ICourseRatingRepository : IRepository<Model.CourseRating>
    {
        bool Exists(int courseID);
        bool CourseReviewExists(int courseId, int UserId);
        Task<Model.CourseRating> GetCourseRating(int courseId);
        Task<APICourseRatingAndReview> GetRatingByCourseId(int id);
        Task<APICourseRatingMerged> GetReviewByCourseId(int id);
        Task<string> GetUserCompletion(int CourseId, int UserId);

    }
}
