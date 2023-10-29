using Courses.API.Helper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;


namespace Assessment.API.Helper
{
    public class Api
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Api));
        private IConfiguration _configuration { get; }
        private string apiKey;
        private string apiSecret;
        private string dataType;
        private string url;
        private string hostId;
        private List<KeyValuePair<string, string>> apiKeys = null;

        public Api(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.apiKey = this._configuration[Configuration.ApiKey];
            this.apiSecret = this._configuration[Configuration.ApiSecret];
            this.url = this._configuration[Configuration.ApiUrl];
            this.hostId = this._configuration[Configuration.HostId];
            this.dataType = Configuration.DataType;
            this.apiKeys = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Configuration.ApiKey, this.apiKey),
                new KeyValuePair<string, string>(Configuration.ApiSecret, this.apiSecret),
                new KeyValuePair<string, string>(Configuration.DataType, this.dataType)
            };
        }

        public async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = this.url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }
        }

        public static DataTable JsonStringToDataTable(string jsonString)
        {
            DataTable dt = new DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");
            List<string> ColumnsName = new List<string>();
            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {
                        int idx = ColumnsNameData.IndexOf(":");
                        string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                        if (!ColumnsName.Contains(ColumnsNameString.Trim()))
                        {
                            ColumnsName.Add(ColumnsNameString.Trim());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));
                    }
                }

                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
    }
}
