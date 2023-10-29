using MyCourse.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.APIModel
{
    public class APICourseCategory
    {
        public int? Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int? SequenceNo { get; set; }
    }
}
