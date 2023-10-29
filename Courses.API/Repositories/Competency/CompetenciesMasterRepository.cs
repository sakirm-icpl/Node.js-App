// ======================================
// <copyright file="CompetenciesMasterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Courses.API.APIModel.Competency;
using Courses.API.Helper;
using Courses.API.Model.Competency;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Courses.API.Repositories.Interfaces.Competency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Courses.API.APIModel;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.IO;
using System.Text;
using Courses.API.Common;
using static Courses.API.Model.ResponseModels;

namespace Courses.API.Repositories.Competency
{
    public class CompetenciesMasterRepository : Repository<CompetenciesMaster>, ICompetenciesMasterRepository
    {
        StringBuilder sb = new StringBuilder();
        string[] header = { };
        string[] headerStar = { };
        string[] headerWithoutStar = { };
        List<string> CompetencyMasterList = new List<string>();
        APICompetencyMasterImport competencyMasterImport = new APICompetencyMasterImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;


        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesMasterRepository));
        private CourseContext db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private ICourseRepository _courseRepository;
        private readonly IConfiguration _configuration;

        public CompetenciesMasterRepository(IConfiguration configuration, CourseContext context, ICustomerConnectionStringRepository customerConnectionStringRepository, ICourseRepository courseRepository) : base(context)
        {
            this.db = context;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
            _courseRepository = courseRepository;
        }
        public async Task<IEnumerable<CompetenciesMaster>> GetAllCompetenciesMaster(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Model.Competency.CompetenciesMaster> Query = this.db.CompetenciesMaster;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.CompetencyName.StartsWith(search) || v.CompetencyDescription.StartsWith(search) && v.IsDeleted == Record.NotDeleted);
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
                return await this.db.CompetenciesMaster.Where(r => ((r.CompetencyName.StartsWith(search) || r.CompetencyDescription.StartsWith(search)) && (r.IsDeleted == Record.NotDeleted))).CountAsync();
            return await this.db.CompetenciesMaster.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<bool> Exists(string Code, string category, int categoryid)
        {
            var count = await this.db.CompetenciesMaster.Where(p => (String.Equals(p.CompetencyName, Code, StringComparison.CurrentCultureIgnoreCase) && (p.IsDeleted == Record.NotDeleted) && (p.CategoryId == categoryid))).CountAsync();
            var countCategary = await this.db.CompetenciesMaster.Where(p => (String.Equals(p.CompetencyDescription, category, StringComparison.CurrentCultureIgnoreCase)) && (p.IsDeleted == Record.NotDeleted) && (p.CategoryId == categoryid)).CountAsync();
            if (count > 0)
                return true;
            if (countCategary > 0)
                return true;
            return false;
        }



        public async Task<int> ExistsRecordForJobrole(string Code)
        {
            int competencymasterid = 0;
            Code = Code.ToLower().Trim();
            int Count = 0;

            Count = await (from c in this.db.CompetenciesMaster
                           where c.IsDeleted == false && (c.CompetencyName.ToLower().Equals(Code))
                           select new
                           { c.Id }).CountAsync();


            if (Count > 0)
            {
                return await db.CompetenciesMaster.Where(c => String.Equals(c.CompetencyName, Code, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Id).FirstOrDefaultAsync();
            }
            else
            {
                return competencymasterid;
            }


        }


        public async Task<bool> ExistsRecord(int? id, string Code)
        {

            Code = Code.ToLower().Trim();
            int Count = 0;

            if (id != null)
            {
                Count = await (from c in this.db.CompetenciesMaster
                               where c.Id != id && c.IsDeleted == false && (c.CompetencyName.ToLower().Equals(Code))
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this.db.CompetenciesMaster
                               where c.IsDeleted == false && (c.CompetencyName.ToLower().Equals(Code))
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;


        }
        public bool IsDependacyExist(int comptId)
        {

            int CountLevelCount = (from competencyLevels in db.CompetencyLevels
                                   where (competencyLevels.IsDeleted == false && competencyLevels.CompetencyId == comptId)
                                   select new { competencyLevels.Id }).Count();
            if (CountLevelCount > 0)
                return true;

            int CompetenciesMappingCount = (from competenciesMapping in db.CompetenciesMapping
                                            where (competenciesMapping.IsDeleted == false && competenciesMapping.CompetencyId == comptId)
                                            select new { competenciesMapping.Id }).Count();

            if (CompetenciesMappingCount > 0)
                return true;

            return false;
        }

        public async Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMaster()
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competenciesMaster in context.CompetenciesMaster
                                  join competencyCategory in context.CompetencyCategory on competenciesMaster.CategoryId equals competencyCategory.Id into cat
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
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IApiResponse> GetCompetenciesMaster(int page, int pageSize, string search = null)
        {
            try
            {
                APIResponse<APICompetenciesMaster> aPIResponse = new APIResponse<APICompetenciesMaster>();
                List<APICompetenciesMaster> aPICompetencies = new List<APICompetenciesMaster>();
                var enable_subset = await _courseRepository.GetMasterConfigurableParameterValue("COMP_SUB_SUBCATEGORY");
                if (Convert.ToString(enable_subset).ToLower() == "yes")
                {
                    IQueryable<APICompetenciesMaster> result = null;
                    //using (var context = this.db)
                    //{
                    result = (from competenciesMaster in this.db.CompetenciesMaster
                              join competencyCategory in this.db.CompetencyCategory on competenciesMaster.CategoryId equals competencyCategory.Id
                              into competencyCategory1
                              from competenciesMasterSub in competencyCategory1.DefaultIfEmpty()
                              join competencySubcat in this.db.CompetencySubCategory on competenciesMaster.SubCategoryId equals competencySubcat.Id
                              into competencySubCategory1
                              from competenciesMasterSubSub in competencySubCategory1.DefaultIfEmpty()
                              join competencySubSubcat in this.db.CompetencySubSubCategory on competenciesMaster.SubSubCategoryId equals competencySubSubcat.Id
                              into tempcompetenciesMaster
                              from competenciesMasterAll in tempcompetenciesMaster.DefaultIfEmpty()
                                  //orderby competenciesMaster.Id descending
                              where competenciesMaster.IsDeleted == Record.NotDeleted
                              //&& (competenciesMaster.CategoryId != null || competenciesMaster.CategoryId == 0)
                              select new APICompetenciesMaster
                              {
                                  Id = competenciesMaster.Id,
                                  CategoryId = competenciesMaster.CategoryId,
                                  SubcategoryId = competenciesMaster.SubCategoryId == null ? 0 : competenciesMaster.SubCategoryId,
                                  SubcategoryName = competenciesMasterSubSub.SubcategoryDescription,
                                  SubSubCategoryId = competenciesMaster.SubSubCategoryId == null ? 0 : competenciesMaster.SubSubCategoryId,
                                  SubSubcategoryName = competenciesMasterAll.SubSubcategoryDescription,
                                  CompetencyName = competenciesMaster.CompetencyName,
                                  CompetencyDescription = competenciesMaster.CompetencyDescription,
                                  Category = competenciesMasterSub.Category == null ? "NoCategories" : competenciesMasterSub.Category
                                  //Category = competenciesMasterSub.Category == null ? "NoCategories" : competenciesMasterSub.Category

                              });

                    if (result == null)
                    {
                        return new APIResposeNo { Message = "No Records Found", StatusCode = 204 };
                    }
                    else
                    {


                        if (!string.IsNullOrEmpty(search))
                        {
                            result = result.Where((a => (
                            (Convert.ToString(a.CompetencyName).StartsWith(search) ||
                            Convert.ToString(a.CompetencyDescription).StartsWith(search) ||
                            Convert.ToString(a.Category).StartsWith(search)))));
                        }

                        


                        result = result.OrderByDescending(v => v.Id);
                        if (page != -1)
                        {
                            result = result.Skip((page - 1) * pageSize);

                        }

                        if (pageSize != -1)
                        {
                            result = result.Take(pageSize);

                        }
                        aPIResponse.Data.RecordCount = await result.CountAsync();
                        aPICompetencies = await result.ToListAsync();

                        aPIResponse.Data.Records = aPICompetencies;


                        return aPIResponse;
                        //}
                    }

                }

                else
                {

                    //using (var context = this.db)
                    //{
                    IQueryable<APICompetenciesMaster> result = null;
                    result = (from competenciesMaster in this.db.CompetenciesMaster
                              join competencyCategory in this.db.CompetencyCategory on competenciesMaster.CategoryId equals competencyCategory.Id
                              into tempcompetenciesMaster
                              from competencyCategory in tempcompetenciesMaster.DefaultIfEmpty()
                              join competencySubcat in this.db.CompetencySubCategory on competenciesMaster.SubCategoryId equals competencySubcat.Id
                              into competencySubCategory1
                              from competencySubCategory in competencySubCategory1.DefaultIfEmpty()
                                  // join competencySubSubcat in context.CompetencySubSubCategory on competenciesMaster.SubSubCategoryId equals competencySubSubcat.Id


                              orderby competenciesMaster.Id descending
                              where competenciesMaster.IsDeleted == Record.NotDeleted &&
                              (competenciesMaster.CategoryId != null || competenciesMaster.CategoryId == 0)
                              select new APICompetenciesMaster
                              {
                                  Id = competenciesMaster.Id,
                                  CategoryId = competenciesMaster.CategoryId,
                                  SubcategoryId = competencySubCategory.Id,
                                  SubcategoryName = competencySubCategory.SubcategoryDescription,
                                  // SubSubCategoryId = competenciesMaster.SubSubCategoryId,
                                  // SubSubcategoryName = competencySubSubcat.SubSubcategoryDescription,
                                  CompetencyName = competenciesMaster.CompetencyName,
                                  CompetencyDescription = competenciesMaster.CompetencyDescription,
                                  Category = competencyCategory.Category == null ? "NoCategories" : competencyCategory.Category

                              });



                    //if (result.Count() > 0)
                    //{

                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where((a => ((Convert.ToString(a.CompetencyName).StartsWith(search) || Convert.ToString(a.CompetencyDescription).StartsWith(search) || Convert.ToString(a.Category).StartsWith(search)))));
                    }

                    if (page != -1)
                    {
                        result = result.Skip((page - 1) * pageSize);

                    }

                    if (pageSize != -1)
                    {
                        result = result.Take(pageSize);

                    }
                    aPIResponse.Data.RecordCount = await result.CountAsync();
                    aPICompetencies = await result.ToListAsync();
                    aPIResponse.Data.Records = aPICompetencies;
                    //}
                    return aPIResponse;
                    //}
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMaster(int id)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competenciesMaster in context.CompetenciesMaster
                                  join competencyCategory in context.CompetencyCategory on competenciesMaster.CategoryId equals competencyCategory.Id
                                  into tempCategory
                                  from competencyCategory in tempCategory.DefaultIfEmpty()
                                  join competencySubCategory in context.CompetencySubCategory on competenciesMaster.SubCategoryId equals competencySubCategory.Id
                                  into tempSubCategory
                                  from competencySubCategory in tempSubCategory.DefaultIfEmpty()
                                  join competencySubSubCatgory in context.CompetencySubSubCategory on competenciesMaster.SubSubCategoryId equals competencySubSubCatgory.Id
                                  into tempSubSubCategory
                                  from competencySubSubCategory in tempSubSubCategory.DefaultIfEmpty()
                                  where competenciesMaster.Id == id && competenciesMaster.IsDeleted == Record.NotDeleted
                                  select new APICompetenciesMaster
                                  {
                                      Id = competenciesMaster.Id,
                                      CategoryId = competenciesMaster.CategoryId,
                                      CompetencyName = competenciesMaster.CompetencyName,
                                      CompetencyDescription = competenciesMaster.CompetencyDescription,
                                      Category = competencyCategory.Category,
                                      SubcategoryId = competenciesMaster.SubCategoryId,
                                      SubcategoryName = competencySubCategory.SubcategoryDescription,
                                      SubSubCategoryId = competenciesMaster.SubSubCategoryId,
                                      SubSubcategoryName = competencySubSubCategory.SubSubcategoryDescription

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
        public async Task<IEnumerable<APICompetenciesMaster>> GetCompetenciesMasterByID(int? id)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competenciesMaster in context.CompetenciesMaster
                                  join competencyCategory in context.CompetencyCategory on competenciesMaster.CategoryId equals competencyCategory.Id into cat
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
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<bool> Exists(string CompetencyName)
        {

            var count = await this.db.CompetenciesMaster.Where((p => (p.CompetencyName == CompetencyName) && (p.IsDeleted == Record.NotDeleted))).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<string> GetIdByCompetencyName(string CompetencyName)
        {

            return await db.CompetenciesMaster.Where(c => String.Equals(c.CompetencyName, CompetencyName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Id.ToString()).FirstOrDefaultAsync();
        }

        public async void FindElementsNotInArray(int[] roleCompetencies, int[] aPIJobRoleForIds, int jobroleId)
        {
            var result = aPIJobRoleForIds.Except(roleCompetencies);
            foreach (var res in result)
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            dbContext.Database.ExecuteSqlCommand("Update Course.RoleCompetency set IsDeleted = 1 where CompetencyId = " + res + " and JobRoleId=" + jobroleId);

                        }
                    }
                }

            }
            return;
        }

        public async Task CompetenciesMasterAuditlog(CompetenciesMaster competenciesMaster, string action)
        {
            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "InsertCompetenciesMasterAuditlog";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = competenciesMaster.Id });
                            cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = competenciesMaster.CategoryId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyName", SqlDbType.NVarChar) { Value = competenciesMaster.CompetencyName });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyDescription", SqlDbType.NVarChar) { Value = competenciesMaster.CompetencyDescription });
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = competenciesMaster.CreatedBy });
                            cmd.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = competenciesMaster.CreatedDate });
                            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = competenciesMaster.IsActive });
                            cmd.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = competenciesMaster.IsDeleted });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.Int) { Value = competenciesMaster.ModifiedBy });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime2) { Value = competenciesMaster.ModifiedDate });
                            cmd.Parameters.Add(new SqlParameter("@Action", SqlDbType.NVarChar) { Value = action });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Bulk Upload Competency Master

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

        public async Task<bool> SubCategoryExists(string category)
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
        public async Task<bool> SubSubCategoryExists(string category)
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


        public async Task<string> GetIdByCategory(string Category)
        {
            string category = Category.ToLower().Trim();
            CompetencyCategory competencyCategory = await db.CompetencyCategory.Where(c => c.Category == category && c.IsDeleted == false && c.IsActive == true).FirstOrDefaultAsync();
            if(competencyCategory == null)
            {
                return null;
            }

            return competencyCategory.Id.ToString();

        }

        public async Task<string> GetIdBySubCategory(string Category)
        {
            string category = Category.ToLower().Trim();

            return await db.CompetencySubCategory.Where(c => String.Equals(c.SubcategoryDescription, category)).Select(c => c.Id.ToString()).FirstOrDefaultAsync(); ;

        }
        public async Task<string> GetIdBySubSubCategory(string Category)
        {
            string category = Category.ToLower().Trim();

            return await db.CompetencySubSubCategory.Where(c => String.Equals(c.SubSubcategoryDescription, category)).Select(c => c.Id.ToString()).FirstOrDefaultAsync(); ;

        }

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            var Subcategoryconfig = _courseRepository.GetMasterConfigurableParameterValue("COMP_SUBCATEGORY");
            var SubSubcategoryconfig = _courseRepository.GetMasterConfigurableParameterValue("COMP_SUB_SUBCATEGORY");

            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            if (Convert.ToString(Subcategoryconfig.Result).ToLower() == "yes" && Convert.ToString(SubSubcategoryconfig.Result).ToLower() == "yes")
            {
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.CompetencyName, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.CompetencyDescription, 500));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.Category, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.SubCategory, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.SubSubCategory, 250));
            }
            else if (Convert.ToString(Subcategoryconfig.Result).ToLower() == "yes" && Convert.ToString(SubSubcategoryconfig.Result).ToLower() == "no")
            {
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.CompetencyName, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.CompetencyDescription, 500));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.Category, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.SubCategory, 250));
            }
            else
            {
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.CompetencyName, 250));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.CompetencyDescription, 500));
                columns.Add(new KeyValuePair<string, int>(APICompetencyMasterImportColumns.Category, 250));
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


                DataTable competencymasterImportdt = ReadFile(filepath);
                if (competencymasterImportdt == null || competencymasterImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(competencymasterImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(competencymasterImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
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
            CompetencyMasterList.Clear();

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
                    return false;
            }
            return true;
        }


        public async Task<ApiResponse> ProcessRecordsAsync(DataTable competencymasterImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            List<APICompetencyMasterImport> apiCompetencyMasterImportRejected = new List<APICompetencyMasterImport>();

            

            int columnIndex = 0;
            DataColumnCollection columns = competencymasterImportdt.Columns;

            foreach (string column in importcolumns)
            {
                competencymasterImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }

            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = competencymasterImportdt.Clone();

            if (competencymasterImportdt != null && competencymasterImportdt.Rows.Count > 0)
            {
                List<APICompetencyMasterImport> apiCompetencyMasterImportList = new List<APICompetencyMasterImport>();

                foreach (DataRow dataRow in competencymasterImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column, "CompetencyName", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Competency Name required";
                            }

                        }
                        if (string.Compare(column, "CompetencyDescription", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Competency Description required";
                            }

                        }
                        if (string.Compare(column, "Category", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (await CategoryExists(Convert.ToString(dataRow[column])))
                                {
                                    string y = Convert.ToString(dataRow[column]);
                                    var x = await GetIdByCategory(Convert.ToString(dataRow[column]));
                                    dataRow[column] = x;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Category does not exist, Please enter an existing Category";
                                }
                            }
                            else
                            {
                                dataRow[column] = 0;
                            }
                        }
                        if (string.Compare(column, "SubCategory", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (await SubCategoryExists(Convert.ToString(dataRow[column])))
                                {
                                    string y = Convert.ToString(dataRow[column]);
                                    var x = await GetIdBySubCategory(Convert.ToString(dataRow[column]));
                                    dataRow[column] = x;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "SubCategory does not exist, Please enter an existing SubCategory";
                                }
                            }
                            else
                            {
                                dataRow[column] = 0;
                            }
                        }
                        if (string.Compare(column, "SubSubCategory", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (await SubSubCategoryExists(Convert.ToString(dataRow[column])))
                                {
                                    string y = Convert.ToString(dataRow[column]);
                                    var x = await GetIdBySubSubCategory(Convert.ToString(dataRow[column]));
                                    dataRow[column] = x;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "SubSubCategory does not exist, Please enter an existing SubSubCategory";
                                }
                            }
                            else
                            {
                                dataRow[column] = 0;
                            }
                        }
                    }

                    if (await ExistsRecord(null, Convert.ToString(dataRow[0])))
                    {
                        isError = true;
                        errorMsg = "Competency Name exists, duplicates not allowed";
                    }

                    if (isError)
                    {
                        competencyMasterImport.CompetencyName = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                        competencyMasterImport.CompetencyDescription = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        competencyMasterImport.Category = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;

                        competencyMasterImport.ErrMessage = errorMsg;
                        competencyMasterImport.IsInserted = "false";
                        competencyMasterImport.IsUpdated = "false";
                        competencyMasterImport.InsertedID = null;
                        competencyMasterImport.InsertedCode = "";
                        competencyMasterImport.notInsertedCode = "";
                        dataRow[3] = competencyMasterImport.ErrMessage;
                        apiCompetencyMasterImportList.Add(competencyMasterImport);
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

                    competencyMasterImport = new APICompetencyMasterImport();
                    sb.Clear();

                }

                try
                {

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
                            cmd.CommandText = "dbo.CompetenciesMaster_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 0;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyMasterBulkUpload_TVP", SqlDbType.Structured) { Value = finalDt });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiCompetencyMasterImportList.AddRange(dtResult.ConvertToList<APICompetencyMasterImport>());
                        connection.Close();

                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiCompetencyMasterImportList)
                {
                    if (!string.IsNullOrEmpty(data.CompetencyName) || !string.IsNullOrEmpty(data.CompetencyDescription))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiCompetencyMasterImportRejected.Add(data);
                        }
                    }
                }
            }
            string resultstring = "Total number of records inserted : " + totalRecordInsert + ",  Total number of records rejected : " + totalRecordRejected;

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiCompetencyMasterImportRejected };
            return response;

        }
        #endregion
    }
}
