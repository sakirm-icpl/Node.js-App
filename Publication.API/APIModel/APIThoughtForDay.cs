// ======================================
// <copyright file="APIThoughtForDay.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Publication.API.APIModel
{
    public class APIThoughtForDay //: CommonFields
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        [CSVInjection]
        public string Thought { get; set; }
        public DateTime ForDate { get; set; }
        public int TotalLikesForDate { get; set; }
        public int TotalDeslikesForDate { get; set; }
        public int TotalLikes { get; set; }
    }
    public class APIThoughtForDayCounter //: CommonFields
    {
        public int? Id { get; set; }
        public int ThoughtForDayId { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public bool UserAction { get; set; }
    }
}
