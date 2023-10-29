//======================================
// <copyright file="APIKeyAreaSetting.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIKeyAreaSetting
    {
        public int Id { get; set; }      
        public int KeyAreaUserId { get; set; }
        [MaxLength(200)]
        [Required]
        public string KeyResultAreaDescription { get; set; }
        [MaxLength(1000)]
        public string AdditionalDescription { get; set; }
        public string UserName { get; set; }

        public bool Status { get; set; }

        public bool IsAllowEdit { get; set; }
        public string AssignedByUserName { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
