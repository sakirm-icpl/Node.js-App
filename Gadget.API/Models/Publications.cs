// ======================================
// <copyright file="Publications.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Gadget.API.Models
{
    public class Publications : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        public string Publication { get; set; }
        public int VolumeNumber { get; set; }
        public DateTime PublishedDate { get; set; }
        [MaxLength(2000)]
        public string Icon { get; set; }
        [MaxLength(2000)]
        [Required]
        public string File { get; set; }
        public int ClicksCount { get; set; }
        public int RatingCount { get; set; }
        public Decimal AverageRating { get; set; }

        [MaxLength(1000)]
       public string Metadata { get; set; }
    }
}
