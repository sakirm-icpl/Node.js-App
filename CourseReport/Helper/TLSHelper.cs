using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using CourseReport.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using CourseReport.API.Service;
using CourseReport.API.Helper.Interfaces;

namespace CourseReport.API.Helper
{
    public class TLSHelper : ITLSHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IIdentityService _identityService;
        private IConfiguration _configuration;
        private ICalendarRepository _calendarRepository;
        IAzureStorage _azurestorage;
        public TLSHelper(IWebHostEnvironment hostingEnvironment, IAzureStorage azurestorage,ICalendarRepository calendarRepository, IIdentityService identityService, IConfiguration configuration)
        {
            this._hostingEnvironment = hostingEnvironment;
            this._identityService = identityService;
            this._configuration = configuration;
            this._calendarRepository = calendarRepository;
            this._azurestorage = azurestorage;
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
                    ExcelWorksheet WorkSheet = package.Workbook.Worksheets.Add(fileName);
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

                    using (ExcelRange rngitems = WorkSheet.Cells["A1:DA1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                        //rngitems.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //rngitems.Style.Fill.BackgroundColor.SetColor(Color.DimGray);
                        rngitems.Style.Font.Size = 12;
                        rngitems.AutoFitColumns();
                    }
                    package.Save();
                }
                return file;
           
        }

        public async Task<FileInfo> GenerateBlobExcelFile(string fileName, Dictionary<int, List<string>> excelData)
        {
            string OrgCode = Security.Decrypt(_identityService.GetOrgCode());
            string EnableBlobStorage = await _calendarRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                string WwwRootFolder = this._configuration["ApiGatewayWwwroot"];
                WwwRootFolder = Path.Combine(WwwRootFolder, OrgCode);
                if (!Directory.Exists(WwwRootFolder))
                {
                    Directory.CreateDirectory(WwwRootFolder);
                }
                string FileName = DateTime.Now.Ticks+fileName;
                FileInfo file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(WwwRootFolder, FileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet WorkSheet = package.Workbook.Worksheets.Add(fileName);
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

                    using (ExcelRange rngitems = WorkSheet.Cells["A1:DA1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                        //rngitems.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //rngitems.Style.Fill.BackgroundColor.SetColor(Color.DimGray);
                        rngitems.Style.Font.Size = 12;
                        rngitems.AutoFitColumns();
                    }
                    package.Save();
                }
                return file;
            }
            else
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet WorkSheet = package.Workbook.Worksheets.Add(fileName);
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

                    using (ExcelRange rngitems = WorkSheet.Cells["A1:DA1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                        //rngitems.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //rngitems.Style.Fill.BackgroundColor.SetColor(Color.DimGray);
                        rngitems.Style.Font.Size = 12;
                        rngitems.AutoFitColumns();
                    }
                    var bytes = package.GetAsByteArray();
                    BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode,null,null,"xlsx");
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            string file = res.Blob.Name.ToString();

                            FileInfo fileInfo = new FileInfo(res.Blob.Uri);
                            return fileInfo;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }               
            }
            return null;
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
            //headers    
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
                    else
                    {
                        sw.Write("-");
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

        public FileInfo GenerateExcelFilewithFormatColumns(string fileName, Dictionary<int, List<string>> excelData, List<int> datecolumns)
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
                ExcelWorksheet WorkSheet = package.Workbook.Worksheets.Add(fileName);
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
                foreach (int dc in datecolumns)
                {
                    WorkSheet.Column(dc).Style.Numberformat.Format = "dd-MM-yyyy";
                }

                using (ExcelRange rngitems = WorkSheet.Cells["A1:DA1"])//Applying Css for header
                {

                    rngitems.Style.Font.Bold = true;
                    //rngitems.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //rngitems.Style.Fill.BackgroundColor.SetColor(Color.DimGray);
                    rngitems.Style.Font.Size = 12;
                    rngitems.AutoFitColumns();
                }
                package.Save();
            }
            return file;
        }
    }
}
