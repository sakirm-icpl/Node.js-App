// ======================================
// <copyright file="SurveyResultDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class SurveyResultDetail : CommonFields
    {
        public int Id { get; set; }
        public int SurveyResultId { get; set; }
        [MaxLength(100)]
        public string Section { get; set; }
        [Required]
        public int ServeyQuestionId { get; set; }
        public int? ServeyOptionId { get; set; }
        public string SubjectiveAnswer { get; set; }
        public SurveyResult SurveyResults { get; set; }
    }
}
