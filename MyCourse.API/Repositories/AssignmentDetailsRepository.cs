using MyCourse.API.APIModel;
using MyCourse.API.Common;
using MyCourse.API.Helper;
using MyCourse.API.Model;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
namespace MyCourse.API.Repositories
{
    public class AssignmentDetailsRepository : Repository<AssignmentDetails>, IAssignmentDetailsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssignmentDetailsRepository));
        private CourseContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITLSHelper _tlsHelper;
        private readonly ICourseCompletionStatusRepository _courseCompletionStatusRepository;

        public AssignmentDetailsRepository(
            CourseContext context,
            IHttpContextAccessor httpContextAccessor,
            ICourseCompletionStatusRepository courseCompletionStatusRepository,
            ITLSHelper tlsHelper) : base(context)
        {
            this._db = context;
            _httpContextAccessor = httpContextAccessor;
            this._tlsHelper = tlsHelper;
            _courseCompletionStatusRepository = courseCompletionStatusRepository;
        }

        public async Task<AssignmentDetails> GetAssignmentDetail(int id)
        {
            AssignmentDetails assignmentDetail = await this._db.AssignmentDetails.Where(Assignment => Assignment.Id == id).FirstOrDefaultAsync();
            return assignmentDetail;
        }

        public async Task<AssignmentDetails> GetAssignmentDetailByCourseIdAndAssignmentId( int AssignmentId,int courseId ,int userId )
        {
            AssignmentDetails assignmentDetail = await this._db.AssignmentDetails.Where(Assignment => Assignment.AssignmentId == AssignmentId && Assignment.CourseId == courseId && Assignment.UserId == userId).FirstOrDefaultAsync();
            return assignmentDetail;
        }


        public async Task<string> UpdateAdssignmentDetail(ApiAssignmentDetails apiAssignmentDetails,string OrgCode=null)
        {
            try
            {
                if ((apiAssignmentDetails.Status == Record.Approved || apiAssignmentDetails.Status == Record.Rejected) && !(string.IsNullOrEmpty(apiAssignmentDetails.Remark)))
                {
                    AssignmentDetails assignmentDetail =  await this.GetAssignmentDetailByCourseIdAndAssignmentId(apiAssignmentDetails.AssignmentId, apiAssignmentDetails.CourseId, apiAssignmentDetails.UserId);
                    //Check Assignment already Approved 
                    if (assignmentDetail.Status != Record.Approved)
                    {
                        assignmentDetail.Remark = apiAssignmentDetails.Remark;

                        if (apiAssignmentDetails.Status == Record.Approved)
                            assignmentDetail.Status = Record.Approved;
                        else if (apiAssignmentDetails.Status == Record.Rejected)
                            assignmentDetail.Status = Record.Rejected;

                        await this.Update(assignmentDetail);

                        if (apiAssignmentDetails.Status == Record.Approved)
                        {
                              //Add Course Completion Status
                                CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
                                courseCompletionStatus.CourseId = assignmentDetail.CourseId;
                                courseCompletionStatus.UserId = assignmentDetail.UserId;
                                courseCompletionStatus.Status = Status.Completed;
                                await _courseCompletionStatusRepository.Post(courseCompletionStatus,OrgCode);
                        }
                        return "true";
                    }
                    else
                    {
                        return "Assignment already approved";
                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(apiAssignmentDetails.Remark))
                        return "Remark required";
                    else
                        return "Invalid Status";
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return "BadRequest";
            }

        }

        public async Task<GetUserInfo> GetUserMasterInfo(string UserId)
        {
            UserId = Security.Encrypt(UserId.ToLower());
            GetUserInfo getUserInfo = new GetUserInfo();
            var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetUserMasterInfo";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.NVarChar) { Value = UserId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    foreach (DataRow row in dt.Rows)
                    {
                        getUserInfo.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : Convert.ToInt32(row["Id"].ToString());
                        getUserInfo.UserName = row["UserName"].ToString();
                    }
                    reader.Dispose();
                }
                connection.Close();
                return getUserInfo;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                throw ex;
            }
        }

        public FileInfo GetReportExcel(IEnumerable<ApiAssignmentInfo> apiAssignmentInfo)
        {
            String ExcelName = "AssignmentDetails.xlsx";
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            //Adding Headers for excel file
            List<string> Headers = GetHeaders();
            ExcelData.Add(RowNumber, Headers);

            //Adding data row wise for excel file
            foreach (var row in apiAssignmentInfo)
            {
                List<string> DataRow = GetDataRow(row);
                RowNumber++;
                ExcelData.Add(RowNumber, DataRow);
            }

            FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
            return ExcelFile;
        }

        private List<string> GetDataRow(ApiAssignmentInfo data)
        {

            List<string> Row = new List<string>();
            Row.Add(data.CourseName);
            Row.Add(data.AssignmentName);
            Row.Add(data.UserNameId);
            Row.Add(data.UserName);
            Row.Add(data.Status);
            Row.Add(data.TextAnswer);
            Row.Add(data.Remark);
            Row.Add(data.ModifiedDate);
            return Row;
        }

        private List<string> GetHeaders()
        {
            List<string> Headers = new List<string>();
            Headers.Add("CourseName");
            Headers.Add("AssignmentName");
            Headers.Add("UserId");
            Headers.Add("UserName");
            Headers.Add("Status");
            Headers.Add("TextAnswer");
            Headers.Add("Remark");
            Headers.Add("Last Modified Date");
            return Headers;
        }
        
        public async Task<List<ApiAssignmentInfo>> GetAssignmentDetails(int loginUserId, SearchAssignmentDetails searchAssignmentDetails)
        {
            List<ApiAssignmentInfo> apiAssignmentInfo = new List<ApiAssignmentInfo>();
            var connection = this._db.Database.GetDbConnection();//Dont use Using statement for Connection variable            
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetAssignmentDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = searchAssignmentDetails.Page });
                    cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = searchAssignmentDetails.PageSize });
                    cmd.Parameters.Add(new SqlParameter("@loginUserId", SqlDbType.Int) { Value = loginUserId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = searchAssignmentDetails.UserId });
                    cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = searchAssignmentDetails.CourseId });
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = searchAssignmentDetails.Status });
                    cmd.Parameters.Add(new SqlParameter("@ColumnName", SqlDbType.NVarChar) { Value = searchAssignmentDetails.ColumnName });
                    cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchAssignmentDetails.SearchText });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            ApiAssignmentInfo apiAssignment = new ApiAssignmentInfo();
                            apiAssignment.AssignmentId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : Convert.ToInt32(row["Id"].ToString());
                            apiAssignment.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : Convert.ToInt32(row["CourseId"].ToString());
                            apiAssignment.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? 0 : Convert.ToInt32(row["UserId"].ToString());
                            apiAssignment.UserName = row["UserName"].ToString();
                            apiAssignment.CourseName = row["Title"].ToString();
                            apiAssignment.AssignmentName = row["Name"].ToString();
                            apiAssignment.FilePath = row["FilePath"].ToString();
                            apiAssignment.FileType = row["FileType"].ToString();
                            apiAssignment.TextAnswer = row["TextAnswer"].ToString();
                            apiAssignment.Status = row["Status"].ToString();
                            apiAssignment.Remark = row["Remark"].ToString();
                            apiAssignment.UserNameId = Security.Decrypt(row["UserNameID"].ToString());
                            apiAssignment.ModifiedDate = string.IsNullOrEmpty(row["ModifiedDate"].ToString()) ? null : row["ModifiedDate"].ToString();
                            apiAssignmentInfo.Add(apiAssignment);
                        }
                    }
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                string exception = ex.Message;
            }

             return apiAssignmentInfo;
        }
        
        public void Delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                _db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Course].[AssignmentRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }

        }


        public async Task<string> ProcessImportFile(FileInfo file,
             IAssignmentDetailsRepository _assignmentRepository,
             ICourseRepository _courseRepository,
              int userid, string OrganisationCode)
        {
            string result;
            try
            {
                AssignmentImport.ProcessFile.Reset();
                int resultMessage = await AssignmentImport.ProcessFile.InitilizeAsync(file);
                if (resultMessage == 1)
                {
                    result = await AssignmentImport.ProcessFile.ProcessRecordsAsync(_assignmentRepository, _courseRepository, 
                        userid, OrganisationCode);
                    AssignmentImport.ProcessFile.Reset();
                    return result;
                }
                else if (resultMessage == 2)
                {
                    result = Record.CannotContainNewLineCharacters;
                    AssignmentImport.ProcessFile.Reset();
                    return result;
                }
                else
                {
                    result = Record.FileInvalid;
                    AssignmentImport.ProcessFile.Reset();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Record.FileInvalid;
        }


        public async Task<AssignmentDetailsRejected> AddAssignmentDetailsRejected(AssignmentDetailsRejected apiAssignmentDetails)
        {
            this._db.AssignmentDetailsRejected.Add(apiAssignmentDetails);
            await this._db.SaveChangesAsync();
            return apiAssignmentDetails;
        }

        public async Task<IEnumerable<AssignmentDetailsRejected>> GetAssignmentDetailsRejected(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<AssignmentDetailsRejected> Query = this._db.AssignmentDetailsRejected;

                if (!string.IsNullOrEmpty(search))
                {
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
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.AssignmentDetailsRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.AssignmentDetailsRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }



        public void TRUNCATEAssignmentDetailsRejected()
        {
            try
            {
                //Truncate Table to delete all old records.
                _db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Course].[AssignmentDetailsRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }

        }

    }
}
