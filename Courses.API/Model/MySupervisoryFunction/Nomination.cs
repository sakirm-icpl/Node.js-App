using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.MySupervisoryFunction
{
    public class Nomination : BaseModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public int? CourseId { get; set; }
        [Required]
        public int? ModuleId { get; set; }
        [Required]
        public string BactchCode { get; set; }
        public DateTime StartDate { get; set; }
        public string TrainingPlace { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public bool Nominate { get; set; }
    }
}
