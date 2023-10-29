//======================================
// <copyright file="Configure1.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class Configure1
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }
    }
}
