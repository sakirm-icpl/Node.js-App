// ======================================
// <copyright file="ProjectMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Gadget.API.Models
{
    public class ProjectMaster : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(500)]
        [Required]
        public Guid RowGuid { get; set; }

        public bool IsTeamEntry { get; set; }
        public int? TeamSize { get; set; }
        [MaxLength(2000)]
        [Required]
        public string Answer1 { get; set; }
        [MaxLength(2000)]
        [Required]
        public string Answer2 { get; set; }
        [MaxLength(2000)]
        //[Required]
        public string Answer3 { get; set; }

        [MaxLength(1000)]
       // [Required]
        public string CategoryCode { get; set; }
    }
}
