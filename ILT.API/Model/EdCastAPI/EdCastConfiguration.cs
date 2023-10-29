using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ILT.API.Model.EdCastAPI
{
    public class EdCastConfiguration
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string LxpDetails { get; set; }
        [MaxLength(500)]
        [Required]
        public string CourseUrl { get; set; }
        [MaxLength(500)]
        [Required]
        public string LmsClientID { get; set; }

        [MaxLength(100)]
        [Required]
        public string LmsClientSecrete { get; set; }
        [MaxLength(100)]
        [Required]
        public string LmsHost { get; set; }
        [MaxLength(500)]
        [Required]
        public string V5APIKey { get; set; }
        [MaxLength(500)]
        [Required]
        public string V5Secrete { get; set; }
        [MaxLength(100)]
        [Required]
        public string V5HostURL { get; set; }
   
        [MaxLength(100)]
        [Required]
        public string EmpoweredHost { get; set; }
        
    }
}
