// ======================================
// <copyright file="CompetencyCategory.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.Competency
{
    public class CompetencyCategory : BaseModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; }
        [Required]
        [MaxLength(100)]
        public string Category { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
