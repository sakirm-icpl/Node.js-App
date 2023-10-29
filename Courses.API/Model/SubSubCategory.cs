using System.ComponentModel.DataAnnotations;
using System;

namespace Courses.API.Model
{
    public class SubSubCategory
    {
        public int Id { get; set; }
        [Required]
        public int SubCategoryId { get; set; }
        [MinLength(1), MaxLength(8)]
        [Required]
        public int CategoryId { get; set; }
        [MinLength(1), MaxLength(8)]
        [Required]
        public string Code { get; set; }
        [Required] 
        [MinLength(1), MaxLength(80)]
        public string Name { get; set; }
      //  public string SubCategoryName { get; set; }
      //  public string CategoryName { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? SequenceNo { get; set; }
        public bool? IsExternalSubSubCategory { get; set; }
        public string? ExternalSubSubCategoryProvider { get; set; }
    }
}
