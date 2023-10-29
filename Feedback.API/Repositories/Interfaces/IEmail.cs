using Feedback.API.APIModel;

namespace Courses.API.Repositories.Interfaces
{
    public interface IEmail
    {

        Task<int> SendCourseCompletionStatusMail(string CourseTitle, int UserId, APIUserDetails aPIUserDetails, int CourseId);


    }
}
