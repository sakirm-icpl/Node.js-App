namespace Courses.API.Model
{
    public class LcmsQuestionAssociation : BaseModel
    {
        public int Id { get; set; }
        public int LcmsId { get; set; }
        public int QuetionId { get; set; }
    }
}
