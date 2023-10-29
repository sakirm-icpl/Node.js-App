using Assessment.API.APIModel;
using Newtonsoft.Json.Linq;

namespace Assessment.API.Repositories.Interfaces
{
    public interface IEmail
    {
        Task<int> SendCourseCompletionStatusMail(string CourseTitle, int UserId, APIUserDetails aPIUserDetails, int CourseId);
        Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject);
    }
}
