//======================================
// <copyright file="APIRoleResponsibility.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System;
using User.API.Models;

namespace User.API.APIModel
{
    public class APIJobResponsibility //: CommonFields
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public int ResponsibileUserId { get; set; }
        public string UserName { get; set; }
        public string User { get; set; }
        public String JobDescription { get; set; }
        public String AdditionalDescription { get; set; }
        public int JobResponsibilityDetailId { get; set; }
        public int JobResponsibilityId { get; set; }
        public DateTime Date { get; set; }
    }
    public class APIJobResponsibilityDetail //: CommonFields
    {
        public int? Id { get; set; }
        public int JobResponsibilityId { get; set; }
        public String JobDescription { get; set; }
        public String AdditionalDescription { get; set; }

    }

    public class APIJobResponsibilityMerge : CommonFields
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public int ResponsibileUserId { get; set; }
        public APIJobResponsibilityDetail[] APIJobResponsibilityDetails { get; set; }
    }
}
