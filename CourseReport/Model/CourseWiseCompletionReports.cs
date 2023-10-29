using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CourseReport.API.Model;
namespace CourseReport.API.Model
{

    public class CourseWiseCompletionReports
    {
        public List<PostCourseWiseCompletionReport> ReportList { get; set; }
        public int Count { get; set; }

    }
}
