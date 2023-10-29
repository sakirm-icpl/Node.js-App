using Courses.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IBasicAuthRepository: IRepository<BasicAuthCredentials>
    {
        Task<BasicAuthCredentials> AuthenticateApiToken(string apiToken);
        Task<BasicAuthCredentials> Authenticate(string userName, string password);
    }
}
