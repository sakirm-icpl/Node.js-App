// ======================================
// <copyright file="RoleResponsibility.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class RoleResponsibility : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string UserId { get; set; }
        [Required]
        public int ResponsibileUserId { get; set; }
        [MaxLength(200)]
        public string SerialNo { get; set; }
        [MaxLength(200)]
        [Required]
        public string JobDescription { get; set; }
        [MaxLength(1000)]
        public string AdditionalDescription { get; set; }

    }
}
