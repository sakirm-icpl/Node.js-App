using Courses.API.Validations;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class LCMSAPI
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
        [Range(0, float.MaxValue, ErrorMessage = "Version must be a positive number")]
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
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number")]
        public string Duration { get; set; }
        public string IsBuiltInAssesment { get; set; }
        public string ThumbnailPath { get; set; }
        public string IsMobileCompatible { get; set; }
        [MaxLength(15)]
        public string YoutubeVideoId { get; set; }
        public int? AssessmentSheetConfigID { get; set; }
        public int? FeedbackSheetConfigID { get; set; }
        public bool IsActive { get; set; }
        public bool IsScorm { get; set; }
        public string StartPagePath { get; set; }
        [ScormTypeAttribute]
        public string ScormType { get; set; }
    
        public bool IsEmbed { get; set; }
        [DefaultValue("false")]
        public bool Ismodulecreate { get; set; }
        public bool? IsNested { get; set; }
        public string? ExternalLCMSId { get; set; }
        public string subContentType { get; set; }
    }

    public class ApiGetLCMSMedia
    {
        [Required]
        public int page { get; set; } = 1;
        [Required]
        public int pageSize { get; set; } = 10;       
        public bool? IsActive { get; set; }
        public string search { get; set; }
        public string metaData { get; set; }
        public bool showAllData { get; set; }
    }

    public class LCMSData 
    {
        public int Id { get; set; }
       
        public string Name { get; set; }
       
        public string Path { get; set; }
        public string ThumbnailPath { get; set; }
       
        public string Version { get; set; }
        public string OriginalFileName { get; set; }
        public bool IsMobileCompatible { get; set; }
        public string MetaData { get; set; }
        public string ContentType { get; set; }
        public string YoutubeVideoId { get; set; }
        public int? AssessmentSheetConfigID { get; set; }
        public int? FeedbackSheetConfigID { get; set; }
        public string Language { get; set; }
        public float Duration { get; set; }
        public bool IsBuiltInAssesment { get; set; }
        public string UserName { get; set; }        
        public int CreatedBy { get; set; }
        public string? ExternalLCMSId { get; set; }
        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
        public bool UserCreated { get; set; }
        public bool? IsExternalContent { get; set; }
        public string? SubContentType { get; set; }
    }

    public class APITotalLCMsView
    {
        public List<LCMSData> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
