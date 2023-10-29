// ======================================
// <copyright file="CompetenciesMappingRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Competency.API.APIModel;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Competency.API.APIModel.Competency;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.IO;
using System.Text;
using Competency.API.Common;

namespace Competency.API.Repositories.Competency
{
    public class CompetenciesMappingRepository : Repository<CompetenciesMapping>, ICompetenciesMappingRepository
    {
        StringBuilder sb = new StringBuilder();
        APICompetencyMappingImport competencyMappingImport = new APICompetencyMappingImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesMappingRepository));
        private CourseContext db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly IConfiguration _configuration;
        private readonly ICompetencyLevelsRepository _competencyLevelsRepository;

        public CompetenciesMappingRepository(CourseContext context, ICustomerConnectionStringRepository customerConnectionStringRepository, IConfiguration configuration, ICompetencyLevelsRepository competencyLevelsRepository) : base(context)
        {
            this.db = context;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
            _competencyLevelsRepository = competencyLevelsRepository;
        }
        public async Task<IEnumerable<APICompetenciesMapping>> GetAllCompetenciesMapping(int page, int pageSize, string search = null)
        {

            var result = (from competenciesMapping in this.db.CompetenciesMapping
                          join competencyCategory in this.db.CompetencyCategory on competenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                          into TempCat
                          from competencyCategory in TempCat.DefaultIfEmpty()
                          join competenciesMaster in this.db.CompetenciesMaster on competenciesMapping.CompetencyId equals competenciesMaster.Id
                          join competencyLevels in this.db.CompetencyLevels on competenciesMapping.CompetencyLevelId equals competencyLevels.Id
                          into levelTemp
                          from competencyLevels in levelTemp.DefaultIfEmpty()
                          join course in this.db.Course on competenciesMapping.CourseId equals course.Id
                          join module in this.db.Module on competenciesMapping.ModuleId equals module.Id
                          into moduleTemp
                          from module in moduleTemp.DefaultIfEmpty()
                          join category in this.db.Category on competenciesMapping.CourseCategoryId equals category.Id
                          into tempCat
                          from category in tempCat.DefaultIfEmpty()
                          where competenciesMapping.IsDeleted == Record.NotDeleted
                          select new APICompetenciesMapping
                          {
                              Id = competenciesMapping.Id,
                              CourseCategoryId = competenciesMapping.CourseCategoryId,
                              CompetencyId = competenciesMapping.CompetencyId,
                              CompetencyLevelId = competenciesMapping.CompetencyLevelId,
                              CompetencyCategoryId = competenciesMapping.CompetencyCategoryId,
                              ModuleId = competenciesMapping.ModuleId.Value,
                              CourseId = competenciesMapping.CourseId,
                              Competency = competenciesMaster.CompetencyName,
                              CourseCategory = category == null ? null : category.Name,
                              Course = course.Title,
                              Module = module == null ? null : module.Name,
                              CompetencyCategory = competencyCategory.Category,
                              CompetencyLevel = competencyLevels == null ? null : competencyLevels.LevelName,
                              TrainingType = module == null ? null : module.ModuleType
                          });
            if (!string.IsNullOrEmpty(search))
            {
                result = result.Where(a => ((Convert.ToString(a.CourseCategory).StartsWith(search) || Convert.ToString(a.Course).StartsWith(search) || Convert.ToString(a.Competency).StartsWith(search) || Convert.ToString(a.CompetencyLevel).StartsWith(search))));
            }
            result = result.OrderByDescending(c => c.Id);
            if (page != -1)
                result = result.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                result = result.Take(pageSize);

            return await result.ToListAsync();
        }

        public async Task<IEnumerable<APICompetenciesMapping>> GetAllCompetenciesMappingByCourse(int courseid)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competenciesMapping in context.CompetenciesMapping
                                  join competencyCategory in context.CompetencyCategory on competenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                                  into TempCat
                                  from competencyCategory in TempCat.DefaultIfEmpty()
                                  join competenciesMaster in context.CompetenciesMaster on competenciesMapping.CompetencyId equals competenciesMaster.Id
                                  join course in context.Course on competenciesMapping.CourseId equals course.Id
                                  join category in context.Category on competenciesMapping.CourseCategoryId equals category.Id
                                  join module in context.Module on competenciesMapping.ModuleId equals module.Id
                                  where competenciesMapping.IsDeleted == Record.NotDeleted && competenciesMapping.CourseId == courseid
                                  select new APICompetenciesMapping
                                  {
                                      Id = competenciesMapping.Id,
                                      CourseCategoryId = competenciesMapping.CourseCategoryId,
                                      CompetencyId = competenciesMapping.CompetencyId,
                                      CompetencyCategoryId = competenciesMapping.CompetencyCategoryId,
                                      ModuleId = competenciesMapping.ModuleId.Value,
                                      CourseId = competenciesMapping.CourseId,
                                      Competency = competenciesMaster.CompetencyDescription,
                                      CourseCategory = category.Name,
                                      Course = course.Title,
                                      Module = module.Name,
                                      CompetencyCategory = competencyCategory.Category,
                                      TrainingType = module.ModuleType

                                  });


                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int[]> getCompIdByCourseId(int CourseId)
        {

            int[] IdS = (from c in this.db.CompetenciesMapping
                        // join rolecompetency in db.RoleCompetency on c.Id equals rolecompetency.JobRoleId
                         where c.IsDeleted == false && c.CourseId == CourseId
                         select Convert.ToInt32(c.CompetencyId)).ToArray();
            IdS.LastOrDefault();
            return IdS;
        }

        public async Task<IEnumerable<APICompetencyWiseCourses>> GetCompetencyWiseCourses(int? comId)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competenciesMapping in context.CompetenciesMapping
                                  join competencyCategory in context.CompetencyCategory on competenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                                  into TempCat
                                  from competencyCategory in TempCat.DefaultIfEmpty()
                                  join competenciesMaster in context.CompetenciesMaster on competenciesMapping.CompetencyId equals competenciesMaster.Id
                                  join course in context.Course on competenciesMapping.CourseId equals course.Id
                                  join category in context.Category on competenciesMapping.CourseCategoryId equals category.Id
                                  join module in context.Module on competenciesMapping.ModuleId equals module.Id
                                  where (competenciesMapping.IsDeleted == Record.NotDeleted && ((competenciesMaster.Id == comId) || comId ==null))
                                  select new APICompetencyWiseCourses
                                  {
                                      Id = competenciesMapping.Id,
                                      CourseCategoryId = competenciesMapping.CourseCategoryId,
                                      CompetencyId = competenciesMapping.CompetencyId,
                                      CompetencyCategoryId = competenciesMapping.CompetencyCategoryId,
                                      ModuleId = competenciesMapping.ModuleId.Value,
                                      CourseId = competenciesMapping.CourseId,
                                      Competency = competenciesMaster.CompetencyDescription,
                                      CourseCategory = category.Name,
                                      Course = course.Title,
                                      Module = module.Name,
                                      CompetencyCategory = competencyCategory.Category,
                                      TrainingType = module.ModuleType,
                                      Days = course.CompletionPeriodDays,
                                      CourseCode = course.Code

                                  });

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<CompetenciesMapping>> GetCompetency(int courseId)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competenciesMapping in context.CompetenciesMapping
                                  where (competenciesMapping.IsDeleted == Record.NotDeleted && competenciesMapping.CourseId == courseId)
                                  select new CompetenciesMapping
                                  {
                                      Id = competenciesMapping.Id,
                                      CourseCategoryId = competenciesMapping.CourseCategoryId,
                                      CompetencyId = competenciesMapping.CompetencyId,
                                      CompetencyCategoryId = competenciesMapping.CompetencyCategoryId,
                                      ModuleId = competenciesMapping.ModuleId.Value,
                                      CourseId = competenciesMapping.CourseId

                                  });

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<int> Count(string search = null)
        {

            using (var context = this.db)
            {
                var result = (from competenciesMapping in this.db.CompetenciesMapping
                              join competencyCategory in this.db.CompetencyCategory on competenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                              into TempCat
                              from competencyCategory in TempCat.DefaultIfEmpty()
                              join competenciesMaster in this.db.CompetenciesMaster on competenciesMapping.CompetencyId equals competenciesMaster.Id
                              join competencyLevels in this.db.CompetencyLevels on competenciesMapping.CompetencyLevelId equals competencyLevels.Id
                              into levelTemp
                              from competencyLevels in levelTemp.DefaultIfEmpty()
                              join course in this.db.Course on competenciesMapping.CourseId equals course.Id
                              join module in this.db.Module on competenciesMapping.ModuleId equals module.Id
                              into moduleTemp
                              from module in moduleTemp.DefaultIfEmpty()
                              join category in this.db.Category on competenciesMapping.CourseCategoryId equals category.Id
                              into tempCat
                              from category in tempCat.DefaultIfEmpty()
                              where competenciesMapping.IsDeleted == Record.NotDeleted
                              select new APICompetenciesMapping
                              {
                                  Id = competenciesMapping.Id,
                                  CourseCategoryId = competenciesMapping.CourseCategoryId,
                                  CompetencyId = competenciesMapping.CompetencyId,
                                  CompetencyLevelId = competenciesMapping.CompetencyLevelId,
                                  CompetencyCategoryId = competenciesMapping.CompetencyCategoryId,
                                  ModuleId = competenciesMapping.ModuleId.Value,
                                  CourseId = competenciesMapping.CourseId,
                                  Competency = competenciesMaster.CompetencyDescription,
                                  CourseCategory = category == null ? null : category.Name,
                                  Course = course.Title,
                                  Module = module == null ? null : module.Name,
                                  CompetencyCategory = competencyCategory.Category,
                                  CompetencyLevel = competencyLevels == null ? null : competencyLevels.LevelName,
                                  TrainingType = module == null ? null : module.ModuleType
                              });

                if (!string.IsNullOrEmpty(search))
                {
                    result = result.Where(a => ((Convert.ToString(a.CourseCategory).StartsWith(search) || Convert.ToString(a.Course).StartsWith(search) || Convert.ToString(a.Competency).StartsWith(search) || Convert.ToString(a.CompetencyLevel).StartsWith(search))));
                }

                return await result.CountAsync();


            }


        }
        public async Task<CompetenciesMapping> GetRecordCourse(int courseId)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competenciesMapping in context.CompetenciesMapping
                                  orderby competenciesMapping.Id descending
                                  where (competenciesMapping.IsDeleted == Record.NotDeleted && competenciesMapping.CourseId == courseId)
                                  select new CompetenciesMapping
                                  {
                                      Id = competenciesMapping.Id,
                                      CourseCategoryId = competenciesMapping.CourseCategoryId,
                                      CompetencyId = competenciesMapping.CompetencyId,
                                      CompetencyCategoryId = competenciesMapping.CompetencyCategoryId,
                                      ModuleId = competenciesMapping.ModuleId.Value,
                                      CourseId = competenciesMapping.CourseId
                                  });
                    return await result.FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<int> CountCourse(int couseid)
        {

            return await this.db.CompetenciesMapping.Where(r => ((r.CourseId == couseid) && (r.IsDeleted == Record.NotDeleted))).CountAsync();

        }

        public async Task<bool> Exists(int courseId,  int comId, int? id = null)
        {
            var count = await this.db.CompetenciesMapping.Where(p => ((p.CourseId == courseId) && (p.CompetencyId == comId) && (p.IsDeleted == Record.NotDeleted) && (p.Id != id || id == null))).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<IEnumerable<APICompetenciesMapping>> GetAllCategoryWiseCompetencies(int page, int pageSize, int? jobroleid, string search = null)
        {
            var categories = (from rolecompetency in this.db.RoleCompetency
                              where rolecompetency.IsDeleted == Record.NotDeleted && rolecompetency.JobRoleId ==jobroleid
                              select rolecompetency);
            if (page != -1)
                categories = categories.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                categories = categories.Take(pageSize);

       

            var result = (from rolecompetenciesMapping in categories
                          join compjobrole in this.db.CompetencyJobRole on rolecompetenciesMapping.JobRoleId equals compjobrole.Id
                          join competencyCategory in this.db.CompetencyCategory on rolecompetenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                          into TempCat
                          from competencyCategory in TempCat.DefaultIfEmpty()
                          join competenciesMaster in this.db.CompetenciesMaster on rolecompetenciesMapping.CompetencyId equals competenciesMaster.Id
                          join competencyLevels in this.db.CompetencyLevels on rolecompetenciesMapping.CompetencyLevelId equals competencyLevels.Id
                          into levelTemp
                          from competencyLevels in levelTemp.DefaultIfEmpty()                          
                          select new APICompetenciesMapping
                          {
                              Id = rolecompetenciesMapping.Id,
                              JobRole= compjobrole.Name,
                              CompetencyId =Convert.ToInt32( rolecompetenciesMapping.CompetencyId),
                              Competency = competenciesMaster.CompetencyDescription,
                              CompetencyName = competenciesMaster.CompetencyName,
                              CompetencyCategoryId = rolecompetenciesMapping.CompetencyCategoryId,
                              CompetencyCategory = competencyCategory.Category,
                              CompetencyLevelId = rolecompetenciesMapping.CompetencyLevelId,
                              CompetencyLevel = competencyLevels == null ? null : competencyLevels.LevelName, 
                              HighestLevel = this.db.CompetencyLevels
                                                  .Where(l => l.CategoryId == competencyCategory.Id)
                                                  .OrderByDescending(l => l.LevelName)
                                                  .Select(l => l.LevelName)
                                                  .FirstOrDefault()
                          });

            if (!string.IsNullOrEmpty(search))
            {
                result = result.Where(a => ((Convert.ToString(a.CompetencyName).StartsWith(search) || Convert.ToString(a.JobRole).StartsWith(search))));
            }
            return await result.ToListAsync();
      
        }

        public async Task<int> GetAllCategoryWiseCompetenciesCount(string search = null,int? jobroleid = null)
        {
            var categories = (from rolecompetency in this.db.RoleCompetency
                              where rolecompetency.IsDeleted == Record.NotDeleted && rolecompetency.JobRoleId == jobroleid
                              select rolecompetency);
            
            var categoriesCount = (from rolecompetenciesMapping in categories
                                   join compjobrole in this.db.CompetencyJobRole on rolecompetenciesMapping.JobRoleId equals compjobrole.Id
                                   join competencyCategory in this.db.CompetencyCategory  on rolecompetenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                                   into TempCat
                                   from competencyCategory in TempCat.DefaultIfEmpty()
                                   join competenciesMaster in this.db.CompetenciesMaster on rolecompetenciesMapping.CompetencyId equals competenciesMaster.Id
                                   join competencyLevels in this.db.CompetencyLevels on rolecompetenciesMapping.CompetencyLevelId equals competencyLevels.Id
                                   into levelTemp
                                   from competencyLevels in levelTemp.DefaultIfEmpty()
                                   where competenciesMaster.IsDeleted==false
                                   select new APICompetenciesMapping
                                   {
                                       Id = rolecompetenciesMapping.Id,
                                       JobRole = compjobrole.Name,
                                       CompetencyId = Convert.ToInt32(rolecompetenciesMapping.CompetencyId),
                                       Competency = competenciesMaster.CompetencyDescription,
                                       CompetencyName = competenciesMaster.CompetencyName,
                                       CompetencyCategoryId = rolecompetenciesMapping.CompetencyCategoryId,
                                       CompetencyCategory = competencyCategory.Category,
                                       CompetencyLevelId = rolecompetenciesMapping.CompetencyLevelId,
                                       CompetencyLevel = competencyLevels == null ? null : competencyLevels.LevelName,
                                       HighestLevel = this.db.CompetencyLevels
                                                  .Where(l => l.CategoryId == competencyCategory.Id)
                                                  .OrderByDescending(l => l.LevelName)
                                                  .Select(l => l.LevelName)
                                                  .FirstOrDefault()
                                   });

            if (!string.IsNullOrEmpty(search))
            {
                categoriesCount = categoriesCount.Where(a => ((Convert.ToString(a.CompetencyName).StartsWith(search) || Convert.ToString(a.JobRole).StartsWith(search))));
            }

            return await categoriesCount.Select(category => category.Id).CountAsync();
        }

        public async Task<APICategorywiseCompetenciesDetails> GetCompetanciesDetail(int competencyMappingId, int courseId, int? moduleId, int userId)
        {

            var Assessments = this.db.PostAssessmentResult.Where(a => a.CourseID == courseId && a.ModuleId == moduleId && a.CreatedBy == userId)
                                    .OrderByDescending(a => a.Id).Take(1);

            var result = (from competenciesMapping in this.db.CompetenciesMapping
                          join competencyLevels in this.db.CompetencyLevels on competenciesMapping.CompetencyLevelId equals competencyLevels.Id
                          into levelTemp
                          from competencyLevels in levelTemp.DefaultIfEmpty()
                          join course in this.db.Course on competenciesMapping.CourseId equals course.Id
                          join module in this.db.Module on competenciesMapping.ModuleId equals module.Id
                          into tempModule
                          from module in tempModule.DefaultIfEmpty()
                          join Assessment in Assessments on
                          new { key1 = courseId, key2 = moduleId } equals
                          new { key1 = Assessment.CourseID, key2 = Assessment.ModuleId }
                          into tempAssessment
                          from Assessment in tempAssessment.DefaultIfEmpty()
                          where competenciesMapping.Id == competencyMappingId && competenciesMapping.IsDeleted == Record.NotDeleted
                          select new APICategorywiseCompetenciesDetails
                          {
                              CourseTitle = course.Title,
                              ModuleName = module == null ? null : module.Name,
                              CompetencyLevel = competencyLevels == null ? null : competencyLevels.LevelName,
                              AssessmentScore = Assessment == null ? (double?)null : Assessment.MarksObtained,
                              Percentage = Assessment == null ? (decimal?)null : Assessment.AssessmentPercentage
                          });

            return await result.FirstOrDefaultAsync();
        }



        public async void FindElementsNotInArray(int[] CurrentCompetencies, int[] aPIOldCompetenciesId, int CourseId)
        {
            var result = aPIOldCompetenciesId.Except(CurrentCompetencies);
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
                            dbContext.Database.ExecuteSqlCommand("Update Course.CompetenciesMapping set IsDeleted = 1 where CompetencyId = " + res + " and CourseId=" + CourseId);

                        }
                    }
                }

            }
            return;
        }

        public async Task CompetenciesMappingAuditlog(CompetenciesMapping competenciesMapping,string action)
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
                            cmd.CommandText = "InsertCompetenciesMappingAuditlog";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = competenciesMapping.Id });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyCategoryId", SqlDbType.Int) { Value = competenciesMapping.CompetencyCategoryId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyId", SqlDbType.Int) { Value = competenciesMapping.CompetencyId });
                            cmd.Parameters.Add(new SqlParameter("@CourseCategoryId", SqlDbType.Int) { Value = competenciesMapping.CourseCategoryId });
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = competenciesMapping.CourseId });
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = competenciesMapping.CreatedBy });
                            cmd.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = competenciesMapping.CreatedDate });
                            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = competenciesMapping.IsActive });
                            cmd.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = competenciesMapping.IsDeleted });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.Int) { Value = competenciesMapping.ModifiedBy });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime2) { Value = competenciesMapping.ModifiedDate });
                            cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = competenciesMapping.ModuleId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyLevelId", SqlDbType.Int) { Value = competenciesMapping.CompetencyLevelId });
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

        #region Bulk Upload for Competencies Mapping

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APICompetencyMappingImportColumns.Course, 500));
            columns.Add(new KeyValuePair<string, int>(APICompetencyMappingImportColumns.Category, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencyMappingImportColumns.Competency, 250));
            columns.Add(new KeyValuePair<string, int>(APICompetencyMappingImportColumns.CompetencyLevel, 250));

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

                DataTable competenciesMappingImportdt = ReadFile(filepath);

                if (competenciesMappingImportdt == null || competenciesMappingImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400 , ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(competenciesMappingImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(competenciesMappingImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
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
                    return false;
            }
            return true;
        }

        public async Task<ApiResponse> ProcessRecordsAsync(DataTable competenciesLevelsImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            List<APICompetencyMappingImport> apiCompetencyMappingImportRejected = new List<APICompetencyMappingImport>();

            competenciesLevelsImportdt.Columns.Add("ErrorMessage",typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = competenciesLevelsImportdt.Columns;

            foreach (string column in importcolumns)
            {
                competenciesLevelsImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }

            DataTable finalDt = competenciesLevelsImportdt.Clone();

            if (competenciesLevelsImportdt != null && competenciesLevelsImportdt.Rows.Count > 0)
            {
                List<APICompetencyMappingImport> apiCompetencyMappingImportList = new List<APICompetencyMappingImport>();

                foreach (DataRow dataRow in competenciesLevelsImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = "";

                    int categoryFlag = 0;
                    int competencyFlag = 0;
                    int catId = 0;
                    string compLevel = "";
                    int compLevelId = 0;
                    int? competencyId = 0;
                    int courseId = 0;

                    IEnumerable<APICompetenciesMaster> competenciesByCatId = null;
                    IEnumerable<APICompetenciesMaster> competenciesAll = null;
                    IEnumerable<APICompetencyLevels> competencyLevels = null;

                    foreach (string column in importcolumns)
                    {
                        //TODO Course part
                        if (string.Compare(column, "Course") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                courseId = await GetCourseIdbyTitle(Convert.ToString(dataRow[column]));
                                if (courseId!=0)
                                {
                                    //isErrorDatarow
                                    //dataRow[column] = courseId;
                                    courseId = courseId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Enter a valid or existing Course Title";
                                    break;
                                }
                            }
                            else
                            {
                                isError = true;
                                errorMsg = "Course Title is a required field.";
                                break;
                            }
                        }


                        if (string.Compare(column, "Category") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (await CategoryExists(Convert.ToString(dataRow[column])))
                                {
                                    // if category exists, get competencies by category id
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
                                //get all competencies
                                competenciesAll = await GetCompetenciesMaster();
                                //isErrorDatarow
                                //dataRow[column] = 0;
                            }

                        }

                        if (string.Compare(column, "Competency") == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Competency is a required field.";
                                break;
                            }

                            if (categoryFlag != 0)
                            {
                                //competencyId = competencyInCategoryCheck(competenciesByCatId, Convert.ToString(dataRow[column]));
                                competencyId = competencyInCategoryCheck(competenciesByCatId, Convert.ToString(dataRow[column]));

                                if (competencyId != null)
                                {
                                    competencyFlag = 1;
                                    //isErrorDatarow
                                    //dataRow[column] = competencyId;
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
                                    //get competency ID and assign to datarow[column] of datatable for insertion in table later
                                    competencyFlag = 1;
                                    //isErrorDatarow
                                    //dataRow[column] = competencyId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Competency Name incorrect. Please enter a valid competency.";
                                    break;
                                }
                            }

                        }
                        //level logic
                        if (string.Compare(column,"CompetencyLevel") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                if (competencyId != 0 || competencyId != null)
                                {
                                    //Get the collection of competency levels
                                    //ERROR Possibility if no catId(optional) then it gives an error, should work without catId. However, competencyId is required
                                    if (catId == 0 || catId == null)
                                    {
                                        competencyLevels = await GetAllCompetencyLevelsCat(null, competencyId);

                                    }
                                    else
                                    {
                                        competencyLevels = await GetAllCompetencyLevelsCat(Convert.ToInt32(catId), competencyId);

                                    }

                                    //perform a level check to see if value entered by user is valid and present
                                    List<string> levels = competencyLevels.Select(c => c.LevelName).ToList();
                                    compLevel = LevelChecker(levels,Convert.ToString(dataRow[column]));
                                    
                                    if (compLevel != null)
                                    {
                                        //Assign the ID of the Level to datarow for insertion later in the table
                                        compLevelId =  Convert.ToInt32(competencyLevels.Where(x => x.LevelName == compLevel).Select(level => level.Id).FirstOrDefault()); // need to assign level ID not level name in data for insertion in table
                                        //isErrorDatarow
                                        //dataRow[column] = compLevelId;
                                    }
                                    else
                                    {
                                        isError = true;
                                        errorMsg = "Competency Level incorrect. Please enter a valid level.";
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    if (await Exists(courseId, Convert.ToInt32(competencyId), null))
                    {
                        isError = true;
                        errorMsg = "Course and Competency Mapping exists, Duplicates not allowed.";
                    }

                    if (!isError)
                    {
                        dataRow["Course"]=courseId;
                        dataRow["Category"] =catId;
                        dataRow["Competency"] =competencyId;
                        dataRow["CompetencyLevel"] =compLevelId;
                    }

                    if (isError)
                    {
                        competencyMappingImport.Course = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                        competencyMappingImport.Category = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        competencyMappingImport.Competency = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;
                        competencyMappingImport.CompetencyLevel = dataRow[3] != null ? Convert.ToString(dataRow[3]) : null;

                        competencyMappingImport.ErrMessage = errorMsg;
                        competencyMappingImport.IsInserted = "false";
                        competencyMappingImport.IsUpdated = "false";
                        competencyMappingImport.InsertedID = null;
                        competencyMappingImport.InsertedCode = "";
                        competencyMappingImport.notInsertedCode = "";
                        dataRow[4] = competencyMappingImport.ErrMessage;
                        apiCompetencyMappingImportList.Add(competencyMappingImport);
                    }
                    else
                    {
                        totalRecordInsert++;
                        finalDt.ImportRow(dataRow);
                    }
                    competencyMappingImport = new APICompetencyMappingImport();
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
                            cmd.CommandText = "dbo.CompetencyMapping_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 0;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@CompetencyMappingBulkUpload_TVP", SqlDbType.Structured) { Value = finalDt });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiCompetencyMappingImportList.AddRange(dtResult.ConvertToList<APICompetencyMappingImport>());
                        connection.Close();

                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiCompetencyMappingImportList)
                {
                    if (!string.IsNullOrEmpty(data.Competency) || !string.IsNullOrEmpty(data.Course))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiCompetencyMappingImportRejected.Add(data);
                        }
                    }
                }

            }
            string resultstring = "Total number of records inserted : " + totalRecordInsert + ",  Total number of records rejected : " + totalRecordRejected;


            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiCompetencyMappingImportRejected };
            return response;

        }

        #region levelrepo functions

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

        public string LevelChecker(List<string> levelsList, string level)
        {
            //List<string> levelsList = new List<string>() { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" };
            level = level.Trim();

            string levelReturned = "";
            levelReturned = levelsList.FirstOrDefault(l => string.Equals(l, level, StringComparison.CurrentCultureIgnoreCase));

            if (levelReturned != null || levelReturned != "")
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
            try
            {
                Count = await (from c in this.db.CompetencyCategory
                               where c.IsDeleted == false && (c.Category.ToLower().Equals(category))
                               select new
                               { c.Id }).CountAsync();

                if (Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;

        }

        public async Task<int> GetIdByCategory(string Category)
        {
            string category = Category.ToLower().Trim();

            return await db.CompetencyCategory.Where(c => String.Equals(c.Category, category)).Select(c => c.Id).FirstOrDefaultAsync(); ;

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

        public async Task<IEnumerable<APICompetencyLevels>> GetAllCompetencyLevelsCat(int? CatId, int? ComId)
        {
            try
            {

                    var result = (from competencyLevels in this.db.CompetencyLevels
                                  join competencyCategory in this.db.CompetencyCategory on competencyLevels.CategoryId equals competencyCategory.Id into tempcat
                                  from competencyCategory in tempcat.DefaultIfEmpty()
                                  join competenciesMaster in this.db.CompetenciesMaster on competencyLevels.CompetencyId equals competenciesMaster.Id
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
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        #endregion

        public async Task<int> GetCourseIdbyTitle(string courseTitle)
        {
            courseTitle = courseTitle.Trim();
            try
            {
                
                    var result = (from course in this.db.Course
                                  where course.Title == courseTitle
                                  select course.Id).Single();

                    //return await result.ToListAsync();
                    return Convert.ToInt32(result);
                
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }

        #endregion
    }
}
