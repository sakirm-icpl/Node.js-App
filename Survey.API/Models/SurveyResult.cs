// ======================================
// <copyright file="SurveyResult.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class SurveyResult : CommonFields
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        [MaxLength(100)]
        [Required]
        public string SurveyResultStatus { get; set; }
        [Required]
        public int UserId { get; set; }
        public List<SurveyResultDetail> SurveyResultDetails { get; set; }
    }
}
