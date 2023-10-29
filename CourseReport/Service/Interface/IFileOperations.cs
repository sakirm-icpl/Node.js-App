using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.Service.Interface
{
    public interface IFileOperations
    {
        Task<byte[]> ExportExcel<T>(List<T> dataList, string FileName, Dictionary<string, string> requiredColumns);
    }
}
