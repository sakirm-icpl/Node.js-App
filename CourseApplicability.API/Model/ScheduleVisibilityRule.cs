using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseApplicability.API.Model
{
    [Table("ScheduleVisibilityRule", Schema = "Course")]
    public class ScheduleVisibilityRule : BaseModel
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
        public int? ConfigurationColumn13 { get; set; }
        public int? ConfigurationColumn14 { get; set; }
        public int? ConfigurationColumn15 { get; set; }

        [MaxLength(10)]
        public string ConditionForRules { get; set; }
        public int? ScheduleId { get; set; }

        public int? UserTeamId { get; set; }


        public string RowGUID { get; set; }
        public DateTime? StartDateOfJoining { get; set; }
        public DateTime? EndDateOfJoining { get; set; }
    }
}

