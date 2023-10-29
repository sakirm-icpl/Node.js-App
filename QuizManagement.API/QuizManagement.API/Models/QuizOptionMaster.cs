// ======================================
// <copyright file="QuizOptionMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace QuizManagement.API.Models
{
    public class QuizOptionMaster : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(1000)]
        public string AnswerText { get; set; }
        [MaxLength(2000)]
        public string AnswerPicturePath { get; set; }
        public int QuizQuestionId { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public QuizQuestionMaster QuizQuestionMasters { get; set; }
    }
}
