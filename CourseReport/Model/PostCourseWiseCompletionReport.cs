using System;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.Model
{
    public class PostCourseWiseCompletionReport
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }

        public string Username { get; set; }

        public DateTime RequestedDate { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string ExportPath { get; set; }
       
    }
}
