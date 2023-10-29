// ======================================
// <copyright file="TrainingAttendanceDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.TrainersFunctions
{
    public class TrainingAttendanceDetail : BaseModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TrainingAttendanceId { get; set; }
        public int UserId { get; set; }
        public bool IsPresent { get; set; }
        [MaxLength(100)]
        public string Remarks { get; set; }
        public TrainingAttendance TrainingAttendances { get; set; }
    }
}
