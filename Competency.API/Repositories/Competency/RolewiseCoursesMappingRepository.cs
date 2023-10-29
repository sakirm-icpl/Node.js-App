using Competency.API.APIModel.Competency;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces.Competency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Competency.API.APIModel;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Data;
using OfficeOpenXml;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Competency.API.Common;
using Competency.API.Repositories.Interfaces;

namespace Competency.API.Repositories.Competency
{
    public class RolewiseCoursesMappingRepository : Repository<RolewiseCourseMapping>, IRolewiseCoursesMapping
    {

        StringBuilder sb = new StringBuilder();
        APIRolewiseCourseMappingImport rolewiseCourseMappingImport = new APIRolewiseCourseMappingImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(RolewiseCoursesMappingRepository));
        private CourseContext db;
        private readonly IConfiguration _configuration;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;

        public RolewiseCoursesMappingRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionStringRepository) : base(context)
        {
            this.db = context;
            _configuration = configuration;
            _customerConnectionStringRepository = customerConnectionStringRepository;
        }
        public async Task<IEnumerable<APIRolewiseCoursesMappingDetails>> GetAllRoleCoursesMapping(int page, int pageSize, string search = null, string filter = null)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from rolewiseCourseMapping in context.RolewiseCourseMapping
                                  join courses in context.Course on rolewiseCourseMapping.CourseId equals courses.Id
                                  join jobroles in context.CompetencyJobRole on rolewiseCourseMapping.JobRoleId equals jobroles.Id

                                  where rolewiseCourseMapping.IsDeleted == Record.NotDeleted
                                  select new APIRolewiseCoursesMappingDetails
                                  {
                                      Id = rolewiseCourseMapping.Id,
                                      CourseId = rolewiseCourseMapping.CourseId,
                                      CourseName = courses.Title,
                                      JobRoleId = rolewiseCourseMapping.JobRoleId,
                                      JobRoleName = jobroles.Name,
                                      Active = rolewiseCourseMapping.IsActive,
                                      ApplicableFromDays = rolewiseCourseMapping.ApplicableFromDays
                                  });


                    if (filter == "null")
                        filter = null;
                    if (search == "null")
                        search = null;

                    if (!string.IsNullOrEmpty(search))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            if (filter.ToLower().Equals("title"))
                                result = result.Where(r => r.CourseName.Contains(search));
                            if (filter.ToLower().Equals("jobrole"))
                                result = result.Where(r => r.JobRoleName.Contains(search));
                        }
                        else
                        {
                            result = result.Where(a => ((Convert.ToString(a.CourseId).StartsWith(search) || Convert.ToString(a.JobRoleId).StartsWith(search))));
                        }
                    }

                    if (page != -1)
                        result = result.Skip((page - 1) * pageSize);

                    if (pageSize != -1)
                        result = result.Take(pageSize);

                    return await result.OrderByDescending(r => r.Id).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<IEnumerable<APIRolewiseCoursesMappingDetails>> GetAllCoursesMapping()
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from rolewiseCourseMapping in context.RolewiseCourseMapping
                                  join courses in context.Course on rolewiseCourseMapping.CourseId equals courses.Id
                                  join jobroles in context.CompetencyJobRole on rolewiseCourseMapping.JobRoleId equals jobroles.Id

                                  where rolewiseCourseMapping.IsDeleted == Record.NotDeleted
                                  select new APIRolewiseCoursesMappingDetails
                                  {
                                      Id = rolewiseCourseMapping.Id,
                                      CourseId = rolewiseCourseMapping.CourseId,
                                      CourseName = courses.Title,
                                      JobRoleId = rolewiseCourseMapping.JobRoleId,
                                      JobRoleName = jobroles.Name,
                                      Active = rolewiseCourseMapping.IsActive,
                                      ApplicableFromDays = rolewiseCourseMapping.ApplicableFromDays
                                  });

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }



        public async Task<int> Count(string search = null, string filter = null)
        {

            using (var context = this.db)
            {
                var result = (from rolewiseCourseMapping in context.RolewiseCourseMapping
                              join courses in context.Course on rolewiseCourseMapping.CourseId equals courses.Id
                              join jobroles in context.CompetencyJobRole on rolewiseCourseMapping.JobRoleId equals jobroles.Id

                              where rolewiseCourseMapping.IsDeleted == Record.NotDeleted
                              select new APIRolewiseCoursesMappingDetails
                              {
                                  Id = rolewiseCourseMapping.Id,
                                  CourseId = rolewiseCourseMapping.CourseId,
                                  CourseName = courses.Title,
                                  JobRoleId = rolewiseCourseMapping.JobRoleId,
                                  JobRoleName = jobroles.Name,
                                  Active = rolewiseCourseMapping.IsActive,

                              });


                if (filter == "null")
                    filter = null;
                if (search == "null")
                    search = null;

                if (!string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (filter.ToLower().Equals("title"))
                            result = result.Where(r => r.CourseName.Contains(search));
                        if (filter.ToLower().Equals("jobrole"))
                            result = result.Where(r => r.JobRoleName.Contains(search));
                    }
                    else
                    {
                        result = result.Where(a => ((Convert.ToString(a.CourseId).StartsWith(search) || Convert.ToString(a.JobRoleId).StartsWith(search))));
                    }
                }
                return await result.CountAsync();
            }
        }

        public async Task<bool> Exists(int roleId, int CourseID, int? RolewiseCourseID = null)
        {
            int Count = 0;
            try
            {

                    if (RolewiseCourseID != null)
                    {
                        Count = await (from c in this.db.RolewiseCourseMapping
                                       where c.Id != RolewiseCourseID && c.IsDeleted == false && (c.JobRoleId == roleId) && (c.CourseId == CourseID)
                                       select new
                                       { c.Id }).CountAsync();
                    }
                    else
                    {

                        Count = await this.db.RolewiseCourseMapping.Where(p => ((p.JobRoleId == roleId) && (p.CourseId == CourseID) && (p.IsDeleted == Record.NotDeleted))).CountAsync();
                    }
                    if (Count > 0)
                        return true;
                
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return false;
        }


        public async Task<int> CountRole(int roleid)
        {

            return await this.db.RolewiseCompetenciesMapping.Where(r => ((r.RoleId == roleid) && (r.IsDeleted == Record.NotDeleted))).CountAsync();

        }


        #region bulk upload role wise courses

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APIRolewiseCourseMappingImportColumns.Role, 250));
            columns.Add(new KeyValuePair<string, int>(APIRolewiseCourseMappingImportColumns.Course, 250));
            columns.Add(new KeyValuePair<string, int>(APIRolewiseCourseMappingImportColumns.AssignFromDays, 250));
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


                DataTable rolewiseCoursesMappingImportdt = ReadFile(filepath);
                if (rolewiseCoursesMappingImportdt == null || rolewiseCoursesMappingImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(rolewiseCoursesMappingImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(rolewiseCoursesMappingImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
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

        public async Task<ApiResponse> ProcessRecordsAsync(DataTable rolewiseCoursesMappingImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {

            ApiResponse response = new ApiResponse();
            List<APIRolewiseCourseMappingImport> apiRolewiseCourseMappingImportRejected = new List<APIRolewiseCourseMappingImport>();

            rolewiseCoursesMappingImportdt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = rolewiseCoursesMappingImportdt.Columns;

            foreach (string column in importcolumns)
            {
                rolewiseCoursesMappingImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }

            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = rolewiseCoursesMappingImportdt.Clone();
            DataTable newfinalDt = rolewiseCoursesMappingImportdt.Clone();

            IEnumerable<APICourseWithID> allCoursesList = await GetAllCourses();
            IEnumerable<APIJobRole> allJobRolesList = await GetAllJobRoles();

            if (rolewiseCoursesMappingImportdt != null && rolewiseCoursesMappingImportdt.Rows.Count > 0)
            {

                List<APIRolewiseCourseMappingImport> apiRolewiseCoursesMappingImportList = new List<APIRolewiseCourseMappingImport>();

                foreach (DataRow dataRow in rolewiseCoursesMappingImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = "";

                    int roleId = 0;
                    int courseId = 0;


                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column,"Role") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                roleId = GetRoleIdByName(allJobRolesList,Convert.ToString(dataRow[column]));
                                if (roleId != 0)
                                {
                                    //isErrorDatarow
                                    //dataRow[column] = roleId;
                                    roleId = roleId;
                                }
                                else
                                {
                                    isError = true;
                                    errorMsg = "Enter a valid or existing Role Name";
                                    break;
                                }
                            }
                            else
                            {
                                isError = true;
                                errorMsg = "Role is a required field.";
                               break;
                            }
                        }

                        if (string.Compare(column, "Course") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                courseId = GetCourseIdbyTitle(allCoursesList,Convert.ToString(dataRow[column]));
                                if (courseId != 0)
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
                                errorMsg = "Course is a required field.";
                                break;
                            }
                        }

                        if (string.Compare(column, "AssignFromDays") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                Regex regex = new Regex("^[0-9]+$");
                                if (!regex.IsMatch(Convert.ToString(dataRow[column])))
                                {
                                    isError = true;
                                    errorMsg = "Enter valid No. of Days";
                                    break;
                                }
                            }
                            else
                            {
                                isError = true;
                                errorMsg = "Assign From Days is a required field.";
                                break;
                            }
                        }
                    }

                    if (await Exists(roleId,courseId))
                    {
                        isError = true;
                        errorMsg = "Record with same values already exists.";
                    }

                    if (!isError)
                    {
                        dataRow["Role"]= roleId;
                        dataRow["Course"] = courseId;
                    }

                    if (isError)
                    {
                        rolewiseCourseMappingImport.Role = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                        rolewiseCourseMappingImport.Course = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        rolewiseCourseMappingImport.AssignFromDays = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;
                       
                        rolewiseCourseMappingImport.ErrorMessage = errorMsg;
                        rolewiseCourseMappingImport.IsInserted = "false";
                        rolewiseCourseMappingImport.IsUpdated = "false";
                        rolewiseCourseMappingImport.InsertedID = null;
                        rolewiseCourseMappingImport.InsertedCode = "";
                        rolewiseCourseMappingImport.notInsertedCode = "";
                        dataRow[3] = rolewiseCourseMappingImport.ErrorMessage;
                        apiRolewiseCoursesMappingImportList.Add(rolewiseCourseMappingImport);
                    }
                    else
                    {
                        //totalRecordInsert++;
                        finalDt.ImportRow(dataRow);
                    }

                    rolewiseCourseMappingImport = new APIRolewiseCourseMappingImport();
                    sb.Clear();

                }

                //Remove duplicates from the Excel file DataTable
                newfinalDt = removeDuplicatesRows(finalDt);
                totalRecordInsert = newfinalDt.Rows.Count;
                //totalRecordRejected = (finalDt.Rows.Count) - (newfinalDt.Rows.Count); 

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
                            cmd.CommandText = "dbo.RolewiseCoursesMapping_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 0;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@RolewiseCoursesMappingBulkUpload_TVP", SqlDbType.Structured) { Value = newfinalDt });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiRolewiseCoursesMappingImportList.AddRange(dtResult.ConvertToList<APIRolewiseCourseMappingImport>());
                        connection.Close();

                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiRolewiseCoursesMappingImportList)
                {
                    if (!string.IsNullOrEmpty(data.Role) || !string.IsNullOrEmpty(data.Course) || !string.IsNullOrEmpty(data.AssignFromDays))
                    {
                        if (data.ErrorMessage != null)
                        {
                            totalRecordRejected++;
                            apiRolewiseCourseMappingImportRejected.Add(data);
                        }
                    }
                }
            }
            string resultstring = "Total number of records inserted : " + totalRecordInsert + ",  Total number of records rejected : " + totalRecordRejected + ". Duplicate entries were removed from the file";


            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiRolewiseCourseMappingImportRejected };
            return response;

        }

        public DataTable removeDuplicatesRows(DataTable dt)
        {
            DataTable uniqueCols = dt.DefaultView.ToTable(true, "Role","Course","AssignFromDays","ErrorMessage");
            return uniqueCols;
        }

        public int GetRoleIdByName(IEnumerable<APIJobRole> apiJobRolesList,string roleName)
        {
            roleName = roleName.Trim();

            var result = apiJobRolesList.Where(r => String.Equals(r.Name,roleName,StringComparison.CurrentCultureIgnoreCase)).Select(r=>r.Id).FirstOrDefault();
            return Convert.ToInt32(result);
        }

        public int GetCourseIdbyTitle(IEnumerable<APICourseWithID> apiCoursesList, string courseName)
        {
            courseName = courseName.Trim();

            var result = apiCoursesList.Where(c => String.Equals(c.Title, courseName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Id).FirstOrDefault();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<APICourseWithID>> GetAllCourses()
        {
            try
            {
                    var result = (from courseobj in this.db.Course
                                  where courseobj.IsDeleted == Record.NotDeleted
                                  select new APICourseWithID
                                  {
                                      Id = courseobj.Id,
                                      Title = courseobj.Title
                                  }); 

                    return await result.ToListAsync();
                    
                  

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APIJobRole>> GetAllJobRoles()
        {
            try
            {
                
                    var result = (from competencyJobRole in this.db.CompetencyJobRole
                                  where competencyJobRole.IsDeleted == Record.NotDeleted
                                  select new APIJobRole
                                  {
                                      Id = competencyJobRole.Id,
                                      Name = competencyJobRole.Name

                                  });

                    return await result.ToListAsync();
                
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
