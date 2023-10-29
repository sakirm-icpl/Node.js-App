using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.APIModel
{
    public class APICourseSubCategory  
    {
        public int? Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [MinLength(2), MaxLength(8)]
        [Required]
        public string Code { get; set; }
        [Required]
        [MinLength(1), MaxLength(80)]
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public int? SequenceNo { get; set; }
    }
}
