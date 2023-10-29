//======================================
// <copyright file="APIUserSetting.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIUserSetting
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
        public bool IsConfidential { get; set; }
        public bool IsUnique { get; set; }
        public bool IsShowInReportFilter { get; set; }
    }

    public class APIUsersSetting
    {
        public string ConfiguredColumnName { get; set; }
        public string ChangedColumnName { get; set; }
        public bool IsConfigured { get; set; }
        public bool IsMandatory { get; set; }
        public string FieldType { get; set; }
        public bool IsShowInAnalyticalDashboard { get; set; }
        public bool IsConfidential { get; set; }
        public bool IsUnique { get; set; }
    }
    public class APIUserSettingRole
    {
        public string ChangedColumnName { get; set; }
        public string RoleCode { get; set; }
    }
}
