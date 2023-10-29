// ======================================
// <copyright file="QuizzesManagement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizManagement.API.Models
{
    public class QuizzesManagement : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        public string QuizTitle { get; set; }
        public int TargetResponseCount { get; set; }
        public bool Status { get; set; }
        public bool IsApplicableToAll { get; set; }
        public List<QuizQuestionMaster> QuizQuestionMasters { get; set; }
    }
}
