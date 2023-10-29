// ======================================
// <copyright file="TrainingExpenses.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.AdministrativeFunctions
{
    public class TrainingExpenses : BaseModel
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
        public List<TrainingExpensesDetail> TrainingExpensesDetails { get; set; }
    }
}
