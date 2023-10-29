using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Model.Assessment
{
    [Table("AssessmentPhotos", Schema = "Course")]

    public class APIassessmentPhotos
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int PostAssessmentId { get; set; }
        public string? ImageData { get; set; }
    }
    public class CameraPhotos
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int PostAssessmentId { get; set; }
        public string? PhotoPath { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class CameraEvaluation
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int PostAssessmentId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class GetAPIassessmentPhotos
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int PostAssessmentId { get; set; }
        public string? Userid { get; set; }
    }
    public class photodate
    {
        public string? Photos { get; set; }
        public DateTime date { get; set; }
    }
    public class CameraPhotosResponse
    {
        public string? CourseName { get; set; }
        public string? ModuleName { get; set; }
        public List<photodate>? photodates { get; set; }
    }
    public class CameraPhotosForCourseEvaluation
    {
        public string? CourseName { get; set; }
        public int CourseId { get; set; }
        public string? ModuleName { get; set; }
        public int ModuleId { get; set; }
        public int PostAssessmentId { get; set; }
        public string? Status { get; set; }
    }

    public class APITotalCourseForEvaluation
    {
        public List<CameraPhotosForCourseEvaluation>? data { get; set; }
        public int TotalRecords { get; set; }

    }
    public class APIEvaulationPayload
    {
        public string? UserId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class CameraPhotosStatusForCourseEvaluation
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int PostAssessmentId { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
    }
}
