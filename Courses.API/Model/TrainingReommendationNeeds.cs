using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class TrainingReommendationNeeds : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(500)]
        [Required]
        public string JobRole { get; set; }
   
        [MaxLength(100)]
        [Required]
        public string Department { get; set; }
        [MaxLength(500)]
        [Required]
        public string Section { get; set; }
        [MaxLength(20)]
        [Required]
        public string Level { get; set; }
        [MaxLength(50)]
        [Required]
        public string Status { get; set; }
        [MaxLength(1000)]
        [Required]
        public string TrainingProgram { get; set; }
        [MaxLength(500)]
        [Required]
        public string Category { get; set; }
        public string Year { get; set; }
       
    }

}
