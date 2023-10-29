using Courses.API.APIModel;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace CourseApplicability.API.Helper
{
    public interface ITLSHelper
    {
        FileInfo GenerateExcelFile(string fileName, Dictionary<int, List<string>> excelData);
    }
}

