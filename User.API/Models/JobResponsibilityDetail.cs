// ======================================
// <copyright file="JobResponsibilityDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class JobResponsibilityDetail : CommonFields
    {
        public int Id { get; set; }
        public int JobResponsibilityId { get; set; }
        [Required]
        [MaxLength(100)]
        public String JobDescription { get; set; }
        [MaxLength(300)]
        public String AdditionalDescription { get; set; }
    }
}
