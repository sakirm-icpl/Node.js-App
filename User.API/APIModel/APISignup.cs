//======================================
// <copyright file="APISignup.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

namespace User.API.APIModel
{
    public class APISignup
    {
        public string ToEmail { get; set; }
        public string CustomerCode { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
    }
}
