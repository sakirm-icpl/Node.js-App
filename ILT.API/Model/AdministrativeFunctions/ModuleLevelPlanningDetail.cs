// ======================================
// <copyright file="ModuleLevelPlanningDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.AdministrativeFunctions
{
    public class ModuleLevelPlanningDetail : BaseModel
    {
        public int Id { get; set; }
        public int TrainingPlaceId { get; set; }
        public int ModuleLevelPlanningId { get; set; }
        public DateTime StartDate { get; set; }
        [Required]
        [MaxLength(10)]
        public string StartTime { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        [MaxLength(10)]
        public string EndTime { get; set; }
        public int CoTrainerId { get; set; }
        public int HRCoOrdinatorId { get; set; }
        public ModuleLevelPlanning ModuleLevelPlannings { get; set; }
    }
}
