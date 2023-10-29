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
    public class CompetencySubSubCategoryRepository: Repository<CompetencySubSubCategory>, ICompetencySubSubCategoryRepository
    {
        StringBuilder sb = new StringBuilder();
        string[] header = { };
        string[] headerStar = { };
        string[] headerWithoutStar = { };
        List<string> CompetencySubSubCategoryList = new List<string>();
        APICompetencySubSubCategoryImport competencySubSubCategoryImport = new APICompetencySubSubCategoryImport();


        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencySubSubCategoryRepository));
        private CourseContext db;
        private ICustomerConnectionStringRepository _customerConnection;
        private IEnumerable<object> apiCompetencySubSubCategoryImportList;
        private readonly IConfiguration _configuration;


        public CompetencySubSubCategoryRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection, IConfiguration configuration) : base(context)
        {
            this.db = context;
            this._customerConnection = customerConnection;
            _configuration = configuration;

        }

        public async Task<bool> Exists(string Code, string subcategory, int? categoryId = null)
        {
            Code = Code.ToLower().Trim();
            subcategory = subcategory.ToLower().Trim();
            int Count = 0;


            Count = await (from c in this.db.CompetencySubSubCategory
                           where c.IsDeleted == false && (c.SubSubcategoryCode.ToLower().Equals(Code) || c.SubSubcategoryDescription.ToLower().Equals(subcategory))
                           select new
                           { c.Id }).CountAsync();


            if (Count > 0)
                return true;
            return false;

        }


        public async Task<IApiResponse> GetAllCompetencySubSubCategory(int page, int pageSize, int categoryId = 0, int subcategoryId = 0, string search = null)
        {
            try
            {

                IQueryable<CompetencySubSubCategoryResult> Query = null;

                if (categoryId != 0 && subcategoryId != 0)
                {
                    Query = (from compsubsub in db.CompetencySubSubCategory
                             join compsub in db.CompetencySubCategory on compsubsub.SubCategoryId equals compsub.Id
                             join compcat in db.CompetencyCategory on compsubsub.CategoryId equals compcat.Id

                             where compsubsub.IsDeleted == false && compsub.IsDeleted == false && compcat.IsDeleted == false
                             && compsubsub.CategoryId == categoryId && compsubsub.SubCategoryId == subcategoryId
                             select new CompetencySubSubCategoryResult
                             {
                                 Id = compsubsub.Id,
                                 CategoryId = compsubsub.CategoryId,
                                 SubCategoryId = compsubsub.SubCategoryId,
                                 CategoryName = compcat.Category,
                                 SubCategoryName = compsub.SubcategoryDescription,
                                 SubSubcategoryCode = compsubsub.SubSubcategoryCode,
                                 SubSubcategoryDescription = compsubsub.SubSubcategoryDescription,
                                 IsDeleted = compsubsub.IsDeleted,
                             });
                }
                else if (categoryId == 0 && subcategoryId == 0)
                {
                    Query = (from compsubsub in db.CompetencySubSubCategory
                             join compsub in db.CompetencySubCategory on compsubsub.SubCategoryId equals compsub.Id
                             join compcat in db.CompetencyCategory on compsubsub.CategoryId equals compcat.Id

                             where compsubsub.IsDeleted == false && compsub.IsDeleted == false && compcat.IsDeleted == false
                             select new CompetencySubSubCategoryResult
                             {
                                 Id = compsubsub.Id,
                                 CategoryId = compsubsub.CategoryId,
                                 SubCategoryId = compsubsub.SubCategoryId,
                                 CategoryName = compcat.Category,
                                 SubCategoryName = compsub.SubcategoryDescription,
                                 SubSubcategoryCode = compsubsub.SubSubcategoryCode,
                                 SubSubcategoryDescription = compsubsub.SubSubcategoryDescription,
                                 IsDeleted = compsubsub.IsDeleted,
                             });
                }
                else if (categoryId == 0)
                {
                    Query = (from compsubsub in db.CompetencySubSubCategory
                             join compsub in db.CompetencySubCategory on compsubsub.SubCategoryId equals compsub.Id
                             join compcat in db.CompetencyCategory on compsubsub.CategoryId equals compcat.Id

                             where compsubsub.IsDeleted == false && compsub.IsDeleted == false && compcat.IsDeleted == false &&
                             compsubsub.SubCategoryId == subcategoryId

                             select new CompetencySubSubCategoryResult
                             {
                                 Id = compsubsub.Id,
                                 CategoryId = compsubsub.CategoryId,
                                 SubCategoryId = compsubsub.SubCategoryId,
                                 CategoryName = compcat.Category,
                                 SubCategoryName = compsub.SubcategoryDescription,
                                 SubSubcategoryCode = compsubsub.SubSubcategoryCode,
                                 SubSubcategoryDescription = compsubsub.SubSubcategoryDescription,
                                 IsDeleted = compsubsub.IsDeleted,
                             });
                }
                else if (subcategoryId == 0)
                {
                    Query = (from compsubsub in db.CompetencySubSubCategory
                             join compsub in db.CompetencySubCategory on compsubsub.SubCategoryId equals compsub.Id
                             join compcat in db.CompetencyCategory on compsubsub.CategoryId equals compcat.Id

                             where compsubsub.IsDeleted == false && compsub.IsDeleted == false && compcat.IsDeleted == false
                             && compsubsub.CategoryId == categoryId
                             select new CompetencySubSubCategoryResult
                             {
                                 Id = compsubsub.Id,
                                 CategoryId = compsubsub.CategoryId,
                                 SubCategoryId = compsubsub.SubCategoryId,
                                 CategoryName = compcat.Category,
                                 SubCategoryName = compsub.SubcategoryDescription,
                                 SubSubcategoryCode = compsubsub.SubSubcategoryCode,
                                 SubSubcategoryDescription = compsubsub.SubSubcategoryDescription,
                                 IsDeleted = compsubsub.IsDeleted,
                             });
                }

                APIResponse<CompetencySubSubCategoryResult> result = new APIResponse<CompetencySubSubCategoryResult>();

                if (Query == null)
                {
                    return new APIResposeNo { Message = "No Records Found.", StatusCode = 204 };
                }
                else
                {

                    if (search == "null")
                    {
                        search = null;
                    }
                    if (!string.IsNullOrEmpty(search))
                    {
                        Query = Query.Where(v => ((v.SubSubcategoryDescription.StartsWith(search) || v.SubSubcategoryCode.StartsWith(search)) && (v.IsDeleted == Record.NotDeleted)));
                        Query = Query.OrderByDescending(v => v.Id);
                    }
                   

                    result.Data.RecordCount =  await Query.CountAsync();

                    Query = Query.OrderByDescending(v => v.Id);
                    if (result.Data.RecordCount > 0)
                    {
                        if (page != -1)
                            Query = Query.Skip((page - 1) * pageSize);

                        if (pageSize != -1)
                            Query = Query.Take(pageSize);

                        Query = Query.OrderByDescending(v => v.Id);
                        result.Data.Records = Query.ToList();
                        result.Message = "Success";
                    }

                    APIResponse<CompetencySubSubCategoryResult> result1 = new APIResponse<CompetencySubSubCategoryResult>();
                    List<CompetencySubSubCategoryResult> CompetencySubSubCategoryResult = new List<CompetencySubSubCategoryResult>();
                    if (Query.Count() > 0)
                    {
                        foreach (var item in Query)
                        {
                            CompetencySubSubCategoryResult competencySubSubCategory = new CompetencySubSubCategoryResult();
                            competencySubSubCategory.Id = item.Id;
                            competencySubSubCategory.CategoryId = item.CategoryId;
                            competencySubSubCategory.CategoryName = item.CategoryName;
                            competencySubSubCategory.SubCategoryId = item.SubCategoryId;
                            competencySubSubCategory.SubCategoryName = item.SubCategoryName;
                            competencySubSubCategory.SubSubcategoryCode = item.SubSubcategoryCode;
                            competencySubSubCategory.SubSubcategoryDescription = item.SubSubcategoryDescription;
                            CompetencySubSubCategoryResult.Add(competencySubSubCategory);
                        }

                        return result;
                    }
                    result1.Data.Records = CompetencySubSubCategoryResult;
                    result1.Data.RecordCount = CompetencySubSubCategoryResult.Count();

                    return result1;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        #region Bulk Upload Competency Category

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APICompetencySubSubCategoryImportColumns.CategoryName, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencySubSubCategoryImportColumns.SubCategory, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencySubSubCategoryImportColumns.SubsetCode, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencySubSubCategoryImportColumns.SubsetDescription, 250));
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


                DataTable competencysubsubcategoryImportdt = ReadFile(filepath);
                if (competencysubsubcategoryImportdt == null || competencysubsubcategoryImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(competencysubsubcategoryImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(competencysubsubcategoryImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
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
            CompetencySubSubCategoryList.Clear();

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

        public async Task<ApiResponse> ProcessRecordsAsync(DataTable competencysubsubcategoryImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            List<APICompetencySubSubCategoryImport> apiCompetencySubSubCategoryImportRejected = new List<APICompetencySubSubCategoryImport>();

            competencysubsubcategoryImportdt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = competencysubsubcategoryImportdt.Columns;
            foreach (string column in importcolumns)
            {
                competencysubsubcategoryImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = competencysubsubcategoryImportdt.Clone();
            DataTable newfinalDt = competencysubsubcategoryImportdt.Clone();


            if (competencysubsubcategoryImportdt != null && competencysubsubcategoryImportdt.Rows.Count > 0)
            {
                List<APICompetencySubSubCategoryImport> apiCompetencySubSubCategoryImportList = new List<APICompetencySubSubCategoryImport>();

                foreach (DataRow dataRow in competencysubsubcategoryImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = null;
                    foreach (string column in importcolumns)
                    {

                        if (string.Compare(column, "SubCategory", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "SubCategory  required";
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
                        if (string.Compare(column, "SubsetDescription", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Subset Description required";
                            }

                        }
                        if (string.Compare(column, "SubsetCode", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "SubsetCode required";
                            }

                        }

                    }
                    int CategoryId = 0;
                    CompetencyCategory category = db.CompetencyCategory.Where(a => a.CategoryName == dataRow[0] && a.IsDeleted == false).FirstOrDefault();
                    CategoryId = category.Id;
                    dataRow[0] = CategoryId;

                    int SubCategoryId = 0;
                    CompetencySubCategory subcategory = db.CompetencySubCategory.Where(a => a.SubcategoryCode == dataRow[1] && a.IsDeleted == false).FirstOrDefault();
                    SubCategoryId = subcategory.Id;
                    dataRow[1] = SubCategoryId;
                    /*if (await Exists(Convert.ToString(dataRow["CompetencyCode"]), Convert.ToString(dataRow["CompetencyName"])))*/
                    if (await Exists(Convert.ToString(dataRow[2]), Convert.ToString(dataRow[3])))
                    {
                        isError = true;
                        errorMsg = "Subset Code or Subset Description exists, duplicates not allowed";
                    }

                    //CategoryID = GetCategoryIDByCategoryName(Convert.ToString(dataRow[0]));
                    if (isError)
                    {
                        /*                        competencyCategoryImport.CompetencyCode = dataRow["CompetencyCode"] != null ? Convert.ToString(dataRow["CategoryCode"]) : null;
                                                competencyCategoryImport.CompetencyName = dataRow["CompetencyName"] != null ? Convert.ToString(dataRow["CategoryName"]) : null;
                        */
                        competencySubSubCategoryImport.CategoryId = CategoryId;
                        competencySubSubCategoryImport.SubCategory = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        competencySubSubCategoryImport.SubsetCode = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;
                        competencySubSubCategoryImport.SubsetDescription = dataRow[3] != null ? Convert.ToString(dataRow[3]) : null;

                        competencySubSubCategoryImport.ErrMessage = errorMsg;
                        competencySubSubCategoryImport.IsInserted = "false";
                        competencySubSubCategoryImport.IsUpdated = "false";
                        competencySubSubCategoryImport.InsertedID = null;
                        competencySubSubCategoryImport.InsertedCode = "";
                        competencySubSubCategoryImport.notInsertedCode = "";

                        dataRow[3] = competencySubSubCategoryImport.ErrMessage;
                        apiCompetencySubSubCategoryImportList.Add(competencySubSubCategoryImport);
                        //competencySubCategoryImport.CategoryName = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                    }
                    else
                    {
                        //totalRecordInsert++;
                        finalDt.ImportRow(dataRow);
                    }

                    competencySubSubCategoryImport = new APICompetencySubSubCategoryImport();
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
                            cmd.CommandText = "dbo.CompetencySubSubCategory_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencySubSubCategoryBulkUpload_TVP", SqlDbType.Structured) { Value = newfinalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiCompetencySubSubCategoryImportList.AddRange(dtResult.ConvertToList<APICompetencySubSubCategoryImport>());
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

                foreach (var data in apiCompetencySubSubCategoryImportList)
                {
                    if (!string.IsNullOrEmpty(data.SubsetCode) || !string.IsNullOrEmpty(data.SubsetDescription) || !string.IsNullOrEmpty(data.CategoryName) || !string.IsNullOrEmpty(data.SubCategory))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiCompetencySubSubCategoryImportRejected.Add(data);
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
            response.ResponseObject = new { resultstring, apiCompetencySubSubCategoryImportRejected };
            return response;

        }

        public DataTable removeDuplicatesRows(DataTable dt)
        {
            DataTable uniqueCols = dt.DefaultView.ToTable(true, "CategoryName", "SubCategory", "SubsetCode", "SubsetDescription", "ErrorMessage");
            return uniqueCols;
        }

        #endregion

    }
}
    

