// ======================================
// <copyright file="Announcements.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Gadget.API.Models
{
    public class Announcements : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        public string Announcement { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalReadCount { get; set; }
    }
}
