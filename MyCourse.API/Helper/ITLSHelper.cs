using MyCourse.API.APIModel;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MyCourse.API.Helper
{
    public interface ITLSHelper
    {
        FileInfo GenerateExcelFile(string fileName, Dictionary<int, List<string>> excelData);
        //DataTable ToDataTableUserLearningReport<APIUserLearningReport>(IEnumerable<APIUserLearningReport> items);
        //DataTable ToDataTableUserLearningReportZoom(IEnumerable<ZoomParticipants> items, ZoomMeetingDetailsForReport zoomMeetingDetailsForReport);
        //FileInfo ToCSV(DataTable dtDataTable, string strFilePath);
    }
}
