using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using User.API.Helper.Interface;
using User.API.Services;

namespace User.API.Helper
{
    public class TLSHelper : ITLSHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IIdentityService _identityService;
        private IConfiguration _configuration;
        public TLSHelper(IWebHostEnvironment hostingEnvironment, IIdentityService identityService, IConfiguration configuration)
        {
            this._hostingEnvironment = hostingEnvironment;
            this._identityService = identityService;
            this._configuration = configuration;
        }
        public FileInfo GenerateExcelFile(string fileName, Dictionary<int, List<string>> excelData)
        {

            string OrgCode = Security.Decrypt(_identityService.GetCustomerCode());
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
    public static class Conversion
    {
        public static DataTable ToDataTable<T>(this List<T> iList)
        {
            DataTable dataTable = new DataTable();
            PropertyDescriptorCollection propertyDescriptorCollection =
                TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }
            object[] values = new object[propertyDescriptorCollection.Count];
            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static List<T> ConvertToList<T>(this DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row => {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch { }
                    }
                }
                return objT;
            }).ToList();
        }
    }
}
