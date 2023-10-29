// ======================================
// <copyright file="OfflineAssessmentScores.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.TrainersFunctions
{
    public class OfflineAssessmentScores : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int BatchId { get; set; }
        [Required]
        public int ModuleId { get; set; }
        [Required]
        public int SessionId { get; set; }
        [Required]
        public int TotalMarks { get; set; }
        public List<OfflineAssessmentScoresDetail> OfflineAssessmentScoresDetails { get; set; }
    }
}
