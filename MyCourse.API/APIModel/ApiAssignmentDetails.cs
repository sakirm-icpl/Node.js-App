using System;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.APIModel
{
    public class ApiAssignmentDetails
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int AssignmentId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int UserId { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string TextAnswer { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }

    }


    public class ApiAssignmentInfo
    {
        public int AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int UserId { get; set; }
        public string UserNameId { get; set; } 
        public string UserName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string TextAnswer { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string ModifiedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class SearchAssignmentDetails
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public int? AssignmentId { get; set; }
        public int? CourseId { get; set; }
        public int? UserId { get; set; }
        public string Status { get; set; }
        public string ColumnName { get; set; }
        public string SearchText { get; set; }
    }

    public class APICourseTypewithCount
    {
        public string CourseType { get; set; }
        public int Count { get; set; }
        public int WebinarCount { get; set; }
        public int ElearningCount { get; set; }
        public int BlendedCount { get; set; }
        public int ClassroomCount { get; set; }
        public int CertificationCount { get; set; }
    }

    public class APIAssignmentFilePath
    {
        public string Path { get; set; }

    }

    public class GetUserInfo
    {
        public int Id { get; set; }

        public string UserName { get; set; }
    }


    public class APICourseTypeCount
    {
        public int WebinarCount { get; set; }
        public int ElearningCount { get; set; }
        public int BlendedCount { get; set; }
        public int ClassroomCount { get; set; }

        public int AssessmentCount { get; set; }
    }
}
