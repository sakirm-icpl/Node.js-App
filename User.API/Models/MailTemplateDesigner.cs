// ======================================
// <copyright file="MailTemplateDesigner.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class MailTemplateDesigner : CommonFields
    {
        public int Id { get; set; }
        public String MailSubject { get; set; }
        [Required]
        public String TemplateContent { get; set; }
        [MaxLength(100)]
        [Required]
        public string CustomerCode { get; set; }
        [MaxLength(1000)]
        public int FunctionID { get; set; }
        [MaxLength(500)]
        public string EventDescription { get; set; }
        [MaxLength(200)]
        public string DataSourceFunction { get; set; }
        [MaxLength(500)]
        public string AdditionalInformation { get; set; }
        public bool Status { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
    }
}
