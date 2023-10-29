using System;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class CompetencyJobRole : BaseModel
    {
        public int? Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string RoleColumn1 { get; set; }
        public int? RoleColumn1value { get; set; }
        public string RoleColumn2 { get; set; }
        public int? RoleColumn2value { get; set; }
        public string Description { get; set; }
        public int NumberOfPositions { get; set; }
        public int CourseId { get; set; }
    }
    public class CompetencyJdUpload
    {
        public int? Id { get; set; }

        public int? CompetencyJobRoleId { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
        public int FileVersion { get; set; }

    }

    public class APICompetencyJdUpload
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public string UserName { get; set; }
        public string FilePath { get; set; }
    }

    public class APIJdUpload
    {
        public int Page { get; set; }
        [RegularExpression("^[0-9]*", ErrorMessage = "Invalid PageSize")]
        public int PageSize { get; set; }

        public string Search { get; set; }
        public string ColumnName { get; set; }
    }
}

