using System.Collections.Generic;

namespace Assessment.API.APIModel
{
    public class APIGetHeaderDetails
    {
        public int Id { get; set; }
        public int HeaderID { get; set; }
        public string CourseTitle { get; set; }
        public int CourseID { get; set; }
        public string Question { get; set; }
        public int QuestionId { get; set; }
        public string Section { get; set; }
        public string Time { get; set; }
        public int Marks { get; set; }
        public bool Status { get; set; }
        public string DifficultyLevel { get; set; }
        public int OptionsCount { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class APIGetQuestionMaster
    {
        public int Id { get; set; }
        public string OptionType { get; set; }
        public string Question { get; set; }
        public int QuestionId { get; set; }
        public string Section { get; set; }
        public int Time { get; set; }
        public int Marks { get; set; }
        public bool Status { get; set; }
        public string DifficultyLevel { get; set; }
        public string Metadata { get; set; }
        public int OptionsCount { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMemoQuestion { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Option5 { get; set; }
        public string  UserName { get; set; }
    }


    public class APIAssessmentMaster
    {
        public int ConfigurationId { get; set; }
        public string CourseTitle { get; set; }
        public string ModuleTitle { get; set; }
        public int NoofQuestion { get; set; }
        public int PassingMarks { get; set; }
        public float Percentage { get; set; }
        public int NoOfAttempts { get; set; }
        public int DurationInMins { get; set; }
        public string Status { get; set; }
        public int TotalMarks { get; set; }
        public double? ObtainedMarks { get; set; }
        public float PassingPercentage { get; set; }
        public string thumbnailPath { get; set; }
        public string AssessmentStatus { get; set; }
        public string Result { get; set; }
        public int TotalNumberOfAttempts { get; set; }
        public bool? IsNegativeMarking { get; set; }
        public float? NegativeMarkingPercentage { get; set; }
        public bool? IsEvaluationBySME { get; set; }
        public bool? IsReviewedBySME { get; set; }
        public int NoOfQuestionAttempted { get; set; }
        public int CorrectAnswerCount { get; set; }
        public int InCorrectAnswerCount { get; set; }
        public string CourseCode { get; set; }
        public int postassessmentId { get; set; }
    }

    public class APIAssessmentHeaderDetails
    {
        public int Durations { get; set; }
        public float PassingPercentage { get; set; }
        public int TotalAttempt { get; set; }
        public string CourseTitle { get; set; }
        public string ModuleTitle { get; set; }
        public int QMarks { get; set; }
        public int NoOfQuestionsToShow { get; set; }
        public bool IsFixed { get; set; }
        public string ThumbnailPath { get; set; }       
        public bool? IsNegativeMarking { get; set; }
        public float? NegativeMarkingPercentage { get; set; }
        public bool? IsEvaluationBySME { get; set; }
        public string Section { get; set; }
        public int QuestionId { get; set; }
        public bool? IsCorrectAnswer { get; set; }
        public int? OptionId { get; set; }
        public string SelectedAnswer { get; set; }
        public string Code { get; set; }
        public string CourseType { get; set; }
        public int TotalModules { get; set; }
    }


    public class APIGetAssessmentQuestion
    {
        public int Id { get; set; }
        public string OptionType { get; set; }
        public string Question { get; set; }
        public int QuestionId { get; set; }
        public string Section { get; set; }       
        public int Marks { get; set; }
        public bool Status { get; set; }
        public string DifficultyLevel { get; set; }
        public string Metadata { get; set; }
        public int OptionsCount { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMemoQuestion { get; set; }
        public string UserName { get; set; }
        public int CreatedBy { get; set; }
        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
        public bool UserCreated { get; set; }
        public string CourseTitle { get; set; }

    }

    public class APITotalAssessmentQuestion
    {
       public List<APIGetAssessmentQuestion> data { get; set; }
        public int TotalRecords { get; set; }
    }
    }
