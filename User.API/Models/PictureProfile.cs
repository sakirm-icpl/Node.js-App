//======================================
// <copyright file="PictureProfile.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class PictureProfile
    {
        public string Base64String { get; set; }
        [MaxLength(50)]
        public string CustomerName { get; set; }
        [MaxLength(50)]
        public string UserId { get; set; }
        public string url { get; set; }
    }
}
