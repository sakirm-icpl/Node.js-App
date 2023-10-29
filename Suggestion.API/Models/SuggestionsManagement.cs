// ======================================
// <copyright file="SuggestionsManagement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Suggestion.API.Models
{
    public class SuggestionsManagement : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime SuggestionDate { get; set; }
        [MaxLength(500)]
        [Required]
        public string Suggestion { get; set; }
        [MaxLength(500)]
        public string ContextualAreaofBusiness { get; set; }
        [MaxLength(500)]
        [Required]
        public string BriefResponse { get; set; }
        [MaxLength(1000)]
        public string DetailedResponse { get; set; }
        public bool Status { get; set; }
        public string ApprovalStatus { get; set; }
    }
}
