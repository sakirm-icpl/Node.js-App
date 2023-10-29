//======================================
// <copyright file="UserSettings.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.ComponentModel.DataAnnotations;
using User.API.Helper;
using User.API.Validation;

namespace User.API.Models
{
    public class UserSettings : CommonFields
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
        [MaxLength(10)]
        public string RoleCode { get; set; }
        [MaxLength(200)]
        [CheckValidationAttribute(AllowValue = new string[] { Record.TYPEAHEAD, Record.TEXTBOX })]
        public string FieldType { get; set; }
        [MaxLength(20)]
        public string DevCode { get; set; }

        public bool IsUnique  { get; set; } = false;
        public bool IsConfidential { get; set; } = false;
        public bool IsShowInReportFilter { get; set; } = false;



    }
}
