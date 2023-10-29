using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Saml.API.Helper;
using Saml.API.Validation;

namespace Saml.API.APIModel
{
//    public class APINodalUserOrgCode
//    {
//        public string OrgCode { get; set; }
//    }
//    public class APINodalUserTypeAhead
//    {
//        public string OrgCode { get; set; }
//        public string SearchText { get; set; }
//    }
//    public class APINodalUserTypeAheadResponse
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//    }
    //public class APINodalUserSignUp
    //{
    //    [Required]
    //    public int CourseId { get; set; }
    //    [Required]
    //    public string UserName { get; set; }
    //    [Required]
    //    public string FHName { get; set; }
    //    [Required]
    //    [DataType(DataType.Date)]
    //    public DateTime? DateOfBirth { get; set; }
    //    [Required]
    //    [MaxLength(100)]
    //    [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
    //    [CSVInjection]
    //    public string EmailId { get; set; }
    //    [Required]
    //    [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
    //    [CSVInjection]
    //    [MaxLength(15)]
    //    public string MobileNumber { get; set; }
    //    [Required]
    //    public string AadharNumber { get; set; }
    //    public int? ConfigurationColumn10Id { get; set; }
    //    [Required]
    //    public string ConfigurationColumn10 { get; set; }
    //    [Required]
    //    public int AirPortId { get; set; }
    //    public string OrgCode { get; set; }
    //    public int? ConfigurationColumn7Id { get; set; }
    //    public string ConfigurationColumn7 { get; set; }
    //    public string AadhaarPath { get; set; }
    //}
    //public class APITTUserSignUp
    //{
    //    [Required]
    //    public int CourseId { get; set; }
    //    [Required]
    //    public string Firstname { get; set; }
    //    [Required]
    //    public string Lastname { get; set; }
    //    [Required]
    //    [DataType(DataType.Date)]
    //    public DateTime? DateOfBirth { get; set; }
    //    [Required]
    //    [MaxLength(100)]
    //    [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
    //    [CSVInjection]
    //    public string EmailId { get; set; }
    //    [Required]
    //    [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
    //    [CSVInjection]
    //    [MaxLength(15)]
    //    public string MobileNumber { get; set; }
    //    //[Required]
    //    //public string AadharNumber { get; set; }
    //    public int? ConfigurationColumn1Id { get; set; }
    //    [Required]
    //    public string ConfigurationColumn1 { get; set; }

    //    public int? ConfigurationColumn11Id { get; set; }
    //    [Required]
    //    public string ConfigurationColumn11 { get; set; }

    //    public int? ConfigurationColumn12Id { get; set; }
    //    [Required]
    //    public string ConfigurationColumn12 { get; set; }


    //    public string OrgCode { get; set; }
       
    //    public string AadhaarPath { get; set; }
    //}
    //public class APIGroupAdminSignUp
    //{
    //    [Required]
    //    public string UserId { get; set; }
    //    [Required]
    //    public string UserName { get; set; }
    //    [Required]
    //    public string FHName { get; set; }
    //    [Required]
    //    [DataType(DataType.Date)]
    //    public DateTime? DateOfBirth { get; set; }
    //    [Required]
    //    [MaxLength(100)]
    //    [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
    //    [CSVInjection]
    //    public string EmailId { get; set; }
    //    [Required]
    //    [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
    //    [CSVInjection]
    //    [MaxLength(15)]
    //    public string MobileNumber { get; set; }
    //    [Required]
    //    public string AadharNumber { get; set; }
    //    public int? ConfigurationColumn10Id { get; set; }
    //    [Required]
    //    public string ConfigurationColumn10 { get; set; }
    //    public int? ConfigurationColumn11Id { get; set; }
    //    [Required]
    //    public string ConfigurationColumn11 { get; set; }
    //    [Required]
    //    public int AirPortId { get; set; }
    //    public string OrgCode { get; set; }
    //}
    public class APINodalUser : APIUserMaster
    {
        public string AadharNumber { get; set; }
        public string FHName { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Response { get; set; }
        public string AadhaarPath { get; set; }
    }
    public class APITTUser : APIUserMaster
    {
        public string AadharNumber { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Response { get; set; }
        public string AadhaarPath { get; set; }
    }
    public class APINodalUserDetails
    {
        public int NodalUserId { get; set; }
        public string NodalUserName { get; set; }
        public string NodalEmailID { get; set; }
        public string NodalMobileNumber { get; set; }
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
        public string AirPort { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string OrgCode { get; set; }
        public string CourseTitle { get; set; }
    }
    public class NodalCourseRequest
    {
        public static string Individual = "Individual";
        public static string Group = "Group";
    }
    public class APIGroupCode
    {
        [Required]
        public string GroupCode { get; set; }
    }
    public class APINodalUserGroups
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string GroupCode { get; set; }
        public string Path { get; set; }
    }
    public class APINodalUserDelete
    {
        public int GroupMapId { get; set; }
    }
    public class APINodalGroupDelete
    {
        public int GroupId { get; set; }
    }
    public class APINodalUserGroupImportColumns
    {
        public const string UserName = "FullName";
        public const string FatherHusbandName = "FatherHusbandName";
        public const string DateOfBirth = "DateOfBirth";
        public const string EmailId = "EmailId";
        public const string MobileNumber = "MobileNumber";
        public const string AadharNumber = "AadharNumber";
    }
    public class APINodalUserInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string FatherHusbandName { get; set; }
        public string DateOfBirth { get; set; }
        public DateTime? DateOfBirth1 { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string AadharNumber { get; set; }
        public int AirPortId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class APIFileName
    {
        public const string GroupImportFormat = "GroupImportFormat.xlsx";
    }
    public class APICourseGroups
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public int NosUsers { get; set; }
    }
    public class APICourseGroupUsers
    {
        public int GroupId { get; set; }
        public string GroupCode { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public int UserMasterId { get; set; }
        public int GroupMapId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string FHName { get; set; }
        public string AadharNumber { get; set; }
        public string Airport { get; set; }
        public string OrganizationID { get; set; }
        public string AadhaarPath { get; set; }
    }
    public class APICourseRegistrations
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string FHName { get; set; }
        public string AadharNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RequestStatus { get; set; }
        public bool IsPaymentDone { get; set; }
        public string PaymentStatus { get; set; }
        public string Reason { get; set; }
        public string CourseStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string OrganizationDetails { get; set; }
        public string OrganizationID { get; set; }
        public string AadhaarPath { get; set; }
    }
    public class APIPaymentDetails
    {
        public int RequestId { get; set; }
    }
    public class APIPaymentRequestData
    {
        public string Url { get; set; }
        public string requestData { get; set; }
        public string merchantId { get; set; }
    }
    public class MerchantParams
    {
        [Required]
        public string merchantId { get; set; }

        [Required]
        public string apiKey { get; set; }

        [Required]
        public string txnId { get; set; } // Merchant Order Number

        [Required]
        public string amount { get; set; }

        [Required]
        public string dateTime { get; set; }

        [Required]
        public string custMail { get; set; }

        [Required]
        public string custMobile { get; set; }

        [Required]
        public string udf1 { get; set; }

        [Required]
        public string udf2 { get; set; }

        [Required]
        public string returnURL { get; set; }

        [Required]
        public string isMultiSettlement { get; set; }

        [Required]
        public string productId { get; set; }

        [Required]
        public string channelId { get; set; }

        [Required]
        public string txnType { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public string instrumentId { get; set; }
        public string cardDetails { get; set; }
        public string cardType { get; set; }

    }
    public class VerificationResponse
    {
        public string payment_mode { get; set; }
        public string resp_message { get; set; }
        public string udf5 { get; set; }
        public string cust_email_id { get; set; }
        public string udf3 { get; set; }
        public string merchant_id { get; set; }
        public string txn_amount { get; set; }
        public string udf4 { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string pg_ref_id { get; set; }
        public string txn_id { get; set; }
        public string resp_date_time { get; set; }
        public string bank_ref_id { get; set; }
        public string resp_code { get; set; }
        public string txn_date_time { get; set; }
        public string trans_status { get; set; }
        public string cust_mobile_no { get; set; }
    }
    public class PayproceesResponseClass
    {
        public string payment_mode { get; set; }
        public string resp_message { get; set; }
        public string udf5 { get; set; }
        public string cust_email_id { get; set; }
        public string udf3 { get; set; }
        public string merchant_id { get; set; }
        public string txn_amount { get; set; }
        public string udf4 { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string pg_ref_id { get; set; }
        public string txn_id { get; set; }
        public string resp_date_time { get; set; }
        public string bank_ref_id { get; set; }
        public string resp_code { get; set; }
        public string txn_date_time { get; set; }
        public string trans_status { get; set; }
        public string cust_mobile_no { get; set; }
        public bool IsStatusShown { get; set; }
    }
    public class CourseApplicablityEmails
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string LM_EmailID { get; set; }
        public string LA_EmailID { get; set; }
        public string TrainingName { get; set; }
        public string CourseTitle { get; set; }
        public int? CourseId { get; set; }
        public int? UserId { get; set; }
    }
    public class ApiNotification
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; }
        public int UserId { get; set; }
        public int? CourseId { get; set; }

    }
    public class APIGroupEmails
    {
        public List<APIGroupUsers> aPIGroupUsers { get; set; }
        public List<APINodalUsers> aPINodalUsers { get; set; }
        public string GroupCode { get; set; }
        public string CourseTitle { get; set; }
        public string OrgCode { get; set; }
    }
    public class APIGroupUsers
    {
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
    }
    public class APINodalUsers
    {
        public int UserMaserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
    }
    public class APIPaymentMailDetails
    {
        public string OrgCode { get; set; }
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string CourseTitle { get; set; }
        public string OrderNumber { get; set; }
        public string OrderAmount { get; set; }
        public string GroupCode { get; set; }
        public string Password { get; set; }
    }
    public class DualVerification : Timer
    {
        public string MerchantId { get; set; }
        public string TransactionId { get; set; }
        public string ConnectionString { get; set; }
        public string OrgCode { get; set; }
    }
    public class PaymentStatusRequest
    {
        public string Id { get; set; }
    }
    public class PaymentStatusResponse
    {
        public string status { get; set; }
        public string c { get; set; }
    }
    public class PaymentStatusResponseInner
    {
        public string s { get; set; }
        public string m { get; set; }
        public string onum { get; set; }
        public string oa { get; set; }
        public string u { get; set; }
    }
    public class APIGroupAdminDetails
    {
        public string OrgCode { get; set; }
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
    }
    public class APIDhangyanUserSignUp
    {
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [CheckValidationAttribute(AllowValue = new string[] { Record.Male, Record.Female, Record.Other })]
        public string Gender { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        [CSVInjection]
        public string EmailId { get; set; }
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
        [CSVInjection]
        [MaxLength(15)]
        public string MobileNumber { get; set; }
        [Required]
        public int StateId { get; set; }
        [Required]
        public string OrganizationType { get; set; }
        public string Organization { get; set; }
        public string OrgCode { get; set; }
    }

    public class APISchoolDhangyanSignUp
    {
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
       
        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        [CSVInjection]
        public string EmailId { get; set; }
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
        [CSVInjection]
        [MaxLength(15)]
        public string MobileNumber { get; set; }      
        [Required]
        [MaxLength(15)]
        public string school_name { get; set; }
        [Required]
        [MaxLength(15)]
        public string class_name { get; set; }
        [Required]
        [MaxLength(15)]
        public string registration { get; set; }
        public string OrgCode { get; set; }
    }
    public class APIDhangyanUserSignUpResponse : APIDhangyanUserSignUp
    {
        public string UserId { get; set; }
        public int Response { get; set; }
    }
    public class APIDhangyanUser : APIUserMaster
    {
        public int Response { get; set; }
    }
    public class APIDhangyanUserTypeAhead
    {
        public string OrgCode { get; set; }
        public string OrganizationType { get; set; }
        public string SearchText { get; set; }
    }

    public class APITTGroupUserSignUp
    {
        [Required]
        public string FirstName { get; set; }       
        [Required]
        public string LastName { get; set; }   
      
        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        [CSVInjection]
        public string EmailId { get; set; }
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile no")]
        [CSVInjection]
        [MaxLength(15)]
        public string MobileNumber { get; set; }
        [Required]
        public string Organization { get; set; }
        public string OrgCode { get; set; }
        [Required]
        public int CityId { get; set; }
        [Required]
        public int CourseId { get; set; }
        public string AadhaarPath { get; set; }

    }

    public class TransactionRequest
    {
        public int Id { get; set; }
        public string order_id { get; set; }
        public string orderNo { get; set; }
        public string tracking_id { get; set; }
        public string bank_ref_no { get; set; }
        public string order_status { get; set; }
        public string failure_message { get; set; }
        public string payment_mode { get; set; }
        public string card_name { get; set; }
        public string status_code { get; set; }
        public string status_message { get; set; }
        public string amount { get; set; }
        public string billing_name { get; set; }
        public string billing_address { get; set; }
        public string billing_city { get; set; }
        public string billing_state { get; set; }
        public string billing_zip { get; set; }
        public string billing_country { get; set; }
        public string billing_tel { get; set; }
        public string billing_email { get; set; }
        public string delivery_name { get; set; }
        public string delivery_address { get; set; }
        public string delivery_city { get; set; }
        public string delivery_state { get; set; }
        public string delivery_zip { get; set; }
        public string delivery_country { get; set; }
        public string delivery_tel { get; set; }
        public string merchant_param1 { get; set; }
        public string merchant_param2 { get; set; }
        public string merchant_param3 { get; set; }
        public string merchant_param4 { get; set; }
        public string merchant_param5 { get; set; }
        public string vault { get; set; }
        public string offer_type { get; set; }
        public string offer_code { get; set; }
        public string discount_value { get; set; }
        public string mer_amount { get; set; }
        public string eci_value { get; set; }
        public string retry { get; set; }
        public string response_code { get; set; }
        public string billing_notes { get; set; }
        public string trans_date { get; set; }
        public string bin_country { get; set; }
    }

}
