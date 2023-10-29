using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model
{
    public class ExternalTrainingRequest : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(200)]

        public string RequestCode { get; set; }
        [MaxLength(500)]
        [Required]
        public string Title { get; set; }

        public float Fee { get; set; }
        [MaxLength(25)]
        public string Currency { get; set; }

        [MaxLength(200)]
        public string Trainer { get; set; }
        [MaxLength(200)]
        public string ContentUrl { get; set; }
       
        public string Traveling { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(200)]
        public string Status { get; set; }
        [MaxLength(200)]
        public string Reason { get; set; }

        
    }

}
