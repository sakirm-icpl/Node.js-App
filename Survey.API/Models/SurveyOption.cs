// ======================================
// <copyright file="SurveyOption.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class SurveyOption : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(1000)]
        public string OptionText { get; set; }
        public int QuestionId { get; set; }
        public SurveyQuestion SurveyQuestions { get; set; }
    }
}
