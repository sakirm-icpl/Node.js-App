// ======================================
// <copyright file="APIOfflineAssessmentScores.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;

namespace Courses.API.APIModel.TrainersFunctions
{
    public class APIOfflineAssessmentScores
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public int SessionId { get; set; }
        public int TotalMarks { get; set; }
    }

    public class APIOfflineAssessmentScoresDetail
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public int OfflineAssessmentScoresId { get; set; }
        public int UserId { get; set; }
        public int ObtainedMarks { get; set; }
        public Decimal Percentage { get; set; }
    }
    public class APIOfflineAssessmentScoresMerged
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public int SessionId { get; set; }
        public int TotalMarks { get; set; }
        public APIOfflineAssessmentScoresDetail[] aPIOfflineAssessmentScoresDetail { get; set; }
    }
}
