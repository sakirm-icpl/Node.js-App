using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class SurveyManagementAccessibilityRule : CommonFields
    {
        public int Id { get; set; }
        public int SurveyManagementId { get; set; }
        public int? Business { get; set; }
        public int? Group { get; set; }
        public int? Area { get; set; }
        public int? Location { get; set; }
        public int? ConfigurationColumn1 { get; set; }
        public int? ConfigurationColumn2 { get; set; }
        public int? ConfigurationColumn3 { get; set; }
        public int? ConfigurationColumn4 { get; set; }
        public int? ConfigurationColumn5 { get; set; }
        public int? ConfigurationColumn6 { get; set; }
        public int? ConfigurationColumn7 { get; set; }
        public int? ConfigurationColumn8 { get; set; }
        public int? ConfigurationColumn9 { get; set; }
        public int? ConfigurationColumn10 { get; set; }
        public int? ConfigurationColumn11 { get; set; }
        public int? ConfigurationColumn12 { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public int? UserId { get; set; }
        public string Value { get; set; }
        public int? GroupTemplateId { get; set; }
        [MaxLength(10)]
        public string ConditionForRules { get; set; }
        [MaxLength(40)]
        public string RowGuid { get; set; }
    }
}
