// ======================================
// <copyright file="MediaLibrary.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Gadget.API.Models
{
    public class MediaLibrary : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(500)]
        [Required]
        public string Album { get; set; }
        [MaxLength(500)]
        [Required]
        public string FileType { get; set; }
        public int AlbumId { get; set; }
        public int ObjectId { get; set; }
        [MaxLength(100)]
        [Required]
        public string ObjectTitle { get; set; }
        [MaxLength(2000)]
        [Required]
        public string FilePath { get; set; }
        [MaxLength(20)]
        public string Keywords { get; set; }
        public int LikesCount { get; set; }
        [MaxLength(1000)]
        public string Metadata { get; set; }
    }
}
