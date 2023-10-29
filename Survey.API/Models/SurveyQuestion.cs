// ======================================
// <copyright file="SurveyQuestionMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class SurveyQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Section { get; set; }
        [MaxLength(1000)]
        [Required]
        public string Question { get; set; }
        public bool AllowSkipAswering { get; set; }
        public bool Status { get; set; }
        public bool IsMultipleChoice { get; set; }
        public string OptionType { get; set; }

    }
}
