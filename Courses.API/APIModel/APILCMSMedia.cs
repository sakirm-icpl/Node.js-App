using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APILCMSMedia
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ThumbnailPath { get; set; }
        public string Version { get; set; }
        public bool IsBuiltInAssesment { get; set; }
        public string OriginalFileName { get; set; }
        public bool IsMobileCompatible { get; set; }
        public string MetaData { get; set; }
        public string ContentType { get; set; }
        public string YoutubeVideoId { get; set; }
        public int? AssessmentSheetConfigID { get; set; }
        public int? FeedbackSheetConfigID { get; set; }
        public double Duration { get; set; }
        public string Language { get; set; }
    }
}
