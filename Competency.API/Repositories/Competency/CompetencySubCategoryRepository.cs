
using Competency.API.APIModel;
using Competency.API.APIModel.Competency;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Repositories.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Competency.API.Model.ResponseModels;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using Competency.API.Common;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Competency.API.Model;

namespace Competency.API.Repositories.Competency
{
    public class CompetencySubCategoryRepository : Repository<CompetencySubCategory>, ICompetencySubCategoryRepository
    {
        StringBuilder sb = new StringBuilder();
        string[] header = { };
        string[] headerStar = { };
        string[] headerWithoutStar = { };
        List<string> CompetencySubCategoryList = new List<string>();
        APICompetencySubCategoryImport competencySubCategoryImport = new APICompetencySubCategoryImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencySubCategoryRepository));
        private CourseContext db;
        private ICustomerConnectionStringRepository _customerConnection;
        private readonly IConfiguration _configuration;


        public CompetencySubCategoryRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection, IConfiguration configuration) : base(context)
        {
            this.db = context;
            this._customerConnection = customerConnection;
            _configuration = configuration;

        }

        public  async Task<IApiResponse> GetAllCompetencySubCategory(int page, int pageSize, int categoryId = 0, string search = null)
        {
            try
            {

                IQueryable<CompetencySubCategoryResult> Query = null;

              

                if (categoryId!= 0)
                {
                    Query = (from compsub in db.CompetencySubCategory 
                       join compcat in db.CompetencyCategory on compsub.CategoryId equals compcat.Id

                       where compsub.IsDeleted == false && compcat.IsDeleted == false
                       && compsub.CategoryId == categoryId
                         select new CompetencySubCategoryResult
                         {
                             Id = compsub.Id,
                             CategoryId = compsub.CategoryId,
                             CategoryName = compcat.Category,
                             SubcategoryCode = compsub.SubcategoryCode,
                             SubcategoryDescription = compsub.SubcategoryDescription,
                             IsDeleted = compsub.IsDeleted,
                         });

                   
                }
                else
                {
                    Query = (from compsub in db.CompetencySubCategory
                             join compcat in db.CompetencyCategory on compsub.CategoryId equals compcat.Id

                             where compsub.IsDeleted == false && compcat.IsDeleted == false
                             select new CompetencySubCategoryResult
                             {
                                 Id = compsub.Id,
                                 CategoryId = compsub.CategoryId,
                                 CategoryName = compcat.Category,
                                 SubcategoryCode = compsub.SubcategoryCode,
                                 SubcategoryDescription = compsub.SubcategoryDescription,
                                 IsDeleted = compsub.IsDeleted,
                             });
                }

                if(Query == null)
                {
                    return new APIResposeNo { };
                }else
                {
                    
                

                //(from CompetencySubCategory in db.CompetencySubCategory
                //             where (CompetencySubCategory.IsDeleted == false && CompetencySubCategory.CategoryId == categoryId)
                //             select new { competenciesMaster.Id })


                APIResponse<CompetencySubCategoryResult> result = new APIResponse<CompetencySubCategoryResult>();


                if (search == "null")
                {
                    search = null;
                }
                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => ((v.SubcategoryDescription.StartsWith(search) || v.SubcategoryCode.StartsWith(search)) && (v.IsDeleted == Record.NotDeleted)));
                    Query = Query.OrderByDescending(v => v.Id);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                Query = Query.OrderByDescending(v => v.Id);

                result.Data.RecordCount = await Query.CountAsync();

                if (page != -1)
                {

                    Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
                    Query = Query.OrderByDescending(v => v.Id);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(Convert.ToInt32(pageSize));
                    Query = Query.OrderByDescending(v => v.Id);
                }

                //List<CompetencySubCategoryResult> CompetencySubCategoryResult = new List<CompetencySubCategoryResult>();
                //if (result.Data.RecordCount > 0)
                //{
                //    foreach (var item in Query)
                //    {
                //        CompetencySubCategoryResult competencySubCategory = new CompetencySubCategoryResult();
                //        competencySubCategory.Id = item.Id;
                //        competencySubCategory.CategoryId = item.CategoryId;
                //        competencySubCategory.CategoryName = item.CategoryName;
                //        competencySubCategory.SubcategoryCode = item.SubcategoryCode;
                //        competencySubCategory.SubcategoryDescription = item.SubcategoryDescription;
                //        CompetencySubCategoryResult.Add(competencySubCategory);
                //    }

                //}
                if (result.Data.RecordCount > 0)
                {
                    result.Data.Records = Query.ToList();
                    result.Message = "List of Competency Subcategories";
                }else
                {
                    result.Data.Records = null;
                    result.Message = "No Records Found";
                    result.StatusCode = 204;
                }
               


                return  result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<bool> Exists(string Code, string subcategory )
        {
            Code = Code.ToLower().Trim();
            subcategory = subcategory.ToLower().Trim();
            int Count = 0;

          
                Count = await (from c in this.db.CompetencySubCategory
                               where c.IsDeleted == false && ( ( c.SubcategoryCode.ToLower().Equals(Code) || c.SubcategoryDescription.ToLower().Equals(subcategory)))
                               select new
                               { c.Id }).CountAsync();
            

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

        public async Task<IApiResponse> Search(int catID, string query)
        {
           
            if (query== null || query.Equals("null"))
            {

                //var competencySubCategoryList = (from competencySubCategory in this.db.CompetencySubCategory
                //                                 where (competencySubCategory.IsDeleted == false && competencySubCategory.CategoryId.Equals(catID))
                //                                 select competencySubCategory).ToListAsync();

                var competencySubCategoryList = db.CompetencySubCategory.Join(db.CompetencyCategory, csub => csub.CategoryId, (ccat => ccat.Id), (csub, ccat) => new
                {
                    IsDeleted = csub.IsDeleted,
                    //ModifiedBy = csub.ModifiedBy,
                    //CreatedBy = csub.CreatedBy,
                    Id = csub.Id,
                    CategoryId = csub.CategoryId,
                    CategoryName = ccat.CategoryName,
                    SubcategoryCode = csub.SubcategoryCode,
                    SubcategoryDescription = csub.SubcategoryDescription,
                    //IsActive = csub.IsActive,
                }).Where(
                   csub => (csub.IsDeleted == false && csub.CategoryId.Equals(catID))
                   ).OrderByDescending(csub => csub.Id).Select(csub => new CompetencySubCategoryResult
                   {
                       Id = csub.Id,
                       CategoryId = csub.CategoryId,
                       SubcategoryCode = csub.SubcategoryCode,
                       CategoryName = csub.CategoryName,
                       SubcategoryDescription = csub.SubcategoryDescription,
                       IsDeleted = csub.IsDeleted,
                   });

                APIResponse<CompetencySubCategoryResult> result = new APIResponse<CompetencySubCategoryResult>();
                List<CompetencySubCategoryResult> CompetencySubCategoryResult = new List<CompetencySubCategoryResult>();
                if (competencySubCategoryList.Count() > 0)
                {
                    foreach (var item in competencySubCategoryList)
                    {
                        CompetencySubCategoryResult competencySubCategory = new CompetencySubCategoryResult();
                        competencySubCategory.Id = item.Id;
                        competencySubCategory.CategoryId = item.CategoryId;
                        competencySubCategory.CategoryName = item.CategoryName;
                        competencySubCategory.SubcategoryCode = item.SubcategoryCode;
                        competencySubCategory.SubcategoryDescription = item.SubcategoryDescription;
                        CompetencySubCategoryResult.Add(competencySubCategory);
                    }

                }

                result.Data.Records = CompetencySubCategoryResult;
                result.Data.RecordCount = CompetencySubCategoryResult.Count();
                return result;
            }
            else
            {

                //&& competencySubCategory.CategoryId.Equals(catID)
                //var competencySubCategoryList = (from competencySubCategory in this.db.CompetencySubCategory
                //                                 where
                //                                 (competencySubCategory.SubcategoryCode.StartsWith(query) ||competencySubCategory.SubcategoryDescription.StartsWith(query)
                //                                 && (competencySubCategory.CategoryId.Equals(catID) && competencySubCategory.IsDeleted == false))
                //                                 select competencySubCategory).ToListAsync();

                var competencySubCategoryList = db.CompetencySubCategory.Join(db.CompetencyCategory, csub => csub.CategoryId, (ccat => ccat.Id), (csub, ccat) => new
                {
                    IsDeleted = csub.IsDeleted,
                    //ModifiedBy = csub.ModifiedBy,
                    //CreatedBy = csub.CreatedBy,
                    Id = csub.Id,
                    CategoryId = csub.CategoryId,
                    CategoryName = ccat.CategoryName,
                    SubcategoryCode = csub.SubcategoryCode,
                    SubcategoryDescription = csub.SubcategoryDescription,
                    //IsActive = csub.IsActive,
                }).Where(
                  csub => (csub.SubcategoryCode.StartsWith(query) || csub.SubcategoryDescription.StartsWith(query) && ( csub.CategoryId.Equals(catID) && csub.IsDeleted == false))
                  ).OrderByDescending(csub => csub.Id).Select(csub => new CompetencySubCategoryResult
                  {
                      Id = csub.Id,
                      CategoryId = csub.CategoryId,
                      SubcategoryCode = csub.SubcategoryCode,
                      CategoryName = csub.CategoryName,
                      SubcategoryDescription = csub.SubcategoryDescription,
                      IsDeleted = csub.IsDeleted,
                  });

                APIResponse<CompetencySubCategoryResult> result = new APIResponse<CompetencySubCategoryResult>();
                List<CompetencySubCategoryResult> CompetencySubCategoryResult = new List<CompetencySubCategoryResult>();
                if (competencySubCategoryList.Count() > 0)
                {
                    foreach (var item in competencySubCategoryList)
                    {
                        CompetencySubCategoryResult competencySubCategory = new CompetencySubCategoryResult();
                        competencySubCategory.Id = item.Id;
                        competencySubCategory.CategoryId = item.CategoryId;
                        competencySubCategory.SubcategoryCode = item.SubcategoryCode;
                        competencySubCategory.CategoryName = item.CategoryName;
                        competencySubCategory.SubcategoryDescription = item.SubcategoryDescription;
                        CompetencySubCategoryResult.Add(competencySubCategory);
                    }

                }

                result.Data.Records = CompetencySubCategoryResult;
                result.Data.RecordCount = CompetencySubCategoryResult.Count();
                return result;

            }
           
           

          
        }

        public async Task<IApiResponse> GetByCategoryId(int catID)
        {
            APIResponse<CompetencySubCategoryFilter> apiResponse = new APIResponse<CompetencySubCategoryFilter>();
            List<CompetencySubCategoryFilter> filters = new List<CompetencySubCategoryFilter>();

            var Query = this.db.CompetencySubCategory.Where(v => v.CategoryId == catID);
            Query.Where(v => v.IsDeleted == Record.NotDeleted);

            foreach (var item in Query)
            {
                CompetencySubCategoryFilter competencySubCategoryFilter = new CompetencySubCategoryFilter();
                competencySubCategoryFilter.id = item.Id;
                competencySubCategoryFilter.name = item.SubcategoryDescription;
                filters.Add(competencySubCategoryFilter);
            }
            apiResponse.Data.Records = filters;
            apiResponse.Data.RecordCount = filters.Count;
            return apiResponse;
        }

        #region Bulk Upload Competency Category

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APICompetencySubCategoryImportColumns.CategoryName, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencySubCategoryImportColumns.CompetencyCode, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencySubCategoryImportColumns.CompetencyDescription, 250));
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


                DataTable competencysubcategoryImportdt = ReadFile(filepath);
                if (competencysubcategoryImportdt == null || competencysubcategoryImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(competencysubcategoryImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(competencysubcategoryImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
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
            CompetencySubCategoryList.Clear();

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

        public async Task<ApiResponse> ProcessRecordsAsync(DataTable competencysubcategoryImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            List<APICompetencySubCategoryImport> apiCompetencySubCategoryImportRejected = new List<APICompetencySubCategoryImport>();

            competencysubcategoryImportdt.Columns.Add("ErrorMessage", typeof(string));
           
            int columnIndex = 0;
            DataColumnCollection columns = competencysubcategoryImportdt.Columns;
            foreach (string column in importcolumns)
            {
                competencysubcategoryImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = competencysubcategoryImportdt.Clone();
            DataTable newfinalDt = competencysubcategoryImportdt.Clone();


            if (competencysubcategoryImportdt != null && competencysubcategoryImportdt.Rows.Count > 0)
            {
                List<APICompetencySubCategoryImport> apiCompetencySubCategoryImportList = new List<APICompetencySubCategoryImport>();

                foreach (DataRow dataRow in competencysubcategoryImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = null;
                    foreach (string column in importcolumns)
                    {

                        if (string.Compare(column, "SubCategoryCode", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "SubCategory Code required";
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
                        if (string.Compare(column, "SubCategoryDescription", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "SubCategory Description required";
                            }

                        }

                    }
                    int CategoryId = 0;
                    CompetencyCategory category = db.CompetencyCategory.Where(a => a.CategoryName == dataRow[0] && a.IsDeleted == false).FirstOrDefault();
                    CategoryId = category.Id;
                    dataRow[0] = CategoryId;
                    /*if (await Exists(Convert.ToString(dataRow["CompetencyCode"]), Convert.ToString(dataRow["CompetencyName"])))*/
                    if (await Exists(Convert.ToString(dataRow[1]), Convert.ToString(dataRow[2])))
                    {
                        isError = true;
                        errorMsg = "SubCategory Code or SubCategory Description exists, duplicates not allowed";
                    }
                   
                    //CategoryID = GetCategoryIDByCategoryName(Convert.ToString(dataRow[0]));
                    if (isError)
                    {
                        /*                        competencyCategoryImport.CompetencyCode = dataRow["CompetencyCode"] != null ? Convert.ToString(dataRow["CategoryCode"]) : null;
                                                competencyCategoryImport.CompetencyName = dataRow["CompetencyName"] != null ? Convert.ToString(dataRow["CategoryName"]) : null;
                        */
                        competencySubCategoryImport.CategoryId = CategoryId;
                        competencySubCategoryImport.SubCategoryCode = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        competencySubCategoryImport.SubCategoryDescription = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;

                        competencySubCategoryImport.ErrMessage = errorMsg;
                        competencySubCategoryImport.IsInserted = "false";
                        competencySubCategoryImport.IsUpdated = "false";
                        competencySubCategoryImport.InsertedID = null;
                        competencySubCategoryImport.InsertedCode = "";
                        competencySubCategoryImport.notInsertedCode = "";
                        
                        dataRow[3] = competencySubCategoryImport.ErrMessage;
                        apiCompetencySubCategoryImportList.Add(competencySubCategoryImport);
                        //competencySubCategoryImport.CategoryName = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                    }
                    else
                    {
                        //totalRecordInsert++;
                        finalDt.ImportRow(dataRow);
                    }

                    competencySubCategoryImport = new APICompetencySubCategoryImport();
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
                            cmd.CommandText = "dbo.CompetencySubCategory_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencySubCategoryBulkUpload_TVP", SqlDbType.Structured) { Value = newfinalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiCompetencySubCategoryImportList.AddRange(dtResult.ConvertToList<APICompetencySubCategoryImport>());
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

                foreach (var data in apiCompetencySubCategoryImportList)
                {
                    if (!string.IsNullOrEmpty(data.SubCategoryCode)  || !string.IsNullOrEmpty(data.SubCategoryDescription) || !string.IsNullOrEmpty(data.CategoryName))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiCompetencySubCategoryImportRejected.Add(data);
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
            response.ResponseObject = new { resultstring, apiCompetencySubCategoryImportRejected };
            return response;

        }

        public DataTable removeDuplicatesRows(DataTable dt)
        {
            DataTable uniqueCols = dt.DefaultView.ToTable(true, "CategoryName", "SubCategoryCode", "SubCategoryDescription","ErrorMessage");
            return uniqueCols;
        }

        #endregion

    }
}
