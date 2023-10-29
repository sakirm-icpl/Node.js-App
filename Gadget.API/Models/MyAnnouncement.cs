// ======================================
// <copyright file="MyAnnouncement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Gadget.API.Models
{
    public class MyAnnouncement : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int AnnouncementId { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        [MaxLength(100)]
        public string FunctionCode { get; set; }
    }
}
