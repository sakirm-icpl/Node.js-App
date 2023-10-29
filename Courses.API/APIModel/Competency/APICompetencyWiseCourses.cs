namespace Courses.API.APIModel.Competency
{
    public class APICompetencyWiseCourses
    {
        public int? Id { get; set; }
        public int? CourseCategoryId { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int CompetencyCategoryId { get; set; }
        public int CompetencyId { get; set; }
        public int CompetencyLevelId { get; set; }
        public string CourseCategory { get; set; }
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string Module { get; set; }
        public string CompetencyCategory { get; set; }
        public string Competency { get; set; }
        public string CompetencyLevel { get; set; }
        public string TrainingType { get; set; }
        public string Modules { get; set; }
        public int Days { get; set; }
    }
}
