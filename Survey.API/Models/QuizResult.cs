// ======================================
// <copyright file="QuizResult.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class QuizResult : CommonFields
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        [MaxLength(100)]
        [Required]
        public string QuizResultStatus { get; set; }
        [Required]
        public int UserId { get; set; }
        public List<QuizResultDetail> QuizResultDetails { get; set; }
    }
}
