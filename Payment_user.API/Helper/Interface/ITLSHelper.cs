using System.Collections.Generic;
using System.IO;

namespace Payment.API.Helper.Interface
{
    public interface ITLSHelper
    {
        FileInfo GenerateExcelFile(string fileName, Dictionary<int, List<string>> excelData);
    }
}
