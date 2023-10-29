using System.ComponentModel;

namespace Assessment.API.APIModel
{
    public class APIAssessmentConfiguration
    {
        public int PassingPercentage { get; set; }
        public int? assessmentaheetconfigid { get; set; }
        public int MaximumNoOfAttempts { get; set; }
        public int Durations { get; set; }
        public string? Description { get; set; }
        public string? MetaData { get; set; }
        public string? Name { get; set; }
        public bool? IsFixed { get; set; }
        public int? NoOfQuestionsToShow { get; set; }
        public bool IsMemo { get; set; }
        public APIQuestionConfiguration[]? aPIQuestionConfiguration { get; set; }
        public bool? IsNegativeMarking { get; set; }
        public bool? IsRandomQuestion { get; set; }
        public int? NegativeMarkingPercentage { get; set; }
        public bool? IsEvaluationBySME { get; set; }
        [DefaultValue("false")]
        public bool Ismodulecreate { get; set; }
    }

    public class APIQuestionConfiguration
    {
        public int QuestionID { get; set; }
        public int? SequenceNumber { get; set; }
    }
    public class APIAssessmentConfigurationParameters
    {
        public int? Id { get; set; }
        public string? Attribute { get; set; }
        public string? Code { get; set; }
        public string? Value { get; set; }
    }
}