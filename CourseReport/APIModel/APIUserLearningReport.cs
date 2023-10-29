namespace CourseReport.API.APIModel
{
    public class APIUserLearningReport
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public string CourseStartDate { get; set; }
        public string CourseCompletionDate { get; set; }
        private string _status;
        public string Status
        {
            get => this._status;
            set => this._status = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }
        public string? RestaurantId { get; set; }
        public string? CurrentRestaurant { get; set; }

        public string? Region { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? ClusterManager { get; set; }

        public string? AreaManager { get; set; }

        public string? TotalMarks { get; set; }
        public string? MarksObtained { get; set; }
        public string? AssessmentPercentage { get; set; }
        public string CourseDuration { get; set; }
        public int TotalRecordCount { get; set; }
    }
}