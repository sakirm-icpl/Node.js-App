// ======================================
// <copyright file="KeyAreaSetting.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;


namespace User.API.Models
{
    public class KeyAreaSetting : CommonFields
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int KeyAreaUserId { get; set; }
        [MaxLength(200)]
        [Required]
        public string KeyResultAreaDescription { get; set; }
        [MaxLength(1000)]
        public string AdditionalDescription { get; set; }
    }
}
