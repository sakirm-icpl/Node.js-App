// ======================================
// <copyright file="QuizQuestionMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizManagement.API.Models
{
    public class QuizQuestionMaster : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(500)]
        [Required]
        public string Question { get; set; }
        [MaxLength(2000)]
        public string PicturePath { get; set; }
        public int QuizId { get; set; }
        [MaxLength(1000)]
        public string Hint { get; set; }
        public bool AnswersArePictures { get; set; }
        public bool RandomizeSequence { get; set; }
        public int Mark { get; set; }
        public QuizzesManagement QuizzesManagements { get; set; }
        public List<QuizOptionMaster> QuizOptionMasters { get; set; }
    }
}
