using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model.ILT
{
    [Table("UserTeams", Schema = "User")]
    public class UserTeams
    {
        public int Id { get; set; }
        public string TeamCode { get; set; }
        public string TeamName { get; set; }
        public string AboutTeam { get; set; }
        public bool TeamStatus { get; set; }
        public bool IsDeleted { get; set; }
        public int? NumberOfRules { get; set; }
        public int? NumberofMembers { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
