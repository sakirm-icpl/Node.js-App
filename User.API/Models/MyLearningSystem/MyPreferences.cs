//======================================
// <copyright file="MyPreferences.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.ComponentModel.DataAnnotations;

namespace User.API.Models.MyLearningSystem
{
    public class MyPreferences : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Profile { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
        [MaxLength(50)]
        public string LandingPage { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
        [MaxLength(50)]
        public string Code { get; set; }
    }

}
