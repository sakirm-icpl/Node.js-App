using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class ExternalCourseCategoryAssociation
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int CategoryId{ get; set; }
        [Required]

        public bool IsDeleted { get; set; }
        public int SubCategoryId{ get; set; }
        public int SubSubCategoryId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
    }

   
}
