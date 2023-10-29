// ======================================
// <copyright file="CompetencyLevel.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.Competency
{
    public class CompetencyLevels : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? SubSubCategoryId { get; set; }
        [Required]
        public int CompetencyId { get; set; }
        [Required]
        [MaxLength(100)]
        public string LevelName { get; set; }
        [Required]
        [MaxLength(200)]
        public string BriefDescriptionCompetencyLevel { get; set; }
        [MaxLength(200)]
        public string DetailedDescriptionOfLevel { get; set; }
    }
}
