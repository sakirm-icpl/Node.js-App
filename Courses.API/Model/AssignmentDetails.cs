using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class AssignmentDetails : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string FilePath { get; set; }

        public string FileType { get; set; }

        public string TextAnswer { get; set; }

        public string Status { get; set; }

        public string Remark { get; set; }



    }
}
