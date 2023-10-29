namespace Assessment.API.APIModel
{
    public class APIMargeModuleAssessment
    {
        public APIModuleAssessment aPIModuleAssessment { get; set; }
        public APIModuleQuestionAssessment[] aPIModuleQuestionAssessment { get; set; }
    }

    public class APIModuleAssessment
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int ModelId { get; set; }
        public int NumberOfQuestion { get; set; }
        public int NoObjectiveQues { get; set; }
        public int NoSubjectiveQues { get; set; }
        public int TotalMarkAllQuestion { get; set; }
        public int TotalMarkSubjectiveQues { get; set; }
        public int TotalMarkObjectiveQues { get; set; }
        public int NumberOfSubjectiveask { get; set; }
        public int NumberOfObjectiveask { get; set; }
        public bool RandomizeQueSequence { get; set; }
        public bool AdaptiveAssessment { get; set; }
        public bool NegativeMarking { get; set; }
        public bool AllowSkipQuestion { get; set; }
        public int PassingScore { get; set; }
        public int NumberOfPermissibleAttempts { get; set; }

    }

    public class APIModuleQuestionAssessment
    {
        public int? Id { get; set; }
        public int ModuleLevelID { get; set; }
        public string Section { get; set; }
        public int QuestionNumber { get; set; }
        public string instructionLearner { get; set; }
        public string Question { get; set; }
        public bool AnswerImages { get; set; }
        public string AnswerOption1 { get; set; }
        public string AnswerOption2 { get; set; }
        public string AnswerOption3 { get; set; }
        public string AnswerOption4 { get; set; }
        public string AnswerOption5 { get; set; }
        public string UploadImage1 { get; set; }
        public string UploadImage2 { get; set; }
        public string UploadImage3 { get; set; }
        public string UploadImage4 { get; set; }
        public string UploadImage5 { get; set; }
        public string CorrectAnswer { get; set; }
        public bool RandamizeAnswer { get; set; }
        public int SubjectiveQuestionLimit { get; set; }
        public string ModelAnswer { get; set; }
        public string DifficultyLevel { get; set; }
        public int permissionTime { get; set; }
        public int Marks { get; set; }
        public bool status { get; set; }

    }


    public class APIQuestionAssessment
    {
        public int Id { get; set; }
        public int AssessmentHeaderID { get; set; }
        public string Section { get; set; }
        public string LearnerInstruction { get; set; }
        public int SubjectiveAnswerLimit { get; set; }
        public string QuestionText { get; set; }
        public string DifficultyLevel { get; set; }
        public string Time { get; set; }
        public string ModelAnswer { get; set; }
        public string MediaFile { get; set; }
        public string AnswerAsImages { get; set; }
        public int Marks { get; set; }
        public bool Status { get; set; }
        public bool IsCorrectAnswer1 { get; set; }
        public bool IsCorrectAnswer2 { get; set; }
        public bool IsCorrectAnswer3 { get; set; }
        public bool IsCorrectAnswer4 { get; set; }
        public bool IsCorrectAnswer5 { get; set; }
        public string AnswerOption1 { get; set; }
        public string AnswerOption2 { get; set; }
        public string AnswerOption3 { get; set; }
        public string AnswerOption4 { get; set; }
        public string AnswerOption5 { get; set; }
        public string UploadImage1 { get; set; }
        public string UploadImage2 { get; set; }
        public string UploadImage3 { get; set; }
        public string UploadImage4 { get; set; }
        public string UploadImage5 { get; set; }
    }
}
