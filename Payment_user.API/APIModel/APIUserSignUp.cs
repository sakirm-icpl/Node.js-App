using Payment.API.Helper;
using Payment.API.Models;
using Payment.API.Repositories.Interfaces;
using Payment.API.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Payment.API.Models.UserMaster;

namespace Payment.API.APIModel
{
    public class APIUserSignUp
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [MaxLength(50)]
        public string SerialNumber { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9-._-]*", ErrorMessage = "Invalid User Id")]
        [CSVInjection]
        public string UserId { get; set; }
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        [CSVInjection]
        public string UserName { get; set; }
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        [CSVInjection]
        public string EmailId { get; set; }
        [Required]
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
        [CSVInjection]

        [MaxLength(15)]
        public string MobileNumber { get; set; }

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
        [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")]
        public string Password { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AccountCreatedDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? AccountExpiryDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? LastModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        [CSVInjection]
        public string ModifiedByName { get; set; }
        [MaxLength(100)]
        [CSVInjection]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        public string ReportsTo { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string Business { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string Group { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string Area { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string Location { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn1 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn2 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn3 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn4 { get; set; }
        [CSVInjection]
        public string ConfigurationColumn5 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn6 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn7 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn8 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn9 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn10 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn11 { get; set; }
        [CSVInjection]
        [MaxLength(100)]
        public string ConfigurationColumn12 { get; set; }

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

        public int? CourseId { get; set; }
        public bool? IsEnrolled { get; set; }
        public string City { get; set; }
        public string CustomerProfile { get; set; }

        public string loginId { get; set; }

        public string employeeGroup { get; set; }
        public string retirementDate { get; set; }
        public string AadhaarPath { get; set; }
        public Exists ValidationsForUserSignUp(IUserSignUpRepository userRepository, APIUserSignUp user)
        {
            if (userRepository.ExistsUserSignUp(user.UserId))
                return Exists.UserIdExist;
            if (userRepository.EmailExistsForUserSignUp(user.EmailId))
                return Exists.EmailIdExist;
            if (userRepository.MobileExistsForUserSignUp((user.MobileNumber)))
                return Exists.MobileExist;

            return Exists.No;
        }
        public APIUserSignUp MapUserSignupToAPIUser(APIUserSignUp user)
        {
            APIUserSignUp apiUser = new APIUserSignUp();
            apiUser.Area = user.Area == null ? null : user.Area.Trim();
            apiUser.Location = user.Location == null ? null : user.Location.Trim();
            apiUser.Group = user.Group == null ? null : user.Group.Trim();
            apiUser.Business = user.Group == null ? null : user.Business.Trim();
            apiUser.AccountExpiryDate = null;
            apiUser.ConfigurationColumn1 = user.ConfigurationColumn1 == null ? null : user.ConfigurationColumn1.Trim();
            apiUser.ConfigurationColumn2 = user.ConfigurationColumn2 == null ? null : user.ConfigurationColumn2.Trim();
            apiUser.ConfigurationColumn3 = user.ConfigurationColumn3 == null ? null : user.ConfigurationColumn3.Trim();
            apiUser.ConfigurationColumn4 = user.ConfigurationColumn4 == null ? null : user.ConfigurationColumn4.Trim();
            apiUser.ConfigurationColumn5 = user.ConfigurationColumn5 == null ? null : user.ConfigurationColumn5.Trim();
            apiUser.ConfigurationColumn6 = user.ConfigurationColumn6 == null ? null : user.ConfigurationColumn6.Trim();
            apiUser.ConfigurationColumn7 = user.ConfigurationColumn7 == null ? null : user.ConfigurationColumn7.Trim();
            apiUser.ConfigurationColumn8 = user.ConfigurationColumn8 == null ? null : user.ConfigurationColumn8.Trim();
            apiUser.ConfigurationColumn9 = user.ConfigurationColumn9 == null ? null : user.ConfigurationColumn9.Trim();
            apiUser.ConfigurationColumn10 = user.ConfigurationColumn10 == null ? null : user.ConfigurationColumn10.Trim();
            apiUser.ConfigurationColumn11 = user.ConfigurationColumn11 == null ? null : user.ConfigurationColumn11.Trim();
            apiUser.ConfigurationColumn12 = user.ConfigurationColumn12 == null ? null : user.ConfigurationColumn12.Trim();
            apiUser.Currency = null;
            apiUser.DateOfBirth = null;
            apiUser.DateOfJoining = null;
            apiUser.EmailId = user.EmailId;
            apiUser.Gender = null;
            apiUser.Id = user.Id;
            apiUser.Language = null;
            apiUser.ReportsTo = null;
            apiUser.MobileNumber = user.MobileNumber;
            apiUser.TimeZone = user.TimeZone;
            apiUser.UserId = user.UserId;
            apiUser.UserName = user.UserName;
            apiUser.UserRole = "CA";
            apiUser.UserType = "Internal";
            apiUser.Password = user.Password;
            apiUser.loginId = user.loginId;
            return apiUser;



        }
    }

    public class UserExists
    {
        [MaxLength(50)]
        public string OrganizationCode { get; set; }
        public string UserId { get; set; }
    }
    public class APIUserSignUpCsl
    {


        [MaxLength(50)]
        public string CustomerCode { get; set; }


        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z0-9][a-zA-Z0-9-._-]*", ErrorMessage = "Invalid User Id")]
        [CSVInjection]
        public string UserId { get; set; }

    }
    public class APICslTokenDetails
    {
        [MaxLength(50)]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
    }
    public class APIEmpDetailsForOTP
    {
        [MaxLength(50)]
        public string action { get; set; }
        [Required]
        public string empCode { get; set; }
        [Required]
        public string otp { get; set; }

    }
    public class APIEmpDetailsForOTPResp
    {
        public string action { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }


    public class APITVSUserData
    {
        [MaxLength(200)]
        public string fullName { get; set; }
        [Required]
        [MaxLength(50)]
        public string employee_id { get; set; }
        public string? email { get; set; }
        public string? mobile { get; set; }
        public string? functionname { get; set; }
        public string? region { get; set; }
        public string? location { get; set; }
        public string? reporting_manager_name { get; set; }
        public string? reporting_manager_id { get; set; }
        public string? state { get; set; }
        public string? position { get; set; }
        public string? designation { get; set; }
        public string? pl_area { get; set; }
        public string? product { get; set; }
        public string? department { get; set; }
        public string? last_working_date { get; set; }
        public string? status_of_resignation { get; set; }
        public string? grade { get; set; }
        public bool? IsActive { get; set; }
        public string? actionFlag { get; set; }
    }

    public class APIHRMSEmployeeEncrypted
    {
        public string EmpName { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
        public string EmpId { get; set; }
    }

    public class APIHRMSEmployee
    {
        public string EmpName { get; set; }
        public string EmpStatus { get; set; }
        public string Action { get; set; }
        public string EmpId { get; set; }
        public DateTime createdDate { get; set; }
    }

    public class APIHRMSResponse
    {
        public string StatusCode { get; set; }
        public bool Success { get; set; }
        public string AppName { get; set; }
        public string StatusMessage { get; set; }
        public List<APIHRMSEmployee> Employee { get; set; }
    }

    public class APIHRMSResponseNew
    {
        public string StatusCode { get; set; }
        public string AppName { get; set; }
        public string StatusMessage { get; set; }
        public string EmpName { get; set; }
        public string EmpStatus { get; set; }
        public string Action { get; set; }
        public string EmpId { get; set; }
    }


}
