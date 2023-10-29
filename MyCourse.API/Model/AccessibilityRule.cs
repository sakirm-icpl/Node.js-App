using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class AccessibilityRule : BaseModel
    {
        public int Id { get; set; }
        public int? UserID { get; set; }
        [MaxLength(100)]
        public string EmailID { get; set; }
        [MaxLength(25)]
        public string MobileNumber { get; set; }
        [MaxLength(100)]
        public string ReportsTo { get; set; }
        [MaxLength(10)]
        public string UserType { get; set; }
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
        [MaxLength(100)]
        public string RuleAnticipation { get; set; }
        public int? TargetPeriod { get; set; }
        [DefaultValue(false)]
        public bool IsCourseFee { get; set; }
        [MaxLength(10)]
        public string ConditionForRules { get; set; }
        public int? CourseId { get; set; }
        public int? CategoryId { get; set; }
        public int? GroupTemplateId { get; set; }
        public string Reason { get; set; }

        public string RowGUID { get; set; }

        public int? SubCategoryId { get; set; }
        public DateTime? StartDateOfJoining { get; set; }
        public DateTime? EndDateOfJoining { get; set; }
        public int? UserTeamId { get; set; }
        public DateTime? EdCast_due_date { get; set; }
        public DateTime? EdCast_assigned_date { get; set; }
    }

    public class UserGroup : BaseModel
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public int UserMasterId { get; set; }
        public string RowGUID { get; set; }
    }

    public class AccebilityRuleUserGroup : BaseModel
    {
        public int Id { get; set; }
        public int UserGroupId { get; set; }
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
