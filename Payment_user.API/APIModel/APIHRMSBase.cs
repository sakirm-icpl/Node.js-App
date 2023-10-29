using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Payment.API.APIModel
{
    public class APIHRMSBase
    {


    }

    public class APIIsFileValidHRMS
    {
        public bool Flag { get; set; }
        public DataTable userImportdt { get; set; }
    }

    public class UserMasterImportFieldNew
    {
        public const string CustomerCode = "CustomerCode";
        public const string UserId = "UserId";
        public const string EmailId = "EmailId";
        public const string UserName = "UserName";
        public const string MobileNumber = "MobileNumber";
        public const string Business = "Business";
        public const string Group = "Group";
        public const string Area = "Area";
        public const string Location = "Location";
        public const string ConfigurationColumn1 = "ConfigurationColumn1";
        public const string ConfigurationColumn2 = "ConfigurationColumn2";
        public const string ConfigurationColumn3 = "ConfigurationColumn3";
        public const string ConfigurationColumn4 = "ConfigurationColumn4";
        public const string ConfigurationColumn5 = "ConfigurationColumn5";
        public const string ConfigurationColumn6 = "ConfigurationColumn6";
        public const string ConfigurationColumn7 = "ConfigurationColumn7";
        public const string ConfigurationColumn8 = "ConfigurationColumn8";
        public const string ConfigurationColumn9 = "ConfigurationColumn9";
        public const string ConfigurationColumn10 = "ConfigurationColumn10";
        public const string ConfigurationColumn11 = "ConfigurationColumn11";
        public const string ConfigurationColumn12 = "ConfigurationColumn12";
        public const string ConfigurationColumn13 = "ConfigurationColumn13";
        public const string ConfigurationColumn14 = "ConfigurationColumn14";
        public const string ConfigurationColumn15 = "ConfigurationColumn15";
        public const string AccountExpiryDate = "AccountExpiryDate";
        public const string Language = "Language";
        public const string Currency = "Currency";
        public const string DateOfBirth = "DateOfBirth";
        public const string DateOfJoining = "DateOfJoining";
        public const string Gender = "Gender";
        public const string ReportsTo = "ReportsTo";
        public const string UserType = "UserType";
        public const string UserMaster = "UserMaster";
        public const string UserMasterxlsx = "UserMaster.xlsx";
        public const string UserMasterWithData = "UserMaster.xlsx";
        public const string UserMasterData = "UserMasterData";
        public const string UserRole = "UserRole";
        public const string IsActive = "IsActive";
        public const string Status = "Status";
        public const string JobRole = "JobRole";
        public const string DateIntoRole = "DateIntoRole";
        public const string TopRanking = "TopRanking.xlsx";
        public const string UserRejected = "UserRejected.xlsx";
    }

    public class UserMasterImportFieldEncrypted
    {
        public const string UserIdEncrypted = "UserIdEncrypted";
        public const string EmailIdEncrypted = "EmailIdEncrypted";
        public const string ReportsToEncrypted = "ReportsToEncrypted";
        public const string MobileNumberEncrypted = "MobileNumberEncrypted";
        public const string BusinessEncrypted = "BusinessEncrypted";
        public const string GroupEncrypted = "GroupEncrypted";
        public const string AreaEncrypted = "AreaEncrypted";
        public const string LocationEncrypted = "LocationEncrypted";
        public const string ConfigurationColumn1Encrypted = "ConfigurationColumn1Encrypted";
        public const string ConfigurationColumn2Encrypted = "ConfigurationColumn2Encrypted";
        public const string ConfigurationColumn3Encrypted = "ConfigurationColumn3Encrypted";
        public const string ConfigurationColumn4Encrypted = "ConfigurationColumn4Encrypted";
        public const string ConfigurationColumn5Encrypted = "ConfigurationColumn5Encrypted";
        public const string ConfigurationColumn6Encrypted = "ConfigurationColumn6Encrypted";
        public const string ConfigurationColumn7Encrypted = "ConfigurationColumn7Encrypted";
        public const string ConfigurationColumn8Encrypted = "ConfigurationColumn8Encrypted";
        public const string ConfigurationColumn9Encrypted = "ConfigurationColumn9Encrypted";
        public const string ConfigurationColumn10Encrypted = "ConfigurationColumn10Encrypted";
        public const string ConfigurationColumn11Encrypted = "ConfigurationColumn11Encrypted";
        public const string ConfigurationColumn12Encrypted = "ConfigurationColumn12Encrypted";
        public const string ConfigurationColumn13Encrypted = "ConfigurationColumn13Encrypted";
        public const string ConfigurationColumn14Encrypted = "ConfigurationColumn14Encrypted";
        public const string ConfigurationColumn15Encrypted = "ConfigurationColumn15Encrypted";

    }

    public class APIIsUserValidHRMS
    {
        public bool Flag { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class APIUserSettingNew
    {
        public int Id { get; set; }
        [Required]
        public string ConfiguredColumnName { get; set; }
        [Required]
        public string ChangedColumnName { get; set; }
        public bool IsConfigured { get; set; }
        [Required]
        public bool IsMandatory { get; set; }
        [Required]
        public bool IsShowInReport { get; set; }
        public bool IsShowInAnalyticalDashboard { get; set; }
        public string ConfiguredColumnModel { get; set; }
        public string FieldType { get; set; }
    }


    public class HRMSProcessDefaults
    {
        public string allowSpecialCharInUserName { get; set; } = "false";
        public string lowerCaseAllow { get; set; } = "true";
        public string applicationDateFormat { get; set; } = "null";
    }

    public class HRMSBasicData : HRMSProcessDefaults
    {
        public string OrgCode { get; set; }
        public string RandomPassword { get; set; }
        public string spName { get; set; }
        public string importTableName { get; set; }
        public string LDap { get; set; }
        public Dictionary<string, bool> validationMatrix { get; set; } = new Dictionary<string, bool>() { { "username", true } };

    }


}
