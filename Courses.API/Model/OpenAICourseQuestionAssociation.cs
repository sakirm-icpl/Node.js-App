using Assessment.API.Models;

namespace Courses.API.Model
{
    public class OpenAICourseQuestionAssociation : CommonFields
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public int QuestionId { get; set; }

    }
}
