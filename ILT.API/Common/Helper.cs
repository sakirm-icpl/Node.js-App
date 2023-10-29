using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using ILT.API.Helper;
using log4net;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace ILT.API.Common
{
    public class Helper
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Helper));
        public async Task<bool> SaveFile(IFormFile uploadedFile, string filePath)
        {
            try
            {
                using (var fs = new FileStream(Path.Combine(filePath), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fs);
                }
                return true;
            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
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
                        catch{ }
                    }
                }
                return objT;
            }).ToList();
        }
    }
}


