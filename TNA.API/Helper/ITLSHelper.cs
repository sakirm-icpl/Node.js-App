using System.Collections.Generic;
using System.Data;
using System.IO;
using TNA.API.APIModel;

namespace TNA.API.Helper
{
    public interface ITLSHelper
    {
        FileInfo GenerateExcelFile(string fileName, Dictionary<int, List<string>> excelData);
        DataTable ToDataTableUserLearningReport<APIUserLearningReport>(IEnumerable<APIUserLearningReport> items);
        DataTable ToDataTableUserLearningReportZoom(IEnumerable<ZoomParticipants> items, ZoomMeetingDetailsForReport zoomMeetingDetailsForReport);
        FileInfo ToCSV(DataTable dtDataTable, string strFilePath);
    }
}
