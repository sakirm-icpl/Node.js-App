using Competency.API.APIModel;
using Competency.API.APIModel.Competency;
using Competency.API.Common;
using Competency.API.Helper;
using Competency.API.Model;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competency.API.Repositories
{
    public class RoleCompetenciesRepository : Repository<RoleCompetency>, IRoleCompetenciesRepository
    {
        StringBuilder sb = new StringBuilder();
        APIRoleCompetenciesImport roleCompetenciesImport = new APIRoleCompetenciesImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;



        private CourseContext db;

        private readonly IConfiguration _configuration;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RoleCompetenciesRepository));

        public RoleCompetenciesRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionStringRepository) : base(context)
        {
            this.db = context;
            this._configuration = configuration;
            _customerConnectionStringRepository = customerConnectionStringRepository;
        }

        public async Task<IEnumerable<APIRoleCompetency>> GetRoleCompetency(int page, int pagesize, string search = null)
        {

            var Query = (from rolecompetency in db.RoleCompetency
                         join competencyjobrole in db.CompetencyJobRole on rolecompetency.JobRoleId equals competencyjobrole.Id

                         join competenciesMaster in db.CompetenciesMaster on rolecompetency.CompetencyId equals competenciesMaster.Id
                         into tempcompetenciesMaster
                         from competenciesMaster in tempcompetenciesMaster.DefaultIfEmpty()

                         join competencyCategory in db.CompetencyCategory on rolecompetency.CompetencyCategoryId equals competencyCategory.Id
                         into tempcompetencyCategory
                         from competencyCategory in tempcompetencyCategory.DefaultIfEmpty()

                         join competencyLevels in db.CompetencyLevels on rolecompetency.CompetencyLevelId equals competencyLevels.Id
                         into tempcompetencyLevels
                         from competencyLevels in tempcompetencyLevels.DefaultIfEmpty()
                         orderby rolecompetency.Id descending
                         where rolecompetency.IsDeleted == false
                         select new APIRoleCompetency
                         {
                             JobRoleId = rolecompetency.JobRoleId,
                             CompetencyLevelId = rolecompetency.CompetencyLevelId,
                             JobRoleName = competencyjobrole.Name,
                             CompetencyCategoryName = competencyCategory.Category,
                             CompetencyName = competenciesMaster.CompetencyName,
                             CompetencyCategoryId = Convert.ToInt32(rolecompetency.CompetencyCategoryId),
                             CompetencyId = Convert.ToInt32(rolecompetency.CompetencyId),
                             IsActive = rolecompetency.IsActive,
                             IsDeleted = rolecompetency.IsDeleted,
                             CompetencyLevelName = competencyLevels.LevelName,
                             Id = rolecompetency.Id
                         });
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(a => ((Convert.ToString(a.JobRoleName).StartsWith(search) || Convert.ToString(a.CompetencyName).StartsWith(search) || Convert.ToString(a.CompetencyCategoryName).StartsWith(search))));
            }

            if (page != -1)
                Query = Query.Skip((page - 1) * pagesize);

            if (pagesize != -1)
                Query = Query.Take(pagesize);

            return await Query.ToListAsync();

        }


        public async Task<IEnumerable<APICompetencySkill>> GetRoleCompetencyForSearch(string search = null)
        {

            var Query = (from competenciesMaster in db.CompetenciesMaster
                         where competenciesMaster.IsDeleted == false
                         select new APICompetencySkill
                         {
                             CompetencyName = competenciesMaster.CompetencyName,
                             Id = competenciesMaster.Id
                         });
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(a => ((Convert.ToString(a.CompetencyName).StartsWith(search))));
            }
            return await Query.ToListAsync();

        }

        public async Task<int> Count(string search = null)
        {
            var Query = (from rolecompetency in db.RoleCompetency
                         join competencyjobrole in db.CompetencyJobRole on rolecompetency.JobRoleId equals competencyjobrole.Id

                         join competenciesMaster in db.CompetenciesMaster on rolecompetency.CompetencyId equals competenciesMaster.Id
                         into tempcompetenciesMaster
                         from competenciesMaster in tempcompetenciesMaster.DefaultIfEmpty()

                         join competencyCategory in db.CompetencyCategory on rolecompetency.CompetencyCategoryId equals competencyCategory.Id
                         into tempcompetencyCategory
                         from competencyCategory in tempcompetencyCategory.DefaultIfEmpty()

                         join competencyLevels in db.CompetencyLevels on rolecompetency.CompetencyLevelId equals competencyLevels.Id
                         into tempcompetencyLevels
                         from competencyLevels in tempcompetencyLevels.DefaultIfEmpty()
                         where rolecompetency.IsDeleted == false
                         select new APIRoleCompetency
                         {
                             JobRoleId = rolecompetency.JobRoleId,
                             CompetencyLevelId = rolecompetency.CompetencyLevelId,
                             JobRoleName = competencyjobrole.Name,
                             CompetencyCategoryName = competencyCategory.Category,
                             CompetencyName = competenciesMaster.CompetencyName,
                             CompetencyCategoryId = Convert.ToInt32(rolecompetency.CompetencyCategoryId),
                             CompetencyId = Convert.ToInt32(rolecompetency.CompetencyId),
                             IsActive = rolecompetency.IsActive,
                             IsDeleted = rolecompetency.IsDeleted,
                             CompetencyLevelName = competencyLevels.LevelName,
                             Id = rolecompetency.Id
                         });
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(a => ((Convert.ToString(a.JobRoleName).StartsWith(search) || Convert.ToString(a.CompetencyName).StartsWith(search) || Convert.ToString(a.CompetencyCategoryName).StartsWith(search))));
            }
            return Query.Count();

        }

        public async Task<RoleCompetency> CheckForDuplicate(int JobRoleId, int CompetencyLevelId, int CompetencyCategoryId, int CompetencyId, string OrgCode)
        {
            if (OrgCode == "cap" || OrgCode == "ent")
            {
                if ((CompetencyLevelId != 0 && CompetencyCategoryId == 0))
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.JobRoleId == JobRoleId && a.CompetencyId == CompetencyId && a.CompetencyLevelId == CompetencyLevelId && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
                else if ((CompetencyLevelId == 0 && CompetencyCategoryId != 0))
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.JobRoleId == JobRoleId && a.CompetencyId == CompetencyId && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
                else if (CompetencyLevelId == 0 && CompetencyCategoryId == 0)
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.JobRoleId == JobRoleId && a.CompetencyId == CompetencyId && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
                else if (CompetencyLevelId == null || CompetencyCategoryId == null)
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.CompetencyLevelId == CompetencyLevelId
                     && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
                else
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.JobRoleId == JobRoleId
                    && a.CompetencyCategoryId == CompetencyCategoryId && a.CompetencyId == CompetencyId && a.CompetencyLevelId == CompetencyLevelId && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
            }
            else
            {
                if (CompetencyLevelId == 0)
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.CompetencyLevelId == CompetencyLevelId && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
                else if (CompetencyLevelId == null)
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.CompetencyLevelId == CompetencyLevelId
                     && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
                else
                {
                    RoleCompetency obj = new RoleCompetency();
                    obj = await this.db.RoleCompetency.Where(a => a.JobRoleId == JobRoleId
                    && a.CompetencyCategoryId == CompetencyCategoryId && a.CompetencyId == CompetencyId && a.CompetencyLevelId == CompetencyLevelId && a.IsDeleted == false).FirstOrDefaultAsync();
                    return obj;
                }
            }

           
        }

        public async Task<int[]> getallids(int Id)
        {


            int[] IdS = (from c in db.CompetenciesMaster
                         join rolecompetency in db.RoleCompetency on c.Id equals rolecompetency.CompetencyId
                         where c.IsDeleted == false
                         select c.Id).ToArray();
            return IdS;

        }


        public async Task<int[]> GetRoleCompetencyForJobRole(string[] skills)
        {


            int[] IdS = (from c in db.CompetenciesMaster
                         where c.IsDeleted == false && skills.Contains(c.CompetencyName)
                         select c.Id).ToArray();
            return IdS;

        }

        public async Task<string[]> GetRoleCompetencyForJobRole1(string[] skills)
        {


            var Query = (from c in db.CompetenciesMaster

                         where c.IsDeleted == false
                         select new APIRoleCompetency
                         {
                             Id = c.Id,
                             CompetencyName = c.CompetencyName
                         }).ToListAsync();
            return skills;


        }
        public async Task<int[]> getIdByJobRoleId(int jobroleId)
        {

            int[] IdS = (from c in this.db.CompetencyJobRole
                         join rolecompetency in db.RoleCompetency on c.Id equals rolecompetency.JobRoleId
                         //   join competenciesmaster in db.CompetenciesMaster on rolecompetency.CompetencyId equals competenciesmaster.Id

                         where c.IsDeleted == false && c.Id == jobroleId
                         select Convert.ToInt32(rolecompetency.CompetencyId)).ToArray();
            IdS.LastOrDefault();
            return IdS;
        }

        #region Bulk Upload Role Competencies 

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APIRoleCompetenciesImportColumns.Role, 250));
            columns.Add(new KeyValuePair<string, int>(APIRoleCompetenciesImportColumns.Category, 250));
            columns.Add(new KeyValuePair<string, int>(APIRoleCompetenciesImportColumns.Competency, 250));
            columns.Add(new KeyValuePair<string, int>(APIRoleCompetenciesImportColumns.CompetencyLevel, 250));

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

                DataTable roleCompetenciesImportdt = ReadFile(filepath);

                if (roleCompetenciesImportdt == null || roleCompetenciesImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(roleCompetenciesImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(roleCompetenciesImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
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


        public async Task<ApiResponse> ProcessRecordsAsync(DataTable roleCompetenciesImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            List<APIRoleCompetenciesImport> apiRoleCompetenciesImportRejected = new List<APIRoleCompetenciesImport>();

            roleCompetenciesImportdt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = roleCompetenciesImportdt.Columns;

            foreach (string column in importcolumns)
            {
                roleCompetenciesImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }

            DataTable finalDt = roleCompetenciesImportdt.Clone();

            IEnumerable<APIJobRole> allJobRolesList = await GetAllJobRoles();

            if (roleCompetenciesImportdt != null && roleCompetenciesImportdt.Rows.Count > 0)
            {
                List<APIRoleCompetenciesImport> apiRoleCompetenciesImportList = new List<APIRoleCompetenciesImport>();

                foreach (DataRow dataRow in roleCompetenciesImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = "";

                    int roleId = 0;
                    int categoryFlag = 0;
                    int competencyFlag = 0;
                    int catId = 0;
                    string compLevel = "";
                    int compLevelId = 0;
                    int? competencyId = 0;

                    IEnumerable<APICompetenciesMaster> competenciesByCatId = null;
                    IEnumerable<APICompetenciesMaster> competenciesAll = null;
                    IEnumerable<APICompetencyLevels> competencyLevels = null;

                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column, "Role") == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                roleId = GetRoleIdByName(allJobRolesList, Convert.ToString(dataRow[column]));
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
                                catId = 0;
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
                                errorMsg = "Competency required";
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
                        if (string.Compare(column, "CompetencyLevel") == 0)
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
                                        if (competencyLevels == null)
                                        {
                                            isError = true;
                                            errorMsg = "Level for the given Category and Competency entry does not exist.";
                                            break;
                                        }

                                    }

                                    //perform a level check to see if value entered by user is valid and present
                                    List<string> levels = competencyLevels.Select(c => c.LevelName).ToList();
                                    compLevel = LevelChecker(levels, Convert.ToString(dataRow[column]));

                                    if (compLevel != null)
                                    {
                                        //Assign the ID of the Level to datarow for insertion later in the table
                                        compLevelId = Convert.ToInt32(competencyLevels.Where(x => x.LevelName == compLevel).Select(level => level.Id).FirstOrDefault());
                                        //isErrorDatarow
                                        //dataRow[column] = compLevelId; // need to assign level ID not level name in data for insertion in table
                                    }
                                    else
                                    {
                                        isError = true;
                                        errorMsg = "Competency Level incorrect. Please enter a valid level.";
                                        break;
                                    }
                                }

                            }
                            //isErrorDatarow
/*                            else
                                dataRow[column] = 0;*/
                        }
                    }

                    //exists logic 
                    if (!isError)
                    {
                        RoleCompetency dupliobjRoleCompetency = await CheckForDuplicate(roleId, compLevelId, Convert.ToInt32(catId), Convert.ToInt32(competencyId), OrgCode);
                        if (dupliobjRoleCompetency != null)
                        {
                            isError = true;
                            errorMsg = "Duplicate Entry not allowed.";
                        }
                    }

                    if (!isError)
                    {
                        dataRow["Role"] = roleId;
                        dataRow["Category"] = catId;
                        dataRow["Competency"] = competencyId;
                        dataRow["CompetencyLevel"] = compLevelId;
                    }
                    

                    if (isError)
                    {
                        roleCompetenciesImport.Role = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                        roleCompetenciesImport.Category = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        roleCompetenciesImport.Competency = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;
                        roleCompetenciesImport.CompetencyLevel = dataRow[3] != null ? Convert.ToString(dataRow[3]) : null;
                        
                        roleCompetenciesImport.ErrMessage = errorMsg;
                        roleCompetenciesImport.IsInserted = "false";
                        roleCompetenciesImport.IsUpdated = "false";
                        roleCompetenciesImport.InsertedID = null;
                        roleCompetenciesImport.InsertedCode = "";
                        roleCompetenciesImport.notInsertedCode = "";
                        dataRow[4] = roleCompetenciesImport.ErrMessage;
                        apiRoleCompetenciesImportList.Add(roleCompetenciesImport);
                    }
                    else
                    {
                        totalRecordInsert++;
                        finalDt.ImportRow(dataRow);
                    }
                    roleCompetenciesImport = new APIRoleCompetenciesImport();
                    sb.Clear();

                }

                //DB
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
                            cmd.CommandText = "dbo.RoleCompetencies_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 0;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCompetenciesBulkUpload_TVP", SqlDbType.Structured) { Value = finalDt });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }

                        apiRoleCompetenciesImportList.AddRange(dtResult.ConvertToList<APIRoleCompetenciesImport>());
                        connection.Close();

                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiRoleCompetenciesImportList)
                {
                    if (!string.IsNullOrEmpty(data.Competency) || !string.IsNullOrEmpty(data.Role))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiRoleCompetenciesImportRejected.Add(data);
                        }
                    }
                }

            }

            string resultstring = "Total number of records inserted : " + totalRecordInsert + ",  Total number of records rejected : " + totalRecordRejected;


            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiRoleCompetenciesImportRejected };
            return response;

        }

        public int GetRoleIdByName(IEnumerable<APIJobRole> apiJobRolesList, string roleName)
        {
            roleName = roleName.Trim();

            var result = apiJobRolesList.Where(r => String.Equals(r.Name, roleName, StringComparison.CurrentCultureIgnoreCase)).Select(r => r.Id).FirstOrDefault();
            return Convert.ToInt32(result);
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

                if(result.Count() != 0)
                    return await result.AsNoTracking().ToListAsync();
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        #endregion


    #endregion
    }
}


