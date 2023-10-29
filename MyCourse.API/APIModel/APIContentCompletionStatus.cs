using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.APIModel
{
    public class APIContentCompletionStatus
    {
        public int Id { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int ModuleId { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int CourseId { get; set; }
        [Required]
        public string Status { get; set; }
        public string Location { get; set; }
        public int? GroupId { get; set; }
        public bool? IsUserConsent { get; set; }
    }
    public class APIContentCompletionStatusForKpoint
    {
        public int ModuleId { get; set; }

        public int CourseId { get; set; }
        public string gccid { get; set; }
    }
    public class APIForKpointReport
    {
        public int CourseId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class KpointStatus
    {
        public int duration { get; set; }
        public int watch_duration { get; set; }
        public string id { get; set; }
    }
    public class List
    {
        public int total { get; set; }
        public int passing_score { get; set; }
        public int correct { get; set; }
        public int percentage { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public int qid { get; set; }
        public int attempts { get; set; }
    }
    public class Quizscore
    {
        public List[] list { get; set; }
        public int totalcount { get; set; }
    }
    public class KPointReport
    {
        public string last_accessed { get; set; }
        public string displayname { get; set; }
        public int view_count { get; set; }
        public string username { get; set;}
        public string accountno { get; set; }
        public string email { get; set; }
        public string viewed_duration { get; set; }
        public string percentage_viewed { get; set; }
        public string afterKey { get; set; }
        public Quizscore quizscore { get; set; }
    }
    public class KPointReportV2
    {
        public string CourseName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int view_count { get; set; }
        public string viewed_duration { get; set; }
        public string percentage_viewed { get; set; }
        public string title { get; set; }
        public int attempts { get; set; }
        public int percentage { get; set; }
    }
}
