// ======================================
// <copyright file="APIPollsManagement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using PollManagement.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace PollManagement.API.APIModel
{
    public class APIPollsManagement//:CommonFields
    {
        public int? Id { get; set; }
        [DataType(DataType.Date)]
        [AllowFutureDtValidationAttribute("ValidityDate")]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime ValidityDate { get; set; }
        [Range(1, 10000, ErrorMessage = "Target Response Count must in the range of 1 to 10000")]
        public int TargetResponseCount { get; set; }
        [MaxLength(500)]
        [Required]
        [CSVInjection]
        public string Question { get; set; }
        [MaxLength(500)]
        [Required]
        [CSVInjection]
        public string Option1 { get; set; }
        [MaxLength(500)]
        [Required]
        [CSVInjection]
        public string Option2 { get; set; }
        [MaxLength(500)]
        [CSVInjection]
        public string Option3 { get; set; }
        [MaxLength(500)]
        [CSVInjection]
        public string Option4 { get; set; }
        [MaxLength(500)]
        [CSVInjection]
        public string Option5 { get; set; }
        [MaxLength(200)]
        public string ApplicabilityParameter { get; set; }
        public string ParameterValue { get; set; }
        public int? ParameterValueId { get; set; }
        public bool Status { get; set; }
        public string CompletionStatus { get; set; }
        public bool ValidTargetResponseCount { get; set; }
    }
    public class APIPollsResult //: CommonFields
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PollsId { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Option5 { get; set; }
    }


}
