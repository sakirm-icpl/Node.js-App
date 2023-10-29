// ======================================
// <copyright file="QuizResultDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class QuizResultDetail : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuizResultId { get; set; }
        [Required]
        public int QuizQuestionId { get; set; }
        public int QuizOptionId { get; set; }
        public QuizResult QuizResults { get; set; }
    }
}
