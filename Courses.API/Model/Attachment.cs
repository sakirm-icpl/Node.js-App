using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class Attachment : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string OriginalFileName { get; set; }
        [MaxLength(100)]
        public string InternalName { get; set; }
        [MaxLength(100)]
        public string MimeType { get; set; }
        [MaxLength(1000)]
        public string AttachmentPath { get; set; }
        public string FileContents { get; set; }
    }
}
