// ======================================
// <copyright file="CompetencyCategoryRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Competency.API.APIModel.Competency;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Competency.API.APIModel;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.IO;
using System.Text;
using Competency.API.Common;

namespace Competency.API.Repositories.Competency
{
    public class CompetencyCategoryRepository : Repository<CompetencyCategory>, ICompetencyCategoryRepository
    {
        StringBuilder sb = new StringBuilder();
        string[] header= { };
        string[] headerStar= { };
        string[] headerWithoutStar= { };
        List<string> CompetencyCategoryList = new List<string>();
        APICompetencyCategoryImport competencyCategoryImport = new APICompetencyCategoryImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;


        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencyCategoryRepository));
        private CourseContext db;
        private ICustomerConnectionStringRepository _customerConnection;
        private readonly IConfiguration _configuration;

        public CompetencyCategoryRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection, IConfiguration configuration) : base(context)
        {
            this.db = context;
            this._customerConnection = customerConnection;
            _configuration = configuration;
        }
        public async Task<IEnumerable<CompetencyCategory>> GetAllCompetencyCategory(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Model.Competency.CompetencyCategory> Query = this.db.CompetencyCategory;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => ((v.Category.StartsWith(search) || v.CategoryName.StartsWith(search)) && (v.IsDeleted == Record.NotDeleted)));
                    Query = Query.OrderByDescending(v => v.Id);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.CompetencyCategory.Where(r => ((r.CategoryName.StartsWith(search) || r.Category.StartsWith(search)) && (r.IsDeleted == Record.NotDeleted))).CountAsync();
            return await this.db.CompetencyCategory.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<bool> Exists(string Code, string category, int? categoryId = null)
        {
            Code = Code.ToLower().Trim();
            category = category.ToLower().Trim();
            int Count = 0;

            if (categoryId != null)
            {
                Count = await (from c in this.db.CompetencyCategory
                               where c.Id != categoryId && c.IsDeleted == false && (c.CategoryName.ToLower().Equals(Code) || c.Category.ToLower().Equals(category))
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this.db.CompetencyCategory
                               where c.IsDeleted == false && (c.CategoryName.ToLower().Equals(Code) || c.Category.ToLower().Equals(category))
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;

        }

        public bool IsDependacyExist(int categoryId)
        {
            int MasterCount = (from competenciesMaster in db.CompetenciesMaster
                               where (competenciesMaster.IsDeleted == false && competenciesMaster.CategoryId == categoryId)
                               select new { competenciesMaster.Id }).Count();
            if (MasterCount > 0)
                return true;

            int CountLevelCount = (from competencyLevels in db.CompetencyLevels
                                   where (competencyLevels.IsDeleted == false && competencyLevels.CategoryId == categoryId)
                                   select new { competencyLevels.Id }).Count();
            if (CountLevelCount > 0)
                return true;

            int CompetenciesMappingCount = (from competenciesMapping in db.CompetenciesMapping
                                            where (competenciesMapping.IsDeleted == false && competenciesMapping.CompetencyCategoryId == categoryId)
                                            select new { competenciesMapping.Id }).Count();

            if (CompetenciesMappingCount > 0)
                return true;

            return false;
        }
        public async Task<IEnumerable<CompetencyCategory>> Search(string query)
        {
            var competencyCategoryList = (from competencyCategory in this.db.CompetencyCategory
                                          where
                                          (competencyCategory.CategoryName.StartsWith(query) ||
                                          competencyCategory.Category.StartsWith(query)
                                          )
                                          && competencyCategory.IsDeleted == false
                                          select competencyCategory).ToListAsync();
            return await competencyCategoryList;
        }




        public async Task<List<APICompetencyChart>> GetCompetencySpiderchart(int UserId)
        {
            List<APICompetencyChart> CompetencyChartList = new List<APICompetencyChart>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetCompetencySpiderChartDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return null;
                            }

                            foreach (DataRow row in dt.Rows)
                            {
                                APICompetencyChart CompetencyChart = new APICompetencyChart();
                                CompetencyChart.CompetencyID = string.IsNullOrEmpty(row["CompetencyID"].ToString()) ? 0 : int.Parse(row["CompetencyID"].ToString());
                                CompetencyChart.CompetencyName = row["CompetencyName"].ToString();
                                CompetencyChart.AssessmentPercentage = string.IsNullOrEmpty(row["AssessmentPercentage"].ToString()) ? 0 : float.Parse(row["AssessmentPercentage"].ToString());


                                CompetencyChartList.Add(CompetencyChart);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return CompetencyChartList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


        #region Bulk Upload Competency Category

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APICompetencyCategoryImportColumns.CompetencyCode, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencyCategoryImportColumns.CompetencyName, 250));
            return columns;
        }
        public async Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                string filepath = sWebRootFolder + aPIDataMigration.Path;


                DataTable competencycategoryImportdt = ReadFile(filepath);
                if (competencycategoryImportdt == null || competencycategoryImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(competencycategoryImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(competencycategoryImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
                    Reset();
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
                    Reset();
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }

        public void Reset()
        {
            sb.Clear();
            header = new string[0];
            headerStar = new string[0];
            headerWithoutStar = new string[0];
            CompetencyCategoryList.Clear();

            sbError.Clear();
            totalRecordInsert = 0;
            totalRecordRejected = 0;
        }

        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filepath))
                {
                    pck.Load(stream);
                }
                    
                var ws = pck.Workbook.Worksheets.First();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    if (!dt.Columns.Contains(firstRowCell.Text))
                        dt.Columns.Add(firstRowCell.Text.Trim());
                }
                var startRow = 2;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    if (!string.IsNullOrEmpty(Convert.ToString(wsRow.ElementAtOrDefault(0))))
                    {
                        DataRow row = dt.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            if (!string.IsNullOrEmpty(cell.Text))
                                row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    else
                        break;
                }
            }
            //check for empty rows
            DataTable validDt = new DataTable();
            validDt = dt.Clone();
            foreach (DataRow dataRow in dt.Rows)
            {
                bool IsEmpty = true;
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dataRow[dataColumn])))
                    {
                        IsEmpty = false;
                        break;
                    }
                }
                if (!IsEmpty)
                    validDt.ImportRow(dataRow);
            }

            return validDt;
        }

        public async Task<bool> ValidateFileColumnHeaders(DataTable userImportdt, List<string> importColumns)
        {
            if (userImportdt.Columns.Count != importColumns.Count)
                return false;

            for (int i = 0; i < userImportdt.Columns.Count; i++)
            {
                string col = userImportdt.Columns[i].ColumnName.Replace("*", "").Replace(" ", "");
                userImportdt.Columns[i].ColumnName = col;

                if (!importColumns.Contains(userImportdt.Columns[i].ColumnName))
                    return false;
            }
            return true;
        }

        public async Task<ApiResponse> ProcessRecordsAsync(DataTable competencycategoryImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            List<APICompetencyCategoryImport> apiCompetencyCategoryImportRejected = new List<APICompetencyCategoryImport>();

            competencycategoryImportdt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = competencycategoryImportdt.Columns;
            foreach (string column in importcolumns)
            {
                competencycategoryImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = competencycategoryImportdt.Clone();
            DataTable newfinalDt = competencycategoryImportdt.Clone();
            

            if (competencycategoryImportdt != null && competencycategoryImportdt.Rows.Count > 0)
            {
                List<APICompetencyCategoryImport> apiCompetencyCategoryImportList = new List<APICompetencyCategoryImport>();

                foreach (DataRow dataRow in competencycategoryImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {

                        if (string.Compare(column,"CategoryCode",true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Category Code required";
                            }
                            
                        }
                        if (string.Compare(column, "CategoryName", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Category Name required";
                            }

                        }

                    }

                    /*if (await Exists(Convert.ToString(dataRow["CompetencyCode"]), Convert.ToString(dataRow["CompetencyName"])))*/
                    if (await Exists(Convert.ToString(dataRow[1]), Convert.ToString(dataRow[0])))
                    {
                        isError = true;
                        errorMsg = "Category Code or Category Name exists, duplicates not allowed";
                    }

                    if (isError)
                    {
/*                        competencyCategoryImport.CompetencyCode = dataRow["CompetencyCode"] != null ? Convert.ToString(dataRow["CategoryCode"]) : null;
                        competencyCategoryImport.CompetencyName = dataRow["CompetencyName"] != null ? Convert.ToString(dataRow["CategoryName"]) : null;
*/
                        competencyCategoryImport.CompetencyCode = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                        competencyCategoryImport.CompetencyName = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;

                        competencyCategoryImport.ErrMessage = errorMsg;
                        competencyCategoryImport.IsInserted = "false";
                        competencyCategoryImport.IsUpdated = "false";
                        competencyCategoryImport.InsertedID = null;
                        competencyCategoryImport.InsertedCode = "";
                        competencyCategoryImport.notInsertedCode = "";
                        dataRow[2] = competencyCategoryImport.ErrMessage;
                        apiCompetencyCategoryImportList.Add(competencyCategoryImport);
                    }
                    else
                    {
                        //totalRecordInsert++;
                        finalDt.ImportRow(dataRow);
                    }

                    competencyCategoryImport = new APICompetencyCategoryImport();
                    sb.Clear();

                }

                //Remove duplicates from the Excel file DataTable
                newfinalDt = removeDuplicatesRows(finalDt);
                totalRecordInsert = newfinalDt.Rows.Count;

                try
                {

                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }

                        DataTable dtResult = new DataTable();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "dbo.CompetencyCategory_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode",SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyCategoryBulkUpload_TVP", SqlDbType.Structured) { Value = newfinalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiCompetencyCategoryImportList.AddRange(dtResult.ConvertToList<APICompetencyCategoryImport>());
                        connection.Close();

/*                        if (apiCompetencyCategoryImportList != null)
                        {
                            if (apiCompetencyCategoryImportList.Where(x => x.ErrMessage==null).Count() > 0)
                            {

                            }
                        }*/
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiCompetencyCategoryImportList)
                {
                    if (!string.IsNullOrEmpty(data.CompetencyCode) || !string.IsNullOrEmpty(data.CompetencyName))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiCompetencyCategoryImportRejected.Add(data);
                        }
                        else
                        {
                            totalRecordInsert++;
                        }
                    }
                }

            }
            string resultstring = "Total number of records inserted : " + totalRecordInsert + ",  Total number of records rejected : " + totalRecordRejected + ". Duplicate entries were removed from the file";

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiCompetencyCategoryImportRejected };
            return response;

        }

        public DataTable removeDuplicatesRows(DataTable dt)
        {
            DataTable uniqueCols = dt.DefaultView.ToTable(true, "CategoryCode", "CategoryName","ErrorMessage");
            return uniqueCols;
        }

        #endregion

    }
}
