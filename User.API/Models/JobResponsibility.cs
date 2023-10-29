// ======================================
// <copyright file="RoleResponsibility.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

namespace User.API.Models
{
    public class JobResponsibility : CommonFields
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ResponsibileUserId { get; set; }

    }
}
