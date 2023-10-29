using CourseReport.API.Helper.MetaData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseReport.API.APIModel
{
    public class APICourseExportReport
    {
        public int Id { get; set; }
        [Required]
        public string ConfiguredColumnName { get; set; }
        [Required]
        public string ChangedColumnName { get; set; }


    }
    [Table("UserSettings", Schema = "User")]
    public class UserSettings
    {
        public int Id { get; set; }
        [Required]
        public string ConfiguredColumnName { get; set; }
        [Required]
        public string ChangedColumnName { get; set; }
        public bool IsConfigured { get; set; }
        [Required]
        public bool IsMandatory { get; set; }
        [Required]
        public bool IsShowInReport { get; set; }
        public bool IsShowInAnalyticalDashboard { get; set; }
        [MaxLength(10)]
        public string RoleCode { get; set; }
        [MaxLength(200)]
        public string FieldType { get; set; }
        [MaxLength(20)]
        public string DevCode { get; set; }

       // public bool IsBuddyConfiguredOn { get; set; } = false;
        public int IsDeleted { get; set; }


    }
}
