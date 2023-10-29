// ======================================
// <copyright file="ThoughtForDayCounter.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;

namespace Publication.API.Models
{
    public class ThoughtForDayCounter : CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public bool UserAction { get; set; }
        public int ThoughtForDayId { get; set; }
    }
}
