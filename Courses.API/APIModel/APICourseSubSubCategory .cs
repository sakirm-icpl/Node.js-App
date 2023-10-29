using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APICourseSubSubCategory 
    {
        public int? Id { get; set; }
        [Required]

        public int CategoryId { get; set; }
        [Required]

        public int SubCategoryId { get; set; }

        [Required]

        
        public string Code { get; set; }
        [Required]

        public string Name { get; set; }
        public string CategoryName { get; set; }

        public string SubCategoryName { get; set; }
        public int? SequenceNo { get; set; }
    }
}
