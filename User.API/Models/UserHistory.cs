//======================================
// <copyright file="UserHistory.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class UserHistory : CommonFields
    {
        public int Id { get; set; }
        public int RowId { get; set; }
        [Required]
        public string Before { get; set; }
        [Required]
        public string After { get; set; }
        public DateTime Created { get; set; }
    }


}
