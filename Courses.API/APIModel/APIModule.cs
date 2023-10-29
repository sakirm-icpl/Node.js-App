using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Validations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIModule
    {
       public int? Id { get; set; }
        [MinLength(2), MaxLength(300)]
        [Required]
        public string Name { get; set; }
        [MaxLength(30)]
        public string ModuleType { get; set; }
        [MaxLength(30)]
        [Required]
        [CourseType]
        public string CourseType { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }
        public int? Duration { get; set; }
        public int? CreditPoints { get; set; }
        [MaxLength(25)]
        public string AssessmentType { get; set; }
        public bool IsFeedback { get; set; }
        public int FeedbackId { get; set; }
        public bool IsPreAssessment { get; set; }
        public int PreAssessmentId { get; set; }
        public bool IsActive { get; set; }
        public int? LCMSId { get; set; }
        public bool IsAssessment { get; set; }
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int? CompletionPeriodDays { get; set; }
        public bool IsNegativeMarking { get; set; }
    }

    public class APIModuleInput
    {
        public int? Id { get; set; }
        [MinLength(1), MaxLength(150)]
        [Required]
        [CSVInjection]       
        public string Name { get; set; }
        [MaxLength(30)]
        [Required]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.H5P, CommonValidation.SCORM, CommonValidation.cmi5, CommonValidation.xAPI, CommonValidation.Document, CommonValidation.Video, CommonValidation.Kpoint, CommonValidation.Audio, CommonValidation.YouTube, CommonValidation.externalLink, CommonValidation.vilt, CommonValidation.Classroom, CommonValidation.Assessment, CommonValidation.Feedback, CommonValidation.AR, CommonValidation.VR, CommonValidation.memo, CommonValidation.Authoring, CommonValidation.Assignment, CommonValidation.nonSCORM, CommonValidation.vr })]
        public string ModuleType { get; set; }

        [MaxLength(30)]
        [Required]
       public string CourseType { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [MaxLength(500)]
        public string Metadata { get; set; }
        [Range(0, 100, ErrorMessage = "Credit point must be between 0 to 100")]
        public int? CreditPoints { get; set; }
        public int? LCMSId { get; set; }
        public bool? IsMultilingual { get; set; }
        public int[] MultilingualLCMSId { get; set; }
    }

    public class APIModuleData
    {
        public int? Id { get; set; }
        [MinLength(1), MaxLength(150)]
        [Required]
        [CSVInjection]
        public string Name { get; set; }
        [MaxLength(30)]
        public string ModuleType { get; set; }

        [MaxLength(30)]
        [Required]
        public string CourseType { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
       
        public int? CreditPoints { get; set; }
        public int? LCMSId { get; set; }
        public bool? IsMultilingual { get; set; }
        public int[] MultilingualLCMSId { get; set; }
        public string UserName { get; set; }      
        public int CreatedBy { get; set; }
        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
        public bool UserCreated { get; set; }


    }
    public class APITotalModuleData
    {
      public List<APIModuleData>Data { get; set; }
        public int TotalRecords { get; set; }
    }


    }
