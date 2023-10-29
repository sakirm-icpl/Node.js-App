//======================================
// <copyright file="APIMyPreferences.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.ComponentModel.DataAnnotations;
using User.API.Helper;
using User.API.Validation;

namespace User.API.APIModel.MyLearningSystem
{
    public class APIMyPreferences
    {
        public int? Id { get; set; }

        [MaxLength(100)]
        public string Profile { get; set; }

        [MaxLength(100)]
        public string Status { get; set; }

        [Required]
        [CheckValidationAttribute(AllowValue = new string[] { Record.LEARNING, Record.DASHBOARDV1, Record.DASHBOARDV2, Record.CATEGORISATION, Record.DYNAMIC_DASHBOARD, Record.GAMIFICATION, Record.SOCIAL, Record.ADMINDASHBOARD, Record.ADMINISTRATOR, Record.ANALYTICALDASHBOARD })]
        public string LandingPage { get; set; }

        [Required]
        public string Language { get; set; }

        public string Code { get; set; }
    }

    public class APIMyPreferencesMobile
    {
        public int? Id { get; set; }

        [MaxLength(100)]
        public string Profile { get; set; }

        [MaxLength(100)]
        public string Status { get; set; }

        public string LandingPage { get; set; }

        [Required]
        public string Language { get; set; }
        public string Code { get; set; }

    }
}
