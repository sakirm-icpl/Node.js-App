using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{ 
    public class CourseAuthorAssociation 
    {
        public int Id { get; set; }       
        public int UserId { get; set; }
        [Required]
        public int CourseId { get; set; }
        public int CreatedBy { get; set; }       
        public DateTime CreatedDate { get; set; }      
        public int IsDeleted { get; set; }

    }
}

