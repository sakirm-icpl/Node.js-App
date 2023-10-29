//======================================
// <copyright file="PermissionMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class PermissionMaster : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public string Module { get; set; }
        public int sequence { get; set; }


    }
}
