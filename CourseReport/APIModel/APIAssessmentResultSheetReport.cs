using System.Collections.Generic;

namespace CourseReport.API.APIModel
{
    public class APIAssessmentResultSheetReport
    {
        public string id { get; set; }

        public string UserName { get; set; }
        public string UserId { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public string NoOfAttempts { get; set; }
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public string AssessmentResult { get; set; }
        public string AssessmentPercentage { get; set; }
        public string AssessmentDate { get; set; }
        public string AssessmentStartTime { get; set; }
        public string AssessmentEndTime { get; set; }
        public string AssessmentTimeDiffrance { get; set; }
        public string OrgCode { get; set; }
        public string ModuleName { get; set; }
        public int TotalRecordCount { get; set; }
        public string WorkLocation { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public string Region { get; set; }
        public string EmployeeState { get; set; }
        public string DateofJoining { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        
        public string CircleZone { get; set; }
        public string ReportingManager { get; set; }
        public string Status { get; set; }
        public string Level { get; set; }

        //Lenexis
        public string? RestaurantId { get; set; }
        public string? CurrentRestaurant { get; set; }

        //public string? Region { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? ClusterManager { get; set; }

        public string? AreaManager { get; set; }
        public string IsAdaptiveAssessment { get; set; }
        //Lenexis
    }


    public class APIRecommondedTrainingReport
    {
        public string id { get; set; }

        public string UserName { get; set; }
        public string UserId { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public string ModuleName { get; set; }
       
        public string JobRole { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Level { get; set; }
        public string Status { get; set; }
        public string TrainingProgram { get; set; }
        public string Category { get; set; }
        
    }


    public class APIUserAssessmentSheet
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string QuestionText { get; set; }
        public string SelectedAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public string ObtainedMarks { get; set; }
        public string TotalMarks { get; set; }
        public string NoOfAttempts { get; set; }
        public string ModuleName { get; set; }
        public string id { get; set; }
        public string? RestaurantId { get; set; }
        public string? CurrentRestaurant { get; set; }

        public string? Region { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? ClusterManager { get; set; }

        public string? AreaManager { get; set; }
        public int TotalRecordCount { get; set; }

    }

    public class APIManagerEvaluation
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string QuestionText { get; set; }
        public string SelectedAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public string ObtainedMarks { get; set; }
        public string TotalMarks { get; set; }
        public string NoOfAttempts { get; set; }
        public string ModuleName { get; set; }
        public string id { get; set; }
        public string? RestaurantId { get; set; }
        public string? CurrentRestaurant { get; set; }

        public string? Region { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? ClusterManager { get; set; }

        public string? AreaManager { get; set; }
        public string? AssessedBy { get; set; }
        public int TotalRecordCount { get; set; }

    }

    public class APIManagerEvaluationData
    {
        public int TotalRecordCount { get; set; }
        public List<APIManagerEvaluation> data { get; set; }
    }
}
