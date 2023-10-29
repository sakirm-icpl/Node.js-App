// ======================================
// <copyright file="Album.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Suggestion.API.Models
{
    public class MediaLibraryAlbum : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(500)]
        [Required]
        public string AlbumName { get; set; }
        public bool ShowToAll { get; set; }
    }
}
