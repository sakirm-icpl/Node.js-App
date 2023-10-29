using System;
using System.ComponentModel.DataAnnotations;
using User.API.Helper;
using User.API.Validation;

namespace User.API.APIModel
{
    public class APIUserMasterDelete
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
        [CSVInjection]
        public string UserId { get; set; }
        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z-. ]+$", ErrorMessage = "Invalid Username")]
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
        [CheckValidationAttribute(AllowValue = new string[] { Record.English, Record.Hindi, Record.Marathi, Record.Arabic })]
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
        public string? deletedBy{ get; set; }
        public DateTime? deletedDate { get; set; }
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
    }

}

