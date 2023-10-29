namespace Assessment.API.APIModel
{
    public class APIModuleLevelAssessment
    {
        public int Id { get; set; }
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
}
