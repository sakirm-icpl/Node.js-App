//======================================
// <copyright file="ResponseMessage.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.ComponentModel;

namespace User.API.Models
{
    public class ResponseMessage
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
    }
    public class ResponseMessageV2 : ResponseMessage
    {
        public string MaskEmail { get; set; }
        public string MaskMobile { get; set; }
    }
    public enum MessageType
    {
        [Description("Record was saved successfully")]
        Success,
        [Description("Failed to save")]
        Fail,
        [Description("Duplicate! Already exist.")]
        Duplicate,
        [Description("Deleted record")]
        Delete,
        [Description("No record found!")]
        NotFound,
        [Description("Record does not exist!")]
        NotExist,
        [Description("Data not available")]
        DataNotAvailable,
        [Description("Invalid data!")]
        InvalidData,
        [Description("Invalid OTP!")]
        InvalidOTP,
        [Description("Employee code '' already exists in the system")]
        validuserforlogin,
        [Description("Invalid file formate!")]
        InvalidFile,
        [Description("Internal Server Error!")]
        InternalServerError,
        [Description("Unique value changed!")]
        UniqueChanged,
        [Description("Does Not Exist!")]
        DoesNotExist,
        [Description("Self ReportTo Not Allow")]
        SelfReportToNotAllow,
        [Description("Dependency Exist")]
        DependencyExist,
        [Description("Limit Extended")]
        LimitExtended,
        [Description("The field NewPassword must match the regular expression")]
        PasswordNotMatch,
        [Description("Data Not Valid")]
        DataNotValid,
        [Description("Invalid Organization Code")]
        InvalidOrganizationCode,
        [Description("DependancyExist")]
        DependancyExist,
        [Description("Duplicate! Mobile Number Already exist.")]
        DuplicateEmailId,
        [Description("Duplicate! Email Id Already exist.")]
        DuplicateMobileNumber,
        [Description("Language Does not exist!")]
        LanguageDoesNotExist,
        [Description("Please Enter Valid Mobile Number")]
        MobileNumberNotExists,
        [Description("Mobile number should not be less than 10 digits")]
        MobileNumberLessNotExists,
        [Description("Invalid User.")]
        InvalidUser,
        [Description("Please Enter valid BirthDate")]
        BirthdateNotValid,
        [Description("Your OTP Expired!")]
        OTPExpired,
        [Description("You don't have permission to delete.")]
        NoPermission,
        [Description("Account expiry date should be greater than today's date.")]
        AccountExpiryDate,
       [Description("Please set up birth date to Generate Password.")]
        MissingBirthDate,
        [Description("Account Exists! Please contact nodal officer for details.")]
        DuplicateAccount,
        [Description("Account Already Exists!")]
        AccountExists,
        [Description("No Airport defined for this user.")]
        InvalidInfo,
        [Description("Sorry! This Course request does not exists.")]
        InvalidRequest,
        [Description("Sorry! This Course request payment is already done.")]
        RequestPaymentDone,
        [Description("Duplicate! Aadhar Number Already exist.")]
        DuplicateAadharNumber,
        [Description("Already Registered User! Please login to request new courses.")]
        ApprovedRequestsExists,
        [Description("Already Registered User! Pending course request exists.")]
        PendingRequestsExists,
        [Description("Already Requested this course.")]
        AlreadyRequested,
        [Description("Already Registered to this course.")]
        AlreadyRegistered,
        [Description("You Can not delete Yourself.")]
        CannotDeleteYourself,
        [Description("First Name atleast have 2 letters.")]
        FirstNameLength,
        [Description("Last Name atleast have 2 letters.")]
        LastNameLength,
    }
    public class PaymentResponseMessage
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public string OrderNumber { get; set; }
        public string OrderAmount { get; set; }
        public string UserName { get; set; }
        public string TransactionId { get; set; }
    }
}
