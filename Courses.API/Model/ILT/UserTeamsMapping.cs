using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model.ILT
{
    [Table("UserTeamsMapping", Schema = "User")]
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
}
