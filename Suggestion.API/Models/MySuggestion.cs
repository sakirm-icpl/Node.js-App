// ======================================
// <copyright file="MySuggestion.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Suggestion.API.Models
{
    public class MySuggestion : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        public string SuggestionBrief { get; set; }
        [Required]
        [MaxLength(500)]
        public string ContextualAreaofBusiness { get; set; }
        [MaxLength(1000)]
        public string DetailedDescription { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
        public List<MySuggestionDetail> MySuggestionDetails { get; set; }
    }
}
