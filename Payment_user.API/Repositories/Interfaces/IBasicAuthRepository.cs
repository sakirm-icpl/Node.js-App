//======================================
// <copyright file="IConfigure11.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Payment.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payment.API.Repositories.Interfaces
{
    public interface IBasicAuthRepository : IRepository<BasicAuthCredentials>
    {
        Task<BasicAuthCredentials> AuthenticateApiToken(string apiToken);
        Task<BasicAuthCredentials> Authenticate(string userName, string password);

    }
}
