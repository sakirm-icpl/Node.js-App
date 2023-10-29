//======================================
// <copyright file="APIRoleResponsibility.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

namespace User.API.APIModel
{
    public class APIRoleResponsibility //: CommonFields
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ResponsibileUserId { get; set; }
        public string JobDescription { get; set; }
        public string AdditionalDescription { get; set; }
        public string UserName { get; set; }
        public string User { get; set; }
    }
}
