using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;

namespace Publication.API.Models
{
    [Table("UserMaster", Schema = "User")]
    public class UserMaster
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string UserId { get; set; }
        [MaxLength(1000)]
        public string EmailId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Password { get; set; }
        [Required]
        [MaxLength(200)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(10)]
        public string UserRole { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? AccountExpiryDate { get; set; }
        public bool Lock { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RowGuid { get; set; }
        public enum Exists
        {
            [Description("Employee Code already exist.")]
            EmployeeCodeExist,
            [Description("User Id already exist.")]
            UserIdExist,
            [Description("Mobile already exist.")]
            MobileExist,
            [Description("Email Id already exist.")]
            EmailIdExist,
            [Description("User name already exist.")]
            UserNameExist,
            [Description("Activation Email exist.")]
            ActivationSentToExist,
            [Description("Activation Email does not exist.")]
            ActivationSentToNotExist,
            No,
            [Description("Aadhar Number already exist.")]
            AadharExist,
        }
    }

    [Table("UserMasterDetails", Schema = "User")]
    public class UserMasterDetails
    {
        public int Id { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [MaxLength(50)]
        public string SerialNumber { get; set; }
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
        public int? ConfigurationColumn13 { get; set; }
        public int? ConfigurationColumn14 { get; set; }
        public int? ConfigurationColumn15 { get; set; }
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
        [MaxLength(100)]
        public string UserSubType { get; set; }
        public int? JobRoleId { get; set; }
        public DateTime? DateIntoRole { get; set; }
        public string AadharNumber { get; set; }
        public string FHName { get; set; }
        public string AadhaarPath { get; set; }
        public int? BuddyTrainerId { get; set; }
        public int? MentorId { get; set; }
        public int? HRBPId { get; set; }
        public DateTime? AccountDeactivationDate { get; set; }
    }

    [Table("Location", Schema = "User")]
    public class Location
        {
            public int Id { get; set; }
            [MaxLength(200)]
            public string Name { get; set; }
            public DateTime CreatedDate { get; set; }
            public int IsDeleted { get; set; }
            [MaxLength(200)]
            public string NameEncrypted { get; set; }
        }

    [Table("Area", Schema = "User")]
    public class Area
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }
    }

    [Table("Business", Schema = "User")]
    public class Business
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(50)]
        public string Theme { get; set; }
        [MaxLength(100)]
        public string LogoName { get; set; }

    }
    public class GetAreaListandCount
    {
        public List<Area> GetAreasListandCount { get; set; }
        public int Count { get; set; }


    }

}
