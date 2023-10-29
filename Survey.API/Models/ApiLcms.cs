using System.ComponentModel.DataAnnotations;

namespace Survey.API.APIModel
{
    public class ApiLcms
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        public string Description { get; set; }
        [MaxLength(100)]
        [Required]
        public string ContentType { get; set; }
        public string ZipPath { get; set; }
        public string Path { get; set; }
        [MaxLength(50)]
        public string Version { get; set; }
        [Required]
        public string MetaData { get; set; }
        [MaxLength(200)]
        public string OriginalFileName { get; set; }
        [MaxLength(200)]
        public string InternalName { get; set; }
        [MaxLength(200)]
        public string MimeType { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
        public string Duration { get; set; }
        public string IsBuiltInAssesment { get; set; }
        public string ThumbnailPath { get; set; }
        public string IsMobileCompatible { get; set; }
        [MaxLength(15)]
        public string YoutubeVideoId { get; set; }
        public int? AssessmentSheetConfigID { get; set; }
        public int? FeedbackSheetConfigID { get; set; }
        public bool IsActive { get; set; }
        public bool? IsNested { get; set; }
    }
}
