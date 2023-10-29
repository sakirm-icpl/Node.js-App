using CourseReport.API.APIModel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace CourseReport.API.Helper.Interfaces
{
    public interface ITLSHelper
    {
        FileInfo GenerateExcelFile(string fileName, Dictionary<int, List<string>> excelData);
        FileInfo GenerateExcelFilewithFormatColumns(string fileName, Dictionary<int, List<string>> excelData, List<int> datecolumns);

        Task<FileInfo> GenerateBlobExcelFile(string fileName, Dictionary<int, List<string>> excelData);
        FileInfo ToCSV(DataTable dtDataTable, string strFilePath);

    }
}
