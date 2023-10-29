using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel.Competency
{
    public class APIRolewiseCoursesMapping 
    {
        public int Id { get; set; }
        [Required]
        public int JobRoleId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int ApplicableFromDays { get; set; }
       
    }

    public class APIRolewiseCoursesMappingDetails
    {
        public int Id { get; set; }
        [Required]
        public int JobRoleId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int ApplicableFromDays { get; set; }
        public string JobRoleName { get; set; }
        public string CourseName { get; set; }
        public bool Active { get; set; }

    }


    #region bulk import rolewise course

    public class APIRolewiseCourseMappingImport
    {
        public string Role { get; set; }
        public string Course { get; set; }
        public string AssignFromDays { get; set; }


        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrorMessage { get; set; }

    }

    public class APIRolewiseCourseMappingImportColumns
    {
        public const string Role = "Role";
        public const string Course = "Course";
        public const string AssignFromDays = "AssignFromDays";
    }

    #endregion

}
