// ======================================
// <copyright file="APIAnnouncements.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Gadget.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Gadget.API.APIModel
{
    public class APIAnnouncements //: CommonFields
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        [CSVInjection]
        public string Announcement { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalReadCount { get; set; }
    }

    public class APIMyAnnouncement //: CommonFields
    {
        public int? Id { get; set; }
        public int AnnouncementId { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string FunctionCode { get; set; }
    }

}
