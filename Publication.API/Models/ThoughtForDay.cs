// ======================================
// <copyright file="ThoughtForDay.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Publication.API.Models
{
    public class ThoughtForDay : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        public string Thought { get; set; }
        public DateTime ForDate { get; set; }
        public int TotalLikesForDate { get; set; }
        public int TotalDeslikesForDate { get; set; }
        public int TotalLikes { get; set; }
    }
}
