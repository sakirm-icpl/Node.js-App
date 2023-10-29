//======================================
// <copyright file="Hrms.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class HRMS : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        public int SerialNumber { get; set; }
        [Required]
        [MaxLength(50)]
        public string ColumnName { get; set; }
        public bool Configurable { get; set; }
        public bool Configuration { get; set; }
        [MaxLength(50)]
        public string ChangedName { get; set; }
        [Required]
        [MaxLength(50)]
        public string HRMSDataField { get; set; }
        public string Remarks { get; set; }
    }
}
