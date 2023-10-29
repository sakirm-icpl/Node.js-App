// ======================================
// <copyright file="TrainingAttendance.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.Collections.Generic;

namespace Courses.API.Model.TrainersFunctions
{
    public class TrainingAttendance : BaseModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public int SessionId { get; set; }
        public List<TrainingAttendanceDetail> TrainingAttendanceDetails { get; set; }
    }
}
