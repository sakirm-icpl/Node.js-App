using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model
{
    public class CourseGroupMapping : BaseModel
    {
        public int? Id { get; set; }
       
        [Required]
        public int CourseId { get; set; }     
       
        [Required]
        public int GroupId { get; set; }
       
    }
}
