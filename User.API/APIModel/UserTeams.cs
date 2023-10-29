using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using User.API.Models;
using User.API.Validation;

namespace User.API.APIModel
{
    public class UserTeams
    {
        public int Id { get; set; }
        public string TeamCode { get; set; }
        public string TeamName { get; set; }
        public string AboutTeam { get; set; }
        public bool TeamStatus { get; set; }
        public bool IsDeleted { get; set; }
        public bool? EmailNotification { get; set; }
        public int? NumberOfRules { get; set; }
        public int? NumberofMembers { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class UserTeamsPage
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string ColumnName { get; set; }
        public string Search { get; set; }
    }

    public class APIUserTeamsType
    {
        public int Id { get; set; }
        public string TeamCode { get; set; }
        public string TeamName { get; set; }
    }
    public class MappingParameters
    {
        public int Id { get; set; }
        public string AccessibilityParameter1 { get; set; }
        public string AccessibilityParameter2 { get; set; }
        public string AccessibilityValue1 { get; set; }
        public int AccessibilityValueId1 { get; set; }
        public string AccessibilityValue2 { get; set; }
        public int AccessibilityValueId2 { get; set; }
        public string condition1 { get; set; }
        public int UserTeamsId { get; set; }
    }
    public class Mappingparameter
    {
        public int UserTeamId { get; set; }
        public string AccessibilityParameter { get; set; }
        public string ParameterValue { get; set; }
    }

    public class UserTeamsMapping
    {
        public int Id { get; set; }
        public int UserTeamId { get; set; }
        public string ConditionForRules { get; set; }
        public bool MappingStatus { get; set; }
        public bool? IsDeleted { get; set; }
        public int? UserID { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? EmailID { get; set; }
        public string? MobileNumber { get; set; }
        public int? Business { get; set; }
        public int? Group { get; set; }
        public int? Area { get; set; }
        public int? Location { get; set; }
        public DateTime? JoinedBefore { get; set; }
        public DateTime? JoinedAfter { get; set; }
        public DateTime? AgeLessThan { get; set; }
        public DateTime? AgeGreaterThan { get; set; }
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
        public int? ConfigurationColumn13 { get; set; }
        public int? ConfigurationColumn14 { get; set; }
        public int? ConfigurationColumn15 { get; set; }
        public int? ConfigurationColumn12 { get; set; }
    }

    public class Rules
    {
        public int Id { get; set; }
        public string AccessibilityParameter { get; set; }

        public string AccessibilityValue { get; set; }
        public string AccessibilityValue2 { get; set; }

        public string Condition { get; set; }
    }

    public class ConfiguredColumns
    {
        public string ConfiguredColumnName { get; set; }
        public string ChangedColumnName { get; set; }
    }
    public class APIGetRulesFormapping
    {
        public int userTeamsId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class UserTeamsImportPayload
    {
        public string Path { get; set; }
    }
    public class AccessibilityRuleRejected : CommonFields
    {
        public int? Id { get; set; }

        public string UserTeamCode { get; set; }

        public string UserName { get; set; }

        public string ErrorMessage { get; set; }
    }
    public class APIAccessibilityRuleFilePath
    {
        public string Path { get; set; }
        public int totalRecordInsert { get; set; }
        public int totalRecordRejected { get; set; }
        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
    }
    public class Title
    {
        [MaxLength(150)]
        public string Name { get; set; }
        
    }
    public class APIGetRulesUserTeam
    {
        public int userTeamId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class APIAccessibilityRules
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int UserTeamId { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue11 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId1 { get; set; }
        [MaxLength(5)]
        [CommonValidationAttribute(AllowValue = new string[] { "AND", "OR" })]
        public string Condition1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue22 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId2 { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class UserTeamApplicableUser
    {
        public static int UserTeamId { get; internal set; }
        public string UserID { get; internal set; }
        public string UserName { get; internal set; }
    }

    public class UserTeamCount
    {
        public string TeamCode { get; set; }
    }
    public class CourseUserTeam
    {
        public int? UserTeamId { get; set;}
        public int? CourseId { get; set; }
    }
}
