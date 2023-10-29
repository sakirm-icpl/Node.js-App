using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class EBTDetails : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public string courseTitle { get; set; }
        public string FromData { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        [MaxLength(20)]
        public string Status { get; set; }
    }
}
