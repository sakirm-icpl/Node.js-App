// ======================================
// <copyright file="CourseReview.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class CourseReview : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int RatingId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ReviewRating { get; set; }
        [Required]
        [MaxLength(500)]
        public string ReviewText { get; set; }
        [Required]
        [MaxLength(200)]
        public string UseName { get; set; }

    }
}
