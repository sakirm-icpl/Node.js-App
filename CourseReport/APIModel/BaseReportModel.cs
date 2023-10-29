using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.APIModel
{
    public class BaseReportModel
    {


        [Required]
        public int StartIndex { get; set; }
        [Required]
        public int PageSize { get; set; }
        public string SortOrder { get; set; }
    }
}
