// ======================================
// <copyright file="APITrainingAttendance.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;

namespace Courses.API.APIModel.TrainersFunctions
{
    public class APITrainingAttendance
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public int SessionId { get; set; }
    }

    public class APITrainingAttendanceDetail
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public int TrainingAttendanceId { get; set; }
        public int UserId { get; set; }
        public bool IsPresent { get; set; }
        public string Remarks { get; set; }
    }

    public class APITrainingAttendanceMerged
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public int SessionId { get; set; }
        public APITrainingAttendanceDetail[] aPITrainingAttendanceDetail { get; set; }
    }
}