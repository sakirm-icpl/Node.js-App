using MyCourse.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories
{
    public interface IBasicAuthRepository: IRepository<BasicAuthCredentials>
    {
        Task<BasicAuthCredentials> AuthenticateApiToken(string apiToken);
        Task<BasicAuthCredentials> Authenticate(string userName, string password);
    }
}
