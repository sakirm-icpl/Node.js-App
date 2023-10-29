using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Repositories.Interfaces
{
    public interface IConfigurableParameterValuesRepository
    {
        Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string defaultValue = "");

        DataTable GetAllConfigurableParameterValue();
    }
}
