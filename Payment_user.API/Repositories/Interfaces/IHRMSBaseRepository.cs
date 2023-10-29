

using Payment.API.APIModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Payment.API.Repositories.Interfaces
{
    public interface IHRMSBaseRepository  //Repository<Configure5>, IConfigure5Repository
    {
        Task<IEnumerable<APIUserSettingNew>> GetUserSetting(string connectionString);
        Task<APIIsFileValid> ValidateFileColumnHeaders(DataTable userImportdt, string OrgCode, List<APIUserSettingNew> aPIUserSettings);
        DataTable ChangeRawTable(DataTable userImportdt, string OrgCode, Dictionary<String, string> initialColumns);
        Task<string> ProcessRecordsAsync(int UserId, DataTable userImportdt, string OrgCode, List<APIUserSettingNew> aPIUserSettings, string RandomPassword, string spName, string importTableName,
            //    //string IsActive = null,
            HRMSBasicData hRMSBasicData = null, string connectionString = null);
        Task<APIHRMSResponseNew> GetHRMSProcessStatus(DataTable dtTable, string connectionstring);
    }
}
