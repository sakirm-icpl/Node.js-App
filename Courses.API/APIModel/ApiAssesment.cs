using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class ApiAssesment
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string MetaData { get; set; }
        public string Description { get; set; }
        public Question[] Quetions { get; set; }
    }
    public class Question
    {
        [Range(0, int.MaxValue)]
        public int Id { get; set; }
    }
}
