using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    [Table("UserMasterDetails", Schema = "User")]
    public class UserMasterDetails
    {
        public int Id { get; set; }
        public int UserMasterId { get; set; }
        public string MobileNumber { get; set; }
        public string? ReportsTo { get; set; }
        public int? ConfigurationColumn2 { get; set; }
        public int? ConfigurationColumn12 { get; set; }
        public int? ConfigurationColumn1 { get; set; }
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
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? AreaId { get; set; }
        public int? LocationId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfJoining { get; set; }       
        public string TimeZone { get; set; }
    }

  
}
