using MyCourse.API.APIModel;
using MyCourse.API.Model;
//using MyCourse.API.Model.EdCastAPI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IDarwinboxConfiguration : IRepository<DarwinboxConfiguration>
    {
       Task<APIDarwinboxConfiguration> GetDarwinboxConfiguration();
    }
}
