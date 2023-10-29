//======================================
// <copyright file="Roles.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class Roles : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }
        [MaxLength(50)]
        public string RoleCode { get; set; }
        public string RoleDescription { get; set; }
        public bool IsImplicitRole { get; set; }

    }
}
