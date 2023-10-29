// ======================================
// <copyright file="PollsResult.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace PollManagement.API.Models
{
    public class PollsResult : CommonFields
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PollsId { get; set; }
        [MaxLength(500)]
        public string Option1 { get; set; }
        [MaxLength(500)]
        public string Option2 { get; set; }
        [MaxLength(500)]
        public string Option3 { get; set; }
        [MaxLength(500)]
        public string Option4 { get; set; }
        [MaxLength(500)]
        public string Option5 { get; set; }
    }
}
