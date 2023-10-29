//======================================
// <copyright file="RoleAuthority.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class RoleAuthority : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int RoleId { get; set; }
        [Required]
        public int PermissionId { get; set; }
        public bool IsAccess { get; set; }
    }
}
