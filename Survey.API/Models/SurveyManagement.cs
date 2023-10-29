// ======================================
// <copyright file="SurveyManagement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class SurveyManagement : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        public string SurveySubject { get; set; }
        [MaxLength(500)]
        public string SurveyPurpose { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ValidityDate { get; set; }
        public int AverageRespondTime { get; set; }
        public int TargetResponseCount { get; set; }
        public int LcmsId { get; set; }
        public bool Status { get; set; }
        public bool IsApplicableToAll { get; set; }
        public List<SurveyQuestion> SurveyQuestions { get; set; }
    }
}
