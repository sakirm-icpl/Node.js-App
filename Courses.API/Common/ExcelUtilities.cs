using Courses.API.Helper;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Courses.API.Common
{
    public class ExcelUtilities
    {
        IConfiguration _configuration;
        public ExcelUtilities(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DataTable ReadFromExcel(string path, bool hasHeader = true)
        {
            using (var excelPack = new ExcelPackage())
            {
                //Load excel stream
                using (var stream = File.OpenRead(path))
                {
                    excelPack.Load(stream);
                }

                var ws = excelPack.Workbook.Worksheets[1];

                //Get all details as DataTable 
                DataTable excelData = new DataTable();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    //Get colummn details
                    if (!string.IsNullOrEmpty(firstRowCell.Text))
                    {
                        string firstColumn = string.Format("Column {0}", firstRowCell.Start.Column);
                        excelData.Columns.Add(hasHeader ? firstRowCell.Text.Trim() : firstColumn.Trim());
                    }
                }
                var startRow = hasHeader ? 2 : 1;
                //Get row details
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, excelData.Columns.Count];
                    DataRow row = excelData.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        if (!FileValidation.CheckForSQLInjection(cell.Text.Trim()))
                            row[cell.Start.Column - 1] = cell.Text.Trim();
                    }
                }

                return StripEmptyRows(excelData);
            }
        }

        private DataTable StripEmptyRows(DataTable dt)
        {
            List<int> rowIndexesToBeDeleted = new List<int>();
            int indexCount = 0;
            foreach (var row in dt.Rows)
            {
                var r = (DataRow)row;
                int emptyCount = 0;
                int itemArrayCount = r.ItemArray.Length;
                foreach (var i in r.ItemArray) if (string.IsNullOrWhiteSpace(i.ToString())) emptyCount++;

                if (emptyCount == itemArrayCount) rowIndexesToBeDeleted.Add(indexCount);

                indexCount++;
            }

            int count = 0;
            foreach (var i in rowIndexesToBeDeleted)
            {
                dt.Rows.RemoveAt(i - count);
                count++;
            }

            return dt;
        }

        public FileInfo WriteDataTableToExcel(DataTable dataTable, string OrgCode, string fileName)
        {
            FileInfo excelFileInfo = null;

            string sWebRootFolder = _configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string backUpFilePath = sWebRootFolder + @"\CourseApplicabilityImport\";
            string backUpFileName = Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")
                                 + Path.GetExtension(fileName);

            using (ExcelPackage package = new ExcelPackage())
            {
                string workSheetName = "Import Status";
                ExcelWorksheet ws = package.Workbook.Worksheets.Add(workSheetName);
                ws.Cells["A1"].LoadFromDataTable(dataTable, true);

                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, fileName));
                excelFileInfo = new FileInfo(file.FullName);
                if (excelFileInfo.Exists)
                {
                    excelFileInfo.Delete();
                    excelFileInfo = new FileInfo(file.FullName);
                }
                package.SaveAs(excelFileInfo);
                if (!Directory.Exists(backUpFilePath))
                    Directory.CreateDirectory(backUpFilePath);
                File.Copy(excelFileInfo.FullName, backUpFilePath + backUpFileName);
            }
            return excelFileInfo;
        }
    }
}
