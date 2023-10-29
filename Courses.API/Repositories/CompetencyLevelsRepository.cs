// ======================================
// <copyright file="CompetencyLevelsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Courses.API.APIModel.Competency;
using Courses.API.Helper;
using Courses.API.Model.Competency;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces.Competency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Courses.API.APIModel;
using Microsoft.Extensions.Configuration;
using Courses.API.Repositories.Interfaces;
using System.Data;
using System.Text;
using OfficeOpenXml;
using System.IO;
using System.Data.Common;
using Courses.API.Common;
using Microsoft.Data.SqlClient;
using static Courses.API.Model.ResponseModels;

namespace Courses.API.Repositories.Competency
{
    public class CompetencyLevelsRepository : Repository<CompetencyLevels>, ICompetencyLevelsRepository
    {
        StringBuilder sb = new StringBuilder();
        APICompetencyLevelImport competencyLevelImport = new APICompetencyLevelImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencyLevelsRepository));
        private CourseContext db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private IConfiguration _configuration;
        private readonly ICourseRepository _courseRepository;

        public CompetencyLevelsRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionStringRepository, ICourseRepository courseRepository) : base(context)
        {
            this.db = context;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
            _courseRepository = courseRepository;
        }
        public async Task<IApiResponse> GetAllCompetencyLevels(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<APICompetencyLevelsV2> result = null;

                //using (var context = this.db)
                //{
                        result = (from competencyLevels in this.db.CompetencyLevels
                                  join competencyCategory in this.db.CompetencyCategory on competencyLevels.CategoryId equals competencyCategory.Id 
                                  into cat 
                                  from competencyCategory in cat.DefaultIfEmpty()
                                  join competencySubCategory in this.db.CompetencySubCategory on competencyLevels.SubCategoryId equals competencySubCategory.Id
                                  into cat2
                                  from competencySubCategory in cat2.DefaultIfEmpty()
                                  join competencySubSubCategory in this.db.CompetencySubSubCategory on competencyLevels.SubSubCategoryId equals competencySubSubCategory.Id
                                  into cat3
                                  from competencySubSubCategory in cat3.DefaultIfEmpty()
                                  join competenciesMaster in this.db.CompetenciesMaster on competencyLevels.CompetencyId equals competenciesMaster.Id
                                  into compMaster
                                  from competenciesMasterSub2 in compMaster.DefaultIfEmpty()
                                  orderby competencyLevels.Id descending
                                  where competencyLevels.IsDeleted == Record.NotDeleted
                                  select new APICompetencyLevelsV2
                                  {
                                      Id = competencyLevels.Id,
                                      CategoryId = competencyLevels.CategoryId,
                                      SubCategoryId = competencyLevels.SubCategoryId == null ? 0 : competencyLevels.SubCategoryId,
                                      SubSubCategoryId = competencyLevels.SubSubCategoryId == null ? 0 : competencyLevels.SubSubCategoryId,
                                      SubCategoryName = competencySubCategory.SubcategoryDescription,
                                      SubSubCategoryName = competencySubSubCategory.SubSubcategoryDescription,
                                      CompetencyId = competencyLevels.CompetencyId,
                                      LevelName = competencyLevels.LevelName,
                                      BriefDescriptionCompetencyLevel = competencyLevels.BriefDescriptionCompetencyLevel,
                                      DetailedDescriptionOfLevel = competencyLevels.DetailedDescriptionOfLevel,
                                      Category = competencyCategory.Category,
                                      Competency = competenciesMasterSub2.CompetencyName

                                  });

                if(result == null)
                {

                    return new APIResposeNo { Message = "No Records Found", StatusCode = 204 };
                }else
                {
                    APIResponse<APICompetencyLevelsV2> aPIResponse = new APIResponse<APICompetencyLevelsV2>();
                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where(a => ((Convert.ToString(a.LevelName.ToLower()).StartsWith(search.ToLower()))
                        || ((Convert.ToString(a.Competency.ToLower()).StartsWith(search.ToLower())))
                        || ((Convert.ToString(a.Category.ToLower()).StartsWith(search.ToLower())))
                        ));
                    }
                    aPIResponse.Data.RecordCount = await result.CountAsync();
                    if(aPIResponse.Data.RecordCount > 0)
                    {
                        if (page != -1)
                            result = result.Skip((page - 1) * pageSize);

                        if (pageSize != -1)
                            result = result.Take(pageSize);

                        result = result.OrderByDescending(v => v.Id);
                        aPIResponse.Data.Records = result.ToList();

                    }
                    return aPIResponse;
                }

                }
            //}
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;

        }

        public async Task<IApiResponse> GetNextLevel(int id)
        {
            APIResponseSingle<CompetencyLevels> aPIResponse = new APIResponseSingle<CompetencyLevels>();
            IQueryable<CompetencyLevels> result = null;
            result = this.db.CompetencyLevels.Where(c => ((c.CompetencyId == id) && (c.IsDeleted == false)));
            if(await result.CountAsync() > 0)
            {
                var res = result.OrderByDescending(v => v.LevelName).FirstOrDefault();
                var nextLevel = "";
                switch (res.LevelName)
                {
                    case "Level 1":
                        nextLevel = "Level 2";
                        break;
                    case "Level 2":
                        nextLevel = "Level 3";
                        break;
                    case "Level 3":
                        nextLevel = "Level 4";
                        break;
                    case "Level 4":
                        nextLevel = "Level 5";
                        break;
                    case "Level 5":
                        nextLevel = "Level 6";
                        break;
                    case "Level 6":
                        nextLevel = "Level 7";
                        break;
                    case "Level 7":
                        nextLevel = "Level 8";
                        break;
                    case "Level 8":
                        nextLevel = "Level 9";
                        break;
                    case "Level 9":
                        nextLevel = "You have reached to maximum Levels i.e. Level 9";
                        break;

                    default:
                        break;
                }

                return new APIResposeYes { Content = nextLevel };
                
            }
            else
            {
                return new APIResposeYes { Content = "Level 1" };
            }
                      
            
        }

        public async Task<bool> Exists(string Level,  int comId)
        {
            var count = await this.db.CompetencyLevels.Where(p => (((p.LevelName.ToLower()== Level.ToLower()))  && (p.CompetencyId == comId) && (p.IsDeleted == Record.NotDeleted))).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
      

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.CompetencyLevels.Where(r => (r.LevelName.StartsWith(search) || Convert.ToString(r.CategoryId).StartsWith(search) || Convert.ToString(r.DetailedDescriptionOfLevel).StartsWith(search)) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.CompetencyLevels.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<IEnumerable<APICompetencyLevels>> GetCompetencyLevels()
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competencyLevels in context.CompetencyLevels
                                  join competencyCategory in context.CompetencyCategory on competencyLevels.CategoryId equals competencyCategory.Id
                                  join competenciesMaster in context.CompetenciesMaster on competencyLevels.CompetencyId equals competenciesMaster.Id
                                  where competencyLevels.IsDeleted == Record.NotDeleted
                                  select new APICompetencyLevels
                                  {
                                      Id = competencyLevels.Id,
                                      CategoryId = competencyLevels.CategoryId,
                                      CompetencyId = competencyLevels.CompetencyId,
                                      LevelName = competencyLevels.LevelName,
                                      BriefDescriptionCompetencyLevel = competencyLevels.BriefDescriptionCompetencyLevel,
                                      DetailedDescriptionOfLevel = competencyLevels.DetailedDescriptionOfLevel,
                                      Category = competencyCategory.Category,
                                      Competency = competenciesMaster.CompetencyDescription

                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APICompetencyLevels>> GetCompetencyLevels(int id)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competencyLevels in context.CompetencyLevels
                                  join competencyCategory in context.CompetencyCategory on competencyLevels.CategoryId equals competencyCategory.Id
                                  join competenciesMaster in context.CompetenciesMaster on competencyLevels.CompetencyId equals competenciesMaster.Id
                                  where competencyLevels.Id == id && competencyLevels.IsDeleted == Record.NotDeleted
                                  select new APICompetencyLevels
                                  {
                                      Id = competencyLevels.Id,
                                      CategoryId = competencyLevels.CategoryId,
                                      CompetencyId = competencyLevels.CompetencyId,
                                      LevelName = competencyLevels.LevelName,
                                      BriefDescriptionCompetencyLevel = competencyLevels.BriefDescriptionCompetencyLevel,
                                      DetailedDescriptionOfLevel = competencyLevels.DetailedDescriptionOfLevel,
                                      Category = competencyCategory.Category,
                                      Competency = competenciesMaster.CompetencyDescription

                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APICompetencyLevels>> GetAllCompetencyLevelsCat(int? CatId, int? ComId)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competencyLevels in context.CompetencyLevels
                                  join competencyCategory in context.CompetencyCategory on competencyLevels.CategoryId equals competencyCategory.Id into tempcat from competencyCategory in tempcat.DefaultIfEmpty()
                                  join competenciesMaster in context.CompetenciesMaster on competencyLevels.CompetencyId equals competenciesMaster.Id 
                                  where (((competencyLevels.CategoryId == CatId || CatId == null) && competencyLevels.CompetencyId == ComId) && competencyLevels.IsDeleted == Record.NotDeleted)
                                  select new APICompetencyLevels
                                  {
                                      Id = competencyLevels.Id,
                                      CategoryId = competencyLevels.CategoryId,
                                      CompetencyId = competencyLevels.CompetencyId,
                                      LevelName = competencyLevels.LevelName,
                                      BriefDescriptionCompetencyLevel = competencyLevels.BriefDescriptionCompetencyLevel,
                                      DetailedDescriptionOfLevel = competencyLevels.DetailedDescriptionOfLevel,
                                      Category = competencyCategory.Category,
                                      Competency = competenciesMaster.CompetencyDescription

                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public bool IsDependacyExist(int levelId)
        {

            int CompetenciesMappingCount = (from competenciesMapping in db.CompetenciesMapping
                                            where (competenciesMapping.IsDeleted == false && competenciesMapping.CompetencyLevelId == levelId)
                                            select new { competenciesMapping.Id }).Count();

            if (CompetenciesMappingCount > 0)
                return true;

            return false;
        }

        #region Bulk Upload Competency Levels

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            var Subcategoryconfig = _courseRepository.GetMasterConfigurableParameterValue("COMP_SUBCATEGORY");
            var SubSubcategoryconfig = _courseRepository.GetMasterConfigurableParameterValue("COMP_SUB_SUBCATEGORY");

            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            if (Convert.ToString(Subcategoryconfig.Result).ToLower() == "yes" && Convert.ToString(SubSubcategoryconfig.Result).ToLower() == "yes")
            {
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.Category, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.SubCategory, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.SubSubCategory, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.Competency, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.CompetencyLevel, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.LevelDescription, 500));
            }
            else if (Convert.ToString(Subcategoryconfig.Result).ToLower() == "yes" && Convert.ToString(SubSubcategoryconfig.Result).ToLower() == "no")
            {
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.Category, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.SubCategory, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.Competency, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.CompetencyLevel, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.LevelDescription, 500));
            }
            else
            {
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.Category, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.Competency, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.CompetencyLevel, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyLevelImportColumns.LevelDescription, 500));
            }
          
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


                DataTable competenciesLevelsImportdt = ReadFile(filepath);
                if (competenciesLevelsImportdt == null || competenciesLevelsImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(competenciesLevelsImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(competenciesLevelsImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
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
                    {
                        dt.Columns.Add(firstRowCell.Text.Trim());
                    }
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
                            {
                                row[cell.Start.Column - 1] = cell.Text;
                            }
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
                {
                    _logger.Error(importColumns+"\t"+userImportdt.Columns[i].ColumnName);
                    return false;
                }
                   
            }
            return true;
        }


        public async Task<ApiResponse> ProcessRecordsAsync(DataTable competenciesLevelsImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {

            ApiResponse response = new ApiResponse();
            List<APICompetencyLevelImport> apiCompetencyLevelImportRejected = new List<APICompetencyLevelImport>();

           

            int columnIndex = 0;
            DataColumnCollection columns = competenciesLevelsImportdt.Columns;

            foreach (string column in importcolumns)
            {
                competenciesLevelsImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }

            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = competenciesLevelsImportdt.Clone();

            if (competenciesLevelsImportdt != null && competenciesLevelsImportdt.Rows.Count > 0)
            {

                List<APICompetencyLevelImport> apiCompetencyLevelImportList = new List<APICompetencyLevelImport>();

                foreach (DataRow dataRow in competenciesLevelsImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = "";
                    int categoryFlag = 0;
                    int catId = 0;
                    int subcatId = 0;
                    int subsubcatId = 0;
                    string compLevel = "";
                    int? competencyId = 0;

                    IEnumerable<APICompetenciesMaster> competenciesByCatId = null;
                    IEnumerable<APICompetenciesMaster> competenciesAll = null;

                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column,"Category") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (await CategoryExists(Convert.ToString(dataRow[column])))
                                {
                                    catId = await GetIdByCategory(Convert.ToString(dataRow[column]));
                                    competenciesByCatId = await GetCompetenciesMasterByID(Convert.ToInt32(catId));
                                    categoryFlag = 1;
                                    //isErrorDatarow
                                    //dataRow[column] = catId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Category does not exist, Please enter an existing Category";
                                    break;
                                }
                            }
                            else
                            {
                                competenciesAll = await GetCompetenciesMaster();
                                //dataRow[column] = 0;
                            }

                        }

                        if (string.Compare(column, "Subcategory") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (await SubcategoryExists(Convert.ToString(dataRow[column])))
                                {
                                    subcatId = await GetIdBySubcategory(Convert.ToString(dataRow[column]));
                                   // competenciesByCatId = await GetCompetenciesMasterByID(Convert.ToInt32(catId));
                                   // categoryFlag = 1;
                                    //isErrorDatarow
                                    //dataRow[column] = catId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Subcategory does not exist, Please enter an existing Subcategory";
                                    break;
                                }
                            }
                            else
                            {
                                competenciesAll = await GetCompetenciesMaster();
                                //dataRow[column] = 0;
                            }

                        }

                        if (string.Compare(column, "SubSubcategory") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (await SubSubcategoryExists(Convert.ToString(dataRow[column])))
                                {
                                    subsubcatId = await GetIdBySubSubcategory(Convert.ToString(dataRow[column]));
                                    // competenciesByCatId = await GetCompetenciesMasterByID(Convert.ToInt32(catId));
                                    // categoryFlag = 1;
                                    //isErrorDatarow
                                    //dataRow[column] = catId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Subcategory does not exist, Please enter an existing Subcategory";
                                    break;
                                }
                            }
                            else
                            {
                                competenciesAll = await GetCompetenciesMaster();
                                //dataRow[column] = 0;
                            }

                        }

                        if (string.Compare(column, "Competency") == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Competency required";
                                break;
                            }

                            if (categoryFlag!=0)
                            {
                                competencyId = competencyInCategoryCheck(competenciesByCatId, Convert.ToString(dataRow[column]));
                                if (competencyId != null)
                                {
                                    //isErrorDatarow
                                    //dataRow[column] = competencyId;
                                    competencyId = competencyId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Competency Name incorrect. Please enter a valid competency for the entered Category.";
                                    break;
                                }
                                
                            }
                            else
                            {
                                competencyId = competencyInCategoryCheck(competenciesAll, Convert.ToString(dataRow[column]));
                                if (competencyId != null)
                                {
                                    //isErrorDatarow
                                    //dataRow[column] = competencyId;
                                    competencyId = competencyId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Competency Name incorrect. Please enter a valid competency.";
                                    break;
                                }
                            }
                            
                        }

                        if (string.Compare(column, "CompetencyLevel") == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Competency Level required";
                                break;
                            }
                            else
                            {
                                List<string> levelsList = new List<string>() { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6", "Level 7", "Level 8", "Level 9" };
                                compLevel = LevelChecker(levelsList,Convert.ToString(dataRow[column]));
                                if (compLevel!=null)
                                {
                                    //isErrorDatarow
                                    //dataRow[column] = compLevel;
                                    compLevel = compLevel;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Competency Level incorrect. Please enter a valid level.";
                                    break;
                                }
                            }
                        }

                        if (string.Compare(column, "LevelDescription") == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Level Description required";
                            }
                        }

                    }

                    if (competencyId != null && compLevel != null)
                    {
                        if (await Exists(compLevel, Convert.ToInt32(competencyId)))
                        {
                            isError = true;
                            errorMsg = "Duplicate entry for competency and level combination not allowed.";
                        }
                    }

                    if (!isError)
                    {
                        dataRow["Category"]= catId;
                        if(subcatId == 0)
                        {
                            dataRow["Subcategory"] = null;
                        }
                        else
                        {
                            dataRow["Subcategory"] = subcatId;
                        }
                        if(subsubcatId == 0)
                        {
                            dataRow["SubSubcategory"] = null;
                        }
                        else
                        {
                            dataRow["SubSubcategory"] = subsubcatId;
                        }
                        dataRow["Competency"] = competencyId;
                        dataRow["CompetencyLevel"] = compLevel;
                    }

                    if (isError)
                    {
                        competencyLevelImport.Category = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                        competencyLevelImport.Competency = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        competencyLevelImport.CompetencyLevel = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;
                        competencyLevelImport.LevelDescription = dataRow[3] != null ? Convert.ToString(dataRow[3]) : null;

                        competencyLevelImport.ErrMessage = errorMsg;
                        competencyLevelImport.IsInserted = "false";
                        competencyLevelImport.IsUpdated = "false";
                        competencyLevelImport.InsertedID = null;
                        competencyLevelImport.InsertedCode = "";
                        competencyLevelImport.notInsertedCode = "";
                        dataRow[4] = competencyLevelImport.ErrMessage;
                        apiCompetencyLevelImportList.Add(competencyLevelImport);
                        response.ResponseObject = apiCompetencyLevelImportList;
                        response.StatusCode = 400;
                        response.Message = errorMsg;
                        return response;
                    }
                    else
                    {
                        totalRecordInsert++;
                        finalDt.ImportRow(dataRow);
                        if (!finalDt.Columns.Contains("Subcategory"))
                        {
                            finalDt.Columns.Add("Subcategory", typeof(string), null);

                        }
                        if (!finalDt.Columns.Contains("SubSubcategory"))
                        {
                            finalDt.Columns.Add("SubSubcategory", typeof(string), null);
                        }
                        finalDt.Columns.Add("ErrorMessage", typeof(string));
                    }

                    competencyLevelImport = new APICompetencyLevelImport();
                    sb.Clear();
                }

                try
                {
                    //DB 
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                        {
                            connection.Open();
                        }

                        DataTable dtResult = new DataTable();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "dbo.CompetenciesLevels_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 0;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyLevelBulkUpload_TVP", SqlDbType.Structured) { Value = finalDt });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiCompetencyLevelImportList.AddRange(dtResult.ConvertToList<APICompetencyLevelImport>());
                        connection.Close();

                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiCompetencyLevelImportList)
                {
                    if (!string.IsNullOrEmpty(data.Competency) || !string.IsNullOrEmpty(data.CompetencyLevel) || !string.IsNullOrEmpty(data.LevelDescription))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiCompetencyLevelImportRejected.Add(data);
                        }
                    }
                }

            }
            string resultstring = "Total number of records inserted : " + totalRecordInsert + ",  Total number of records rejected : " + totalRecordRejected;


            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiCompetencyLevelImportRejected };
            return response;

        }


        public int? competencyInCategoryCheck(IEnumerable<APICompetenciesMaster> competenciesByCatId, string comp)
        {
            comp = comp.Trim();
            //string compName = "";
            //compName = competenciesByCatId.FirstOrDefault(c => String.Equals(c.CompetencyName,comp,StringComparison.CurrentCultureIgnoreCase)).CompetencyName;
            int? compId = 0;
            var res = competenciesByCatId.FirstOrDefault(c => String.Equals(c.CompetencyName, comp, StringComparison.CurrentCultureIgnoreCase));

            if (res != null)
            {
                compId = res.Id;
                return compId;
            }
/*
            if (compId != 0 || compId != null)
            {
                return Convert.ToInt32(compId);
            }*/
            else
                return null;
        }

        public string LevelChecker(List<string> levelsList,string level)
        {
            //List<string> levelsList = new List<string>() { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" };
            level = level.Trim();

            string levelReturned = "";
            levelReturned = levelsList.FirstOrDefault(l => string.Equals(l, level, StringComparison.CurrentCultureIgnoreCase));

            if (levelReturned != null || levelReturned!="")
            {
                return levelReturned;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> CategoryExists(string category)
        {
            category = category.ToLower().Trim();
            int Count = 0;
            Count = await (from c in this.db.CompetencyCategory
                           where c.IsDeleted == false && (c.Category.ToLower().Equals(category))
                           select new
                           { c.Id }).CountAsync();

            if (Count > 0)
                return true;
            return false;

        }

        public async Task<bool> SubcategoryExists(string category)
        {
            category = category.ToLower().Trim();
            int Count = 0;
            Count = await (from c in this.db.CompetencySubCategory
                           where c.IsDeleted == false && (c.SubcategoryDescription.ToLower().Equals(category))
                           select new
                           { c.Id }).CountAsync();

            if (Count > 0)
                return true;
            return false;

        }
        public async Task<bool> SubSubcategoryExists(string category)
        {
            category = category.ToLower().Trim();
            int Count = 0;
            Count = await (from c in this.db.CompetencySubSubCategory
                           where c.IsDeleted == false && (c.SubSubcategoryDescription.ToLower().Equals(category))
                           select new
                           { c.Id }).CountAsync();

            if (Count > 0)
                return true;
            return false;

        }

        public async Task<int> GetIdByCategory(string Category)
        {
            string category = Category.ToLower().Trim();

            return await db.CompetencyCategory.Where(c => String.Equals(c.Category, category)).Select(c => c.Id).FirstOrDefaultAsync(); ;

        }

        public async Task<int> GetIdBySubcategory(string Category)
        {
            string category = Category.ToLower().Trim();

            return await db.CompetencySubCategory.Where(c => String.Equals(c.SubcategoryDescription, category)).Select(c => c.Id).FirstOrDefaultAsync(); ;

        }
        public async Task<int> GetIdBySubSubcategory(string Category)
        {
            string category = Category.ToLower().Trim();

            return await db.CompetencySubSubCategory.Where(c => String.Equals(c.SubSubcategoryDescription, category)).Select(c => c.Id).FirstOrDefaultAsync(); ;

        }

        public async Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMasterByID(int? id)
        {
            try
            {

                    var result = (from competenciesMaster in this.db.CompetenciesMaster
                                  join competencyCategory in this.db.CompetencyCategory on competenciesMaster.CategoryId equals competencyCategory.Id into cat
                                  from competencyCategory in cat.DefaultIfEmpty()
                                  where (competenciesMaster.CategoryId == id || id == null) && competenciesMaster.IsDeleted == Record.NotDeleted
                                  select new APICompetenciesMaster
                                  {
                                      Id = competenciesMaster.Id,
                                      CategoryId = competenciesMaster.CategoryId,
                                      CompetencyName = competenciesMaster.CompetencyName,
                                      CompetencyDescription = competenciesMaster.CompetencyDescription,
                                      Category = competencyCategory.Category

                                  });
                    return await result.AsNoTracking().ToListAsync();
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMaster()
        {
            try
            {
                    var result = (from competenciesMaster in this.db.CompetenciesMaster
                                  join competencyCategory in this.db.CompetencyCategory on competenciesMaster.CategoryId equals competencyCategory.Id into cat
                                  from competencyCategory in cat.DefaultIfEmpty()
                                  where competenciesMaster.IsDeleted == Record.NotDeleted
                                  select new APICompetenciesMaster
                                  {
                                      Id = competenciesMaster.Id,
                                      CategoryId = competenciesMaster.CategoryId,
                                      CompetencyName = competenciesMaster.CompetencyName,
                                      CompetencyDescription = competenciesMaster.CompetencyDescription,
                                      Category = competencyCategory.Category

                                  });
                    return await result.AsNoTracking().ToListAsync();
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


    #endregion
    }
}
