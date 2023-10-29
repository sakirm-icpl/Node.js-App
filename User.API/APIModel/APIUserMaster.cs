//======================================
// <copyright file="APIUserMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using User.API.Helper;
using User.API.Validation;

namespace User.API.APIModel
{
    public class APIUserMaster
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [MaxLength(50)]
        public string SerialNumber { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9 -._-]*", ErrorMessage = "Invalid User Id")]
        [CSVInjection]
        public string UserId { get; set; }
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        [CSVInjection]
        public string UserName { get; set; }
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        [CSVInjection]
        public string EmailId { get; set; }
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
        [CSVInjection]
        [MaxLength(15)]
        public string MobileNumber { get; set; }
        [Required]
        [MaxLength(10)]
        [CSVInjection]
        public string UserRole { get; set; }
        [MaxLength(10)]
        [CSVInjection]
        [CheckValidationAttribute(AllowValue = new string[] { Record.Internal, Record.External })]
        public string UserType { get; set; }
        [MaxLength(10)]
        [CSVInjection]
        [CheckValidationAttribute(AllowValue = new string[] { Record.Male, Record.Female, Record.Other })]
        public string Gender { get; set; }
        [MaxLength(50)]
        [CSVInjection]
        [CheckValidationAttribute(AllowValue = new string[] {
        TimeZones.IST  ,          TimeZones.DST  ,          TimeZones.U    ,          TimeZones.HST  ,          TimeZones.AKDT ,
        TimeZones.PDT  ,          TimeZones.PDT1 ,          TimeZones.UMST ,          TimeZones.MDT  ,          TimeZones.MDT1 ,          TimeZones.CAST ,          TimeZones.CDT  ,
        TimeZones.CDT1 ,          TimeZones.CCST ,          TimeZones.SPST ,          TimeZones.EDT  ,          TimeZones.UEDT ,          TimeZones.VST  ,          TimeZones.PST  ,
        TimeZones.ADT,            TimeZones.CBST   ,        TimeZones.SWST   ,        TimeZones.PSST   ,        TimeZones.NDT    ,        TimeZones.ESAST  ,        TimeZones.AST    ,
        TimeZones.SEST   ,        TimeZones.GDT    ,        TimeZones.MST    ,        TimeZones.BST    ,        TimeZones.U1     ,        TimeZones.MDT2   ,        TimeZones.ADT1   ,
        TimeZones.CVST   ,        TimeZones.MDT3   ,        TimeZones.CUT    ,        TimeZones.GDT1   ,        TimeZones.GST    ,        TimeZones.WEDT   ,        TimeZones.CEDT   ,
        TimeZones.RDT    ,        TimeZones.CEDT1  ,        TimeZones.WCAST  ,        TimeZones.NST    ,        TimeZones.GDT2   ,        TimeZones.MEDT   ,        TimeZones.EST    ,
        TimeZones.SDT    ,        TimeZones.EEDT   ,        TimeZones.SAST   ,        TimeZones.FDT    ,        TimeZones.TDT    ,        TimeZones.JDT    ,        TimeZones.LST    ,
        TimeZones.JST    ,        TimeZones.AST1   ,        TimeZones.KST    ,        TimeZones.AST2   ,        TimeZones.EAST   ,        TimeZones.IDT    ,        TimeZones.AST3   ,
        TimeZones.ADT2   ,        TimeZones.RST    ,        TimeZones.MST1   ,        TimeZones.GST1   ,        TimeZones.CST    ,        TimeZones.AST4   ,        TimeZones.WAST   ,
        TimeZones.PST1   ,        TimeZones.SLST   ,        TimeZones.NST1   ,        TimeZones.CAST1  ,        TimeZones.BST1   ,        TimeZones.EST1   ,        TimeZones.MST2   ,
        TimeZones.SAST1  ,        TimeZones.NCAST  ,        TimeZones.CST1   ,        TimeZones.NAST   ,        TimeZones.MPST   ,        TimeZones.WAST1  ,        TimeZones.TST    ,
        TimeZones.UST    ,        TimeZones.NAEST  ,        TimeZones.TST1   ,        TimeZones.KST1   ,        TimeZones.CAST2  ,        TimeZones.ACST   ,        TimeZones.EAST1  ,
        TimeZones.AEST   ,        TimeZones.WPST   ,        TimeZones.TST2   ,        TimeZones.YST    ,        TimeZones.CPST   ,        TimeZones.VST2   ,        TimeZones.NZST   ,
        TimeZones.U2     ,        TimeZones.FST    ,        TimeZones.MST3   ,        TimeZones.KDT    ,        TimeZones.TST3   ,        TimeZones.SST
        })]
        public string TimeZone { get; set; }
        [MaxLength(50)]
        [CSVInjection]

        [CheckValidationAttribute(AllowValue = new string[] { Record.USDollar, Record.CanadianDollar, Record.BritishPound, Record.Euro, Record.AustralianDollar, Record.IndianRupee, Record.UAEDirham, Record.RialOmani, Record.SingaporeDollar, Record.Yen })]
        public string Currency { get; set; }
        [MaxLength(50)]
        [CSVInjection]
        public string Language { get; set; }
        public string ProfilePicture { get; set; }
        [MaxLength(500)]
        public string Password { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AccountCreatedDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? AccountExpiryDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedDateNew { get; set; }

        public int ModifiedBy { get; set; }
        [CSVInjection]
        public string ModifiedByName { get; set; }
        [MaxLength(200)]
        [CSVInjection]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        public string ReportsTo { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string Business { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string Group { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string Area { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string Location { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn1 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn2 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn3 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn4 { get; set; }
        [CSVInjection]
        public string ConfigurationColumn5 { get; set; }
        [CSVInjection]
        [MaxLengthValidation]
        public string ConfigurationColumn6 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn7 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn8 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn9 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn10 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn11 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn12 { get; set; }
        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn13 { get; set; }

        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn14 { get; set; }

        [CSVInjection]
        [MaxLength(200)]
        public string ConfigurationColumn15 { get; set; }

        public int? ConfigurationColumn1Id { get; set; }

        public int? ConfigurationColumn2Id { get; set; }

        public int? ConfigurationColumn3Id { get; set; }

        public int? ConfigurationColumn4Id { get; set; }

        public int? ConfigurationColumn5Id { get; set; }

        public int? ConfigurationColumn6Id { get; set; }

        public int? ConfigurationColumn7Id { get; set; }

        public int? ConfigurationColumn8Id { get; set; }
        public int? ConfigurationColumn9Id { get; set; }
        public int? ConfigurationColumn10Id { get; set; }
        public int? ConfigurationColumn11Id { get; set; }
        public int? ConfigurationColumn12Id { get; set; }
        public int? ConfigurationColumn13Id { get; set; }
        public int? ConfigurationColumn14Id { get; set; }
        public int? ConfigurationColumn15Id { get; set; }
        public bool IsActive { get; set; }
        public int? LocationId { get; set; }
        public int? BusinessId { get; set; }
        public int? AreaId { get; set; }
        public int? GroupId { get; set; }
        public bool IsPasswordModified { get; set; }
        public string Otp { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? LastLoggedInDate { get; set; }
        public DateTime? PasswordModifiedDate { get; set; }
        public string OrganizationCode { get; set; }
        public bool IsDeleted { get; set; }
        public bool Lock { get; set; }
        public bool TermsCondintionsAccepted { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public bool IsEnableDegreed { get; set; }
        public string ProfilePicturePath { get; set; }
        [CSVInjection]
        public string ImplicitRole { get; set; }

        public int? HouseId { get; set; }
        public string House { get; set; }
        public Guid RowGuid { get; set; }
        public bool AppearOnLeaderboard { get; set; }
        public bool IsAllConfigured { get; set; }
        [MaxLength(100)]
        public string UserSubType { get; set; }
        [MaxLength(100)]
        [CSVInjection]
        public string JobRoleName { get; set; }
        public int? JobRoleId { get; set; }
        public DateTime? DateIntoRole { get; set; }

        public string RandomUserPassword { get; set; }
        public string BuddyTrainer { get; set; }
        public string Mentor { get; set; }
        public string HRBP { get; set; }
        public int? BuddyTrainerId { get; set; }
        public int? MentorId { get; set; }
        public int? HRBPId { get; set; }
        public DateTime? AccountDeactivationDate { get; set; }
        public string FederationId { get; set; }
        public string country { get; set; }
    }

    public class APIUserMasterDetails
    {
        public string UserName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        [MaxLength(50)]
        public string TimeZone { get; set; }
        [MaxLength(50)]
        public string Currency { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
        public string ProfilePicture { get; set; }
        [MaxLength(100)]
        public string ReportsTo { get; set; }
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        public string ConfigurationColumn1 { get; set; }
        public string ConfigurationColumn2 { get; set; }
        public string ConfigurationColumn3 { get; set; }
        public string ConfigurationColumn4 { get; set; }
        public string ConfigurationColumn5 { get; set; }
        public string ConfigurationColumn6 { get; set; }
        public string ConfigurationColumn7 { get; set; }
        public string ConfigurationColumn8 { get; set; }
        public string ConfigurationColumn9 { get; set; }
        public string ConfigurationColumn10 { get; set; }
        public string ConfigurationColumn11 { get; set; }
        public string ConfigurationColumn12 { get; set; }
        public string ConfigurationColumn13 { get; set; }
        public string ConfigurationColumn14 { get; set; }
        public string ConfigurationColumn15 { get; set; }
        public string ProfilePicturePath { get; set; }
        public string House { get; set; }
        public string ChangedUserRole { get; set; }
        [MaxLength(100)]
        public string UserSubType { get; set; }
        public string JobRoleId { get; set; }
        public string Department { get; set; }
        public string WorkLocation { get; set; }
        public string Designation { get; set; }
        public string EmployeeID { get; set; }
        public string country { get; set; }
    }

    public class APIUpdateUserProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string MobileNumber { get; set; }
        [MaxLength(50)]
        public string TimeZone { get; set; }
        [MaxLength(50)]
        public string Currency { get; set; }
        public string ProfilePicture { get; set; }
        public string ProfilePicturePath { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class APIUpdateUserMobileEmail
    {

        public string MobileNumber { get; set; }
        public string EmailId { get; set; }

    }

    public class APIUserMasterProfile
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [MaxLength(50)]
        public string SerialNumber { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9-._-]*", ErrorMessage = "Invalid User Id")]
        public string UserId { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z-. ]+$", ErrorMessage = "Invalid Username")]
        public string UserName { get; set; }
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }
        [Required]
        [StringLength(25)]
        public string MobileNumber { get; set; }
        [Required]
        [MaxLength(10)]
        public string UserRole { get; set; }
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
        [DataType(DataType.Date)]
        public DateTime? LastModifiedDate { get; set; }
        public string ModifiedByName { get; set; }
        [MaxLength(100)]
        public string ReportsTo { get; set; }
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }
        public string ConfigurationColumn1 { get; set; }
        public string ConfigurationColumn2 { get; set; }
        public string ConfigurationColumn3 { get; set; }
        public string ConfigurationColumn4 { get; set; }
        public string ConfigurationColumn5 { get; set; }
        public string ConfigurationColumn6 { get; set; }
        public string ConfigurationColumn7 { get; set; }
        public string ConfigurationColumn8 { get; set; }
        public string ConfigurationColumn9 { get; set; }
        public string ConfigurationColumn10 { get; set; }
        public string ConfigurationColumn11 { get; set; }
        public string ConfigurationColumn12 { get; set; }
        public int? LocationId { get; set; }
        public int? BusinessId { get; set; }
        public int? AreaId { get; set; }
        public int? GroupId { get; set; }
        public bool IsPasswordModified { get; set; }
        public string Otp { get; set; }
        public string OrganizationCode { get; set; }
        public bool Lock { get; set; }
        public bool TermsCondintionsAccepted { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public bool IsEnableDegreed { get; set; }
        public string ProfilePicturePath { get; set; }
        public string ImplicitRole { get; set; }
        public int? HouseId { get; set; }
        public string House { get; set; }
        public string RoleName { get; set; }
        public Guid RowGuid { get; set; }
        public bool AppearOnLeaderboard { get; set; }
    }


    public class APIUserProfile
    {

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9-._-]*", ErrorMessage = "Invalid User Id")]
        public string UserId { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z-. ]+$", ErrorMessage = "Invalid Username")]
        public string UserName { get; set; }
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }
        [Required]
        [StringLength(25)]

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
        [MaxLength(100)]
        public string ReportsTo { get; set; }
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }
        public string ConfigurationColumn1 { get; set; }
        public string ConfigurationColumn2 { get; set; }
        public string ConfigurationColumn3 { get; set; }
        public string ConfigurationColumn4 { get; set; }
        public string ConfigurationColumn5 { get; set; }
        public string ConfigurationColumn6 { get; set; }
        public string ConfigurationColumn7 { get; set; }
        public string ConfigurationColumn8 { get; set; }
        public string ConfigurationColumn9 { get; set; }
        public string ConfigurationColumn10 { get; set; }
        public string ConfigurationColumn11 { get; set; }
        public string ConfigurationColumn12 { get; set; }
        public string ConfigurationColumn13 { get; set; }
        public string ConfigurationColumn14 { get; set; }
        public string ConfigurationColumn15 { get; set; }
        public string District { get; set; }
        public int? LocationId { get; set; }
        public int? BusinessId { get; set; }
        public int? AreaId { get; set; }
        public int? GroupId { get; set; }
        public string OrganizationCode { get; set; }

        public string ProfilePicturePath { get; set; }
        public string House { get; set; }
        public string RoleName { get; set; }
        public string JobRoleName { get; set; }
        public string? IsManager { get; set; }
        public string BuddyTrainerName { get; set; }
        public string MentorName { get; set; }
        public string HrbpName { get; set; }
        public string FederationId { get; set; }
        public string country { get; set; }

    }

    public class APIForgetPassword
    {
        public bool Result { get; set; }
        public string ToEmail { get; set; }
        public string CustomerCode { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public int ID { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string MobileNumber { get; set; }
    }

    public class APIFilePath
    {
        public string Path { get; set; }
    }
    public class APIUserMasterRejected
    {
        public int? Id { get; set; }

        public string CustomerCode { get; set; }

        public string SerialNumber { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string EmailId { get; set; }

        public string MobileNumber { get; set; }

        public string UserRole { get; set; }

        public string UserType { get; set; }

        public string Gender { get; set; }

        public string TimeZone { get; set; }

        public string Currency { get; set; }

        public string Language { get; set; }
        public string ProfilePicture { get; set; }

        public string Password { get; set; }

        public string AccountCreatedDate { get; set; }

        public string AccountExpiryDate { get; set; }

        public string LastModifiedDate { get; set; }

        public string ReportsTo { get; set; }
        [MaxLength(250)]
        public string BusinessCode { get; set; }
        [MaxLength(250)]
        public string GroupCode { get; set; }
        [MaxLength(250)]
        public string AreaCode { get; set; }
        [MaxLength(250)]
        public string LocationCode { get; set; }
        [MaxLength(250)]
        public string DateOfBirth { get; set; }
        [MaxLength(250)]
        public string DateOfJoining { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn1 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn2 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn3 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn4 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn5 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn6 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn7 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn8 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn9 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn10 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn11 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn12 { get; set; }
        [MaxLength(250)]
        public string IsActive { get; set; }
        [MaxLength(250)]
        public string IsPasswordModified { get; set; }
        public string OrgCode { get; set; }
        [MaxLength(500)]
        public string ErrorMessage { get; set; }
    }

    public class APISectionAdminDetails
    {
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
    }

    public class APISectionDetails
    {
        public string CustomerCode { get; set; }
        public string SectionDetail { get; set; }
        public string SectionDetailValue { get; set; }
    }

    public class APIDynamicColumnDetails
    {
        public string ConfiguredColumnName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class APIUserId
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
    }

    public class APISearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class LocationandAreaSearch
    {
        public string UserId { get; set; }
        public string BusinessName { get; set; }
        public string AreaName { get; set; }
        public string UserName { get; set; }

        public int Id { get; set; }

    }

    public class ApiMobileNumber
    {
        public string MobileNumber { get; set; }
    }

    public class ApiForgotPassword
    {
        [Required]
        public string userId { get; set; }
        [Required]
        public string orgCode { get; set; }
    }

    public class ApiGetTopRanking
    {
        public int? ranks { get; set; }
        public string configuredColumnName { get; set; }
        public string houseCode { get; set; }
        public string configuredColumnValue { get; set; }
    }

    public class ApiGetUserById
    {
        public int ID { get; set; }

    }

    public class ApiGetUserDetailsById
    {
        public string ID { get; set; }

    }

    public class ApiGetJobRoleDetails
    {
        public int JobRoleId { get; set; }
    }
    public class ApiTwoWayAuthentication
    {
        public string username { get; set; }
        public string display { get; set; }
        public int? isIos { get; set; }

    }

    public class ApiUserId
    {
        public string UserId { get; set; }
    }

    public class UserSearch
    {
        public string SearchByColumn { get; set; }

        public string SearchText { get; set; }

    }
    public class BTUserSearch
    {
        public string SearchText { get; set; }
        public string BtConfiguredOnId { get; set; }

    }

    public class ApiUserSearchById
    {
        public string SearchByColumn { get; set; }

        public string SearchText { get; set; }

        public string Id { get; set; }

    }

    public class ApiGetUserName
    {
        public int ID { get; set; }
        public string Name { get; set; }

    }

    public class ApiGetUserMasterId
    {
        public string UserID { get; set; }
    }


    public class ApiSearchUser
    {
        public string UserId { get; set; }

        public string UserType { get; set; }
    }

    public class AppConfiguration
    {
        public string Code { get; set; }

        public string value { get; set; }
    }

    public class APIUserSearch
    {
        [RegularExpression("^[0-9]*", ErrorMessage = "Invalid Page")]
        public int Page { get; set; }
        [RegularExpression("^[0-9]*", ErrorMessage = "Invalid PageSize")]
        public int PageSize { get; set; }
        public string ColumnName { get; set; }

        [CheckValidationAttribute(AllowValue = new string[] { Record.active, Record.inactive, Record.all })]

        public string Status { get; set; }
        public string Search { get; set; }

    }

    public class APISearch
    {
        public string Search { get; set; }
    }

    public class APIIntId
    {
        public int Id { get; set; }
    }


    public class ApiName
    {
        public string Name { get; set; }
    }

    public class GetMyRanking
    {
        public string ConfiguredColumnName { get; set; }

        public string HouseCode { get; set; }
    }

    public class ApiDepartmentSearch
    {
        public int DeptId { get; set; }
        public string Search { get; set; }
    }

    public class ApiLoggedHistory
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public string userID { get; set; }
    }

    public class ApiUserNameByGroup
    {
        public string Groupname { get; set; }
        public string Username { get; set; }
    }


    public class ApiSAMLInfo
    {
        public string RelayState { get; set; }
        public string SAMLResponse { get; set; }
    }

    public class ApiGetUserNameADFS
    {
        public string Id { get; set; }

    }
    public class ApiSAMLADFSInfo
    {

        public string SAMLResponse { get; set; }
    }

    public class ApiInfo
    {
        [Required]
        public string EmployeeCode { get; set; }
        [Required]
        public string OrgCode { get; set; }
    }
    public class APIPhotos
    {
        public string UserId { get; set; }
        public string CourseCode { get; set; }
        public string Photo { get; set; }
    }
    public class PhotoArray
    {
        public string Photo { get; set; }
    }
    public class APISamlInformation
    {
        public string CustomerIdentifier { get; set; }
        public string AccessToken { get; set; }
        public string CallbackURL { get; set; }
        public string Timestamp { get; set; }
    }
    public class APIIsUserValid
    {
        public bool Flag { get; set; }
        public string ErrorMessage { get; set; }
    }


    public class APIIsFileValid
    {
        public bool Flag { get; set; }
        public DataTable userImportdt { get; set; }
    }
    public class APILeaderBoard
    {
        public string UserID { get; set; }
    }
    public class APILeaderBoardData1
    {

        public int Rank { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Bonus { get; set; }
        public string Time { get; set; }
        public string CompanyName { get; set; }
        public string Department { get; set; }
    }
    public class APILeaderBoardData
    {
        public List<APILeaderBoardData1> AllUsers { get; set; }
        public List<APILeaderBoardData1> SpecificUser { get; set; }

    }

    public class DarwinBoxPayload
    {
        public string email { get; set; }
        public string timestamp { get; set; }
        public string hash { get; set; }
        public string Uid { get; set; }
        public string employee_no { get; set; }
        public string company_name { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string office_mobile { get; set; }
        public string office_location_city { get; set; }
        public string office_location_pincode { get; set; }
        public string token { get; set; }
    }

    public class APICompetencyJdUpload
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatedBy { get; set; }
        public string FilePath { get; set; }
        public List<string> CompetencyName { get; set; }

    }


}
