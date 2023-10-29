using Assessment.API.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Model
{

    [Table("LCMS", Schema = "Course")]
    public class LCMS : BaseModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [MaxLength(50)]
        [Required]
        public string? ContentType { get; set; }
        [MaxLength(300)]
        public string? ZipPath { get; set; }
        [MaxLength(2000)]
        public string? Path { get; set; }
        [MaxLength(1000)]
        public string? Version { get; set; }
        [Required]
        [MaxLength(500)]
        public string? MetaData { get; set; }
        [MaxLength(200)]
        public string? OriginalFileName { get; set; }
        [MaxLength(200)]
        public string? InternalName { get; set; }
        [MaxLength(200)]
        public string? MimeType { get; set; }
        [MaxLength(50)]
        public string? Language { get; set; }
        public float Duration { get; set; }
        public bool IsBuiltInAssesment { get; set; }
        [MaxLength(300)]
        public string? ThumbnailPath { get; set; }
        public bool IsMobileCompatible { get; set; }
        [MaxLength(20)]
        public string? YoutubeVideoId { get; set; }
        public int? AssessmentSheetConfigID { get; set; }
        public int? FeedbackSheetConfigID { get; set; }

        public string? LaunchData { get; set; }

        public string? ActivityID { get; set; }

        public bool IsEmbed { get; set; }
        [DefaultValue("false")]
        public bool Ismodulecreate { get; set; }

        [DefaultValue("false")]
        public bool? IsNested { get; set; }
        public string? ExternalLCMSId { get; set; }
        public bool? IsExternalContent { get; set; }
        public string? SubContentType { get; set; }

    }
}
