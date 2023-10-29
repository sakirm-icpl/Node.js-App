using CourseApplicability.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace CourseApplicability.API.Helper
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
    }
}