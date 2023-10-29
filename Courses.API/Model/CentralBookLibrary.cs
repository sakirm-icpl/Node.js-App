using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class CentralBookLibrary : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string BookId { get; set; }
        [MaxLength(100)]
        [Required]
        public string BookName { get; set; }
        [MaxLength(200)]
        [Required]
        public string Author { get; set; }
        [MaxLength(50)]
        [Required]
        public string Language { get; set; }
        public int CategoryId { get; set; }
        [MaxLength(200)]
        public string Category { get; set; }
        public bool AccessibleToAllUser { get; set; }
        [MaxLength(50)]
        public string KeywordForSearch { get; set; }
        [MaxLength(100)]
        public string AccessibilityRuleId { get; set; }
        [Required]
        [MaxLength(50)]
        public string ConfigurationColumn { get; set; }
        [Required]
        [MaxLength(50)]
        public string ConfigurationValue { get; set; }
        [MaxLength(2000)]
        public string BookFile { get; set; }
        [MaxLength(2000)]
        public string BookImage { get; set; }
    }
}
