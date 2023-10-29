
using Courses.API.APIModel;
using Courses.API.APIModel.ILT;
using Courses.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace Courses.API.Helper
{
    public class TLSHelper : ITLSHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        IIdentityService _identityService;
        public IConfiguration _configuration { get; }
        public TLSHelper(IWebHostEnvironment hostingEnvironment, IIdentityService identityService, IConfiguration configuration)
        {
            this._hostingEnvironment = hostingEnvironment;
            this._identityService = identityService;
            this._configuration = configuration;
        }
        public FileInfo GenerateExcelFile(string fileName, Dictionary<int, List<string>> excelData)
        {

            string OrgCode = Security.Decrypt(_identityService.GetOrgCode());
            string WwwRootFolder = this._configuration["ApiGatewayWwwroot"];
            WwwRootFolder = Path.Combine(WwwRootFolder, OrgCode);
            if (!Directory.Exists(WwwRootFolder))
            {
                Directory.CreateDirectory(WwwRootFolder);
            }
            string FileName = fileName;
            FileInfo file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                ExcelWorksheet WorkSheet = package.Workbook.Worksheets.Add("temp");
                for (int rowNumber = 0; rowNumber < excelData.Count; rowNumber++)
                {
                    List<string> Row = excelData[rowNumber];
                    int ColumNumber = 1;
                    foreach (string column in Row)
                    {
                        WorkSheet.Cells[rowNumber + 1, ColumNumber].Value = column;
                        ColumNumber++;
                    }

                }

                using (var rngitems = WorkSheet.Cells["A1:BH1"])//Applying Css for header
                {
                    rngitems.Style.Font.Bold = true;
                    rngitems.Style.Font.Size = 12;
                    rngitems.AutoFitColumns();
                }
                package.Save();
            }
            return file;
        }
        public DataTable ToDataTableUserLearningReport<ExportTeamsMeetingReport>(IEnumerable<ExportTeamsMeetingReport> items)
        {
            var tb = new DataTable(typeof(ExportTeamsMeetingReport).Name);

            PropertyInfo[] props = typeof(ExportTeamsMeetingReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); 
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }
                
            tb.Columns["DisplayName"].ColumnName = "DisplayName";
            tb.Columns["emailAddress"].ColumnName = "EmailId";
            tb.Columns["role"].ColumnName = "Role";
            tb.Columns["meetingStartDateTime"].ColumnName = "MeetingStartDateTime";
            tb.Columns["joinDateTime"].ColumnName = "JoinDateTime";
            tb.Columns["meetingEndDateTime"].ColumnName = "MeetingEndDateTime";
            tb.Columns["leaveDateTime"].ColumnName = "LeaveDateTime";

            return tb;
        }
        public DataTable ToDataTableUserLearningReportZoom(IEnumerable<ZoomParticipants> items, ZoomMeetingDetailsForReport zoomMeetingDetailsForReport)
        {
            var tb = new DataTable(typeof(ExportZoomMeetingReport).Name);

            PropertyInfo[] props = typeof(ExportZoomMeetingReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            
            List<ExportZoomMeetingReport> exportZoomMeetingReports = new List<ExportZoomMeetingReport>();
            

            foreach(var item in items)
            {
                ExportZoomMeetingReport exportZoomMeetingReport = new ExportZoomMeetingReport();
                exportZoomMeetingReport.end_time = zoomMeetingDetailsForReport.end_time;
                exportZoomMeetingReport.join_time = item.join_time;
                exportZoomMeetingReport.leave_time = item.leave_time;
                exportZoomMeetingReport.name = item.name;
                exportZoomMeetingReport.start_time = zoomMeetingDetailsForReport.start_time;
                exportZoomMeetingReport.user_email = item.user_email;
                exportZoomMeetingReport.status = item.status;

                exportZoomMeetingReports.Add(exportZoomMeetingReport);
            }

            foreach (var item in exportZoomMeetingReports)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["name"].ColumnName = "Name";
            tb.Columns["user_email"].ColumnName = "EmailId";
            tb.Columns["start_time"].ColumnName = "MeetingStartDateTime";
            tb.Columns["join_time"].ColumnName = "JoinDateTime";
            tb.Columns["end_time"].ColumnName = "MeetingEndDateTime";
            tb.Columns["leave_time"].ColumnName = "LeaveDateTime";
            tb.Columns["status"].ColumnName = "Status";
            return tb;
        }
        public FileInfo ToCSV(DataTable dtDataTable, string strFilePath)
        {
            string OrgCode = Security.Decrypt(_identityService.GetOrgCode());
            string WwwRootFolder = this._configuration["ApiGatewayWwwroot"];
            WwwRootFolder = Path.Combine(WwwRootFolder, OrgCode);
            if (!Directory.Exists(WwwRootFolder))
            {
                Directory.CreateDirectory(WwwRootFolder);
            }
            string FileName = strFilePath;
            FileInfo file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
            }
            StreamWriter sw = new StreamWriter(Path.Combine(WwwRootFolder, FileName), false);
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
            return file;
        }
    }
}
