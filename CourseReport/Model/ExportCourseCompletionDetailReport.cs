using System;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.Model
{
    public class ExportCourseCompletionDetailReport
    {
        public int Id { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public int? CourseId { get; set; }

        public string CourseName { get; set; }

        public string ExportPath { get; set; }

        public bool? IsDownloaded{ get; set; }



    }
}
