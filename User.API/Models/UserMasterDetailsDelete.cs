using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class UserMasterDetailsDelete
    {
        public int Id { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [MaxLength(50)]
        public string SerialNumber { get; set; }
        [Required]
        [MaxLength(1000)]
        public string MobileNumber { get; set; }
        [MaxLength(10)]
        public string UserType { get; set; }
        [MaxLength(10)]
        public string Gender { get; set; }
        [MaxLength(50)]
        public string TimeZone { get; set; }
        [MaxLength(50)]
        public string Currency { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
        public string ProfilePicture { get; set; }

        [MaxLength(1000)]
        public string ReportsTo { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? AreaId { get; set; }
        public int? LocationId { get; set; }
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
        public bool IsPasswordModified { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? AccountCreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public DateTime? PasswordModifiedDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? LastLoggedInDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool TermsCondintionsAccepted { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public bool Degreed { get; set; }
        public bool AppearOnLeaderboard { get; set; }
        public int? HouseId { get; set; }
        public int UserMasterDetail_Id { get; set; }
    }
}
