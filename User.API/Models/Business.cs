//======================================
// <copyright file="Business.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class Business
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(50)]
        public string Theme { get; set; }
        [MaxLength(100)]
        public string LogoName { get; set; }

    }
    public class APIGetBusinessDetails
    {
        public int UserId { get; set; }
        public int? BusinessId { get; set; }
        public string BusinesName { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }

    }

    public class APIBusinessDetails
    {
        public int UserId { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
    }
    public class BusinessDetails
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BusinessId { get; set; }
        public bool Isdeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class DecryptedValues
    {
        public string[] value { get; set; }
    }
}
