//======================================
// <copyright file="UserMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class UserMasterRejected : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string CustomerCode { get; set; }
        [MaxLength(250)]
        public string SerialNumber { get; set; }
        [MaxLength(250)]
        public string UserId { get; set; }
        [MaxLength(250)]
        public string UserName { get; set; }
        [MaxLength(250)]
        public string EmailId { get; set; }
        [MaxLength(250)]
        public string MobileNumber { get; set; }
        [MaxLength(250)]
        public string UserRole { get; set; }
        [MaxLength(250)]
        public string UserType { get; set; }
        [MaxLength(250)]
        public string Gender { get; set; }
        [MaxLength(250)]
        public string TimeZone { get; set; }
        [MaxLength(250)]
        public string Currency { get; set; }
        [MaxLength(250)]
        public string Language { get; set; }
        public string ProfilePicture { get; set; }
        [MaxLength(250)]
        public string Password { get; set; }
        [MaxLength(250)]
        public string AccountCreatedDate { get; set; }
        [MaxLength(250)]
        public string AccountExpiryDate { get; set; }
        [MaxLength(250)]
        public string LastModifiedDate { get; set; }
        [MaxLength(250)]
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
        public string ConfigurationColumn13 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn14 { get; set; }
        [MaxLength(250)]
        public string ConfigurationColumn15 { get; set; }
        [MaxLength(250)]
        public string IsActive { get; set; }
        [MaxLength(250)]
        public string IsPasswordModified { get; set; }
        [MaxLength(500)]
        public string ErrorMessage { get; set; }
    }
}
