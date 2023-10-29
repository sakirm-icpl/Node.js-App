using System;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class SubCategory
    {
        public int Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [MinLength(2), MaxLength(8)]
        [Required]
        public string Code { get; set; }
        [Required]
        [MinLength(1), MaxLength(80)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? SequenceNo { get; set; }
        public bool? IsExternalSubCategory { get; set; }
        public string? ExternalSubCategoryProvider { get; set; }
    }

   
}
