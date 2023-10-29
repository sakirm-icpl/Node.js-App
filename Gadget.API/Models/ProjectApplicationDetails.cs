using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class ProjectApplicationDetails : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Category { get; set; }
        [MaxLength(200)]
        public string TimePeriod { get; set; }      
        public int ApplicationId { get; set; }
        [MaxLength(700)]
        public string Answer1 { get; set; }
        [MaxLength(700)]
        public string Answer2 { get; set; }
        [MaxLength(1000)]
        public string Answer3 { get; set; }
        [MaxLength(1000)]
        public string Answer4 { get; set; }
        public bool Status { get; set; }
        [MaxLength(500)]
        public string FileName1 { get; set; }
        [MaxLength(2000)]
        public string FilePath1 { get; set; }
        [MaxLength(500)]
        public string FileName2 { get; set; }
        [MaxLength(2000)]
        public string FilePath2 { get; set; }
        [MaxLength(200)]
        public string Scope { get; set; }
        [MaxLength(200)]
        public string KaizenClassified { get; set; }
        [MaxLength(200)]
        public string RefinedClassification { get; set; }
        public int JuryCount { get; set; }
        public string AssignmentStatus { get; set; }
        public string FinalStatus { get; set; }
        public double AvgScore { get; set; }
    }
}
