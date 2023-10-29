// ======================================
// <copyright file="JobAid.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class JobAid : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string ContentId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Title { get; set; }
        [MaxLength(50)]
        public string FileType { get; set; }
        [MaxLength(2000)]
        public string AdditionalDescription { get; set; }
        [MaxLength(2000)]
        [Required]
        public string Content { get; set; }
        [MaxLength(1000)]
        public string KeywordForSearch { get; set; }
    }
}
