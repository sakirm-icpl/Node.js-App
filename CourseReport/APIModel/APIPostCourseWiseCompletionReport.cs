using System;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.APIModel
{
    public class APIPostCourseWiseCompletionReport
    {
        
        public int CourseId { get; set; }
        
        public string CourseName { get; set; }
      
    }
    public class APIUpdateCourseWiseCompletionReport
    {
        public int Id { get; set; }
        public string ExportPath { get; set; }
    }
}
