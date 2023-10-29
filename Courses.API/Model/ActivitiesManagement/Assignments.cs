// ======================================
// <copyright file="Assignments.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.ActivitiesManagement
{
    public class Assignments : BaseModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [Required]
        [MaxLength(300)]
        public string Description { get; set; }
        [Required]
        [MaxLength(500)]
        public string PurposeOfExercise { get; set; }
        [Required]
        [MaxLength(500)]
        public string DesirableFormOutput { get; set; }
        public DateTime DateOfSubmission { get; set; }
        [MaxLength(2000)]
        public string ReferenceDocumentPath { get; set; }
        [MaxLength(2000)]
        public string AdditionalReferences { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
        public List<AssignmentsDetail> AssignmentsDetails { get; set; }
    }
}
