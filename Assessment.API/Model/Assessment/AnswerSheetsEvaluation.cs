// ======================================
// <copyright file="AnswerSheetsEvaluation.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================


using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
    public class AnswerSheetsEvaluation : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int AnswerSheetId { get; set; }
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public int Marks { get; set; }
        [MaxLength(100)]
        public string Remarks { get; set; }
    }
}
