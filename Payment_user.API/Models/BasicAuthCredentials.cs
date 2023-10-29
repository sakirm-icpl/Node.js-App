//======================================
// <copyright file="Configure1.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.ComponentModel.DataAnnotations;

namespace Payment.API.Models
{
    public class BasicAuthCredentials
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ApiToken { get; set; }
    }
}
