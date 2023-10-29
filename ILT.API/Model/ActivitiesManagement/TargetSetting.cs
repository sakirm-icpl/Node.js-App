// ======================================
// <copyright file="TargetSetting.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.ActivitiesManagement
{
    public class TargetSetting : BaseModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [MaxLength(500)]
        public string TargetDescription { get; set; }
        [Required]
        [MaxLength(50)]
        public string FrequencyOfAssessment { get; set; }
        public DateTime DateOfAssessment { get; set; }
        [MaxLength(20)]
        public string Status { get; set; }
    }
}
