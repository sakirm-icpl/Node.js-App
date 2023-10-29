namespace Assessment.API.APIModel
{
    public class APIGradingRules
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public int ModelId { get; set; }
        public string GradingRuleID { get; set; }
        public string ScorePercentage { get; set; }
        public string Grade { get; set; }
        public bool IsDeleted { get; set; }
    }
}
