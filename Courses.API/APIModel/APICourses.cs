using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APICourses
    {
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string CourseType { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public bool IsCertificateIssued { get; set; }
        public bool IsActive { get; set; }
        public bool IsPreAssessment { get; set; }
        public DateTime ExpieryDate { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public Int16 TotalModules { get; set; }
        public string SubCategoryName { get; set; }
        [MinLength(2), MaxLength(10)]
        [Required]
        public string Code { get; set; }
        public string Metadata { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CompletionPeriodDays { get; set; }
        public int Id { get; set; }
        public string IsApplicableToAll { get; set; }
        public string ModifiedBy { get; set; }

        public string DurationInMinutes { get; set; }
    }
}
