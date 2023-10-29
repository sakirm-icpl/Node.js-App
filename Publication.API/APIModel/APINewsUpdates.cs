// ======================================
// <copyright file="APINewsUpdates.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Publication.API.APIModel
{
    public class APINewsUpdates //: CommonFields
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime ValidityDate { get; set; }
        [MaxLength(100)]
        [Required]
        [CSVInjection]
        public string Headline { get; set; }
        [MaxLength(500)]
        public string SubHead { get; set; }
        [MaxLength(2000)]
        public string ImagePath { get; set; }
        [MaxLength(2000)]
        public string VideoPath { get; set; }
        public string DetailDescription { get; set; }
        [MaxLength(1000)]
        public string Source { get; set; }
        public int ClicksCount { get; set; }
    }

    public class APINewsUpdatesReward
    {
        public int? Id { get; set; }
        public string Headline { get; set; }
    }
}
