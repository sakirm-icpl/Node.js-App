// ======================================
// <copyright file="OfflineAssessmentScoresDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.TrainersFunctions
{
    public class OfflineAssessmentScoresDetail : BaseModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int OfflineAssessmentScoresId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ObtainedMarks { get; set; }
        public Decimal Percentage { get; set; }
        public OfflineAssessmentScores OfflineAssessmentScore { get; set; }
    }
}
