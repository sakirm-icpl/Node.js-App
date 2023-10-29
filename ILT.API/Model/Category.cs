using System;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model
{
    public class Category
    {
        public int Id { get; set; }
        [MinLength(2), MaxLength(20)]
        [Required]
        public string Code { get; set; }
        [Required]
        [MinLength(1), MaxLength(50)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ImagePath { get; set; }
        public int? SequenceNo { get; set; }
        public bool? IsExternalCategory { get; set; }
        public string? ExternalCategoryProvider { get; set; }

    }
}
