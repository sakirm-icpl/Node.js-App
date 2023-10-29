namespace Courses.API.Model
{
    public class DegreedContent : BaseModel
    {
        public int Id { get; set; }
        public string ContentId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
    }
}
