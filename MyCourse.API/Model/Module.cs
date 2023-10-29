using MyCourse.API.Helper;
using MyCourse.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class Module : BaseModel
    {
        public int Id { get; set; }
        [MinLength(1), MaxLength(300)]
        [Required]
        public string Name { get; set; }
        [MaxLength(30)]
        [Required]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.H5P, CommonValidation.SCORM, CommonValidation.cmi5, CommonValidation.xAPI, CommonValidation.Document, CommonValidation.Video, CommonValidation.Audio, CommonValidation.YouTube, CommonValidation.externalLink, CommonValidation.vilt, CommonValidation.Classroom, CommonValidation.Assessment, CommonValidation.Feedback, CommonValidation.AR, CommonValidation.VR, CommonValidation.memo, CommonValidation.Authoring, CommonValidation.Assignment , CommonValidation.nonSCORM,  CommonValidation.vr })]
        public string ModuleType { get; set; }

        [MaxLength(30)]
        [Required]
        public string CourseType { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [Range(0, 100, ErrorMessage = "Credit point must be between 0 to 100")]
        public int? CreditPoints { get; set; }
        public int? LCMSId { get; set; }

        public bool? IsMultilingual { get; set; }

    }
}
