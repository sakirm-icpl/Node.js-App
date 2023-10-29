using Courses.API.APIModel;
using Courses.API.Model;
using Courses.API.Model.EdCastAPI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces.EdCast
{
    public interface IDarwinboxConfiguration : IRepository<DarwinboxConfiguration>
    {
       Task<APIDarwinboxConfiguration> GetDarwinboxConfiguration();
    }
}
