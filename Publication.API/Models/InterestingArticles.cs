// ======================================
// <copyright file="InterestingArticles.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Publication.API.Models
{
    public class InterestingArticles : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        [MaxLength(500)]
        [Required]
        public string Category { get; set; }
        [MaxLength(500)]
        [Required]
        public string Article { get; set; }
        [Required]
        public string ArticleDescription { get; set; }
        public DateTime ValidityDate { get; set; }
        [MaxLength(20)]
        public bool Status { get; set; }
        public bool ShowToAll { get; set; }
    }
}
