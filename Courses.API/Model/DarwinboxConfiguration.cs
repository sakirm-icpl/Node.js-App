using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model
{
    public class DarwinboxConfiguration
    {
        public int Id { get; set; }     
        [MaxLength(500)]
        [Required]
        public string DarwinboxCourseUrl { get; set; }
        [MaxLength(500)]
        [Required]
        public string DarwinboxHost { get; set; }

        [MaxLength(100)]
        [Required]
        public string Username { get; set; }
        [MaxLength(100)]
        [Required]
        public string Password { get; set; }
        [MaxLength(500)]
        [Required]
        public string Create_LA { get; set; }
        [MaxLength(500)]
        [Required]
        public string Update_LA { get; set; }
        [MaxLength(50)]
        [Required]
        public string Program { get; set; }
   
        [MaxLength(50)]
        [Required]
        public string Vendor { get; set; }
        [MaxLength(200)]
        //[Required]
        public string APIKey { get; set; }

    }
}
