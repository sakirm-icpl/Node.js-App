namespace Assessment.API.APIModel
{
    public class APIPreTrainingAssessment
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int ModelId { get; set; }
        public int NumberOfQuestion { get; set; }
        public int TotalMark { get; set; }
        public int NumberOfQuestionask { get; set; }
        public bool RandomizeQueSequence { get; set; }
        public bool SkipQuestionTryOption { get; set; }
        public string exemptScoreModel { get; set; }

    }

    public class APIPreQuestionAssessment
    {
        public int? Id { get; set; }
        public int QuestionNumber { get; set; }
        public int PreAssessmentID { get; set; }
        public string instructionLearner { get; set; }
        public string Question { get; set; }
        public string UploadMdeiaFile { get; set; }
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
        public string permissionTime { get; set; }
        public string Marks { get; set; }
        public bool status { get; set; }
    }

    public class APIMergeModel
    {
        public APIPreTrainingAssessment aPIPreTrainingAssessment { get; set; }
        public APIPreQuestionAssessment[] apiQuestionMaster { get; set; }
    }

}
