// ======================================
// <copyright file="PollsManagement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PollManagement.API.Models
{
    public class PollsManagement : CommonFields
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ValidityDate { get; set; }
        public int TargetResponseCount { get; set; }
        [MaxLength(500)]
        [Required]
        public string Question { get; set; }
        [MaxLength(500)]
        [Required]
        public string Option1 { get; set; }
        [MaxLength(500)]
        [Required]
        public string Option2 { get; set; }
        [MaxLength(500)]
        public string Option3 { get; set; }
        [MaxLength(500)]
        public string Option4 { get; set; }
        [MaxLength(500)]
        public string Option5 { get; set; }
        public bool Status { get; set; }
        public bool IsApplicableToAll { get; set; }

    }
}
