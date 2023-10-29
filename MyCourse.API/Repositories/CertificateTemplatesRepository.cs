using MyCourse.API.APIModel;
using MyCourse.API.Helper;
using MyCourse.API.Model;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using System.Collections.Generic;
using MyCourse.API.APIModel.NodalManagement;

namespace MyCourse.API.Repositories
{
    public class CertificateTemplatesRepository : Repository<CertificateTemplates>, ICertificateTemplatesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CertificateTemplatesRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IRewardsPointRepository _rewardsPointRepository;

        public CertificateTemplatesRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection, IRewardsPointRepository rewardsPointRepository) : base(context)
        {
            this._db = context;
            this._customerConnection = customerConnection;
            this._rewardsPointRepository = rewardsPointRepository;

        }
        public async Task<bool> IsCourseCompleted(int courseId, int userId)
        {
            try
            {
                var Course = await (from r in this._db.CertificateTemplates
                                    select new
                                    {
                                        IsCourseCompleted = CourseContext.func_IsCourseCompleted(courseId, userId)
                                    }).FirstOrDefaultAsync();

                if (Course.IsCourseCompleted == 1)
                    return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }

        public async Task<ApiCourseCompletionDetails> GetCourseCompletionDetails(int courseId, int userId)
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCourseCompletionDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        foreach (DataRow row in dt.Rows)
                        {
                            ApiCourseCompletionDetails CourseDetails = new ApiCourseCompletionDetails();
                            CourseDetails.CourseStatus = row["CourseStatus"].ToString();
                            CourseDetails.Title = row["Title"].ToString();
                            CourseDetails.CompletionDate = string.IsNullOrEmpty(row["CompletionDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["CompletionDate"].ToString());
                            CourseDetails.TotalMarks = string.IsNullOrEmpty(row["TotalMarks"].ToString()) ? 0 : float.Parse(row["TotalMarks"].ToString());
                            CourseDetails.AssessmentResult = row["AssessmentResult"].ToString();
                            CourseDetails.EmployeeCode = Security.Decrypt(row["EmployeeCode"].ToString());
                            CourseDetails.Department = row["Department"].ToString();
                            CourseDetails.Position = row["Position"].ToString();
                            CourseDetails.CertificateImageName = row["CourseSpecificImageName"].ToString();
                            CourseDetails.Designation = row["Designation"].ToString();
                            CourseDetails.AuthorityName = row["AuthorityName"].ToString();
                            CourseDetails.CourseStartDate = string.IsNullOrEmpty(row["CourseStartDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["CourseStartDate"].ToString());
                            CourseDetails.CourseEndDate = string.IsNullOrEmpty(row["CourseEndDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["CourseEndDate"].ToString());
                            CourseDetails.Grade = row["Grade"].ToString();
                            CourseDetails.RollNumber = row["RollNumber"].ToString();
                            CourseDetails.CourseCode = row["Code"].ToString();
                            CourseDetails.EmployeeCode = row["EmployeeCode2"]==DBNull.Value? null: row["EmployeeCode2"].ToString();
                            CourseDetails.IssuedBy = row["IssuedBy"] ==DBNull.Value? null: row["IssuedBy"].ToString();

                            CourseDetails.AuthorisedBy = row["AuthorisedBy"] == DBNull.Value ? null : row["AuthorisedBy"].ToString();
                            CourseDetails.Area = row["Area"] == DBNull.Value ? null : row["Area"].ToString();
                            CourseDetails.GroupName = row["GroupName"] == DBNull.Value ? null : row["GroupName"].ToString();
                            CourseDetails.StatusFromImage =Convert.ToBoolean( row["StatusFromImage"].ToString());

                            return CourseDetails;
                        }
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<CourseCertificateData> GetAllCourseCompletionDetails(int courseId, int userId, int page, int pageSize,int loggedInUserId)
        {
            try
            {
                CourseCertificateData courseData = new CourseCertificateData();
                List<ApiCourseCompletionDetails> list = new List<ApiCourseCompletionDetails>();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllCourseCompletionDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@LoggedInUserId", SqlDbType.Int) { Value = loggedInUserId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        foreach (DataRow row in dt.Rows)
                        {
                            ApiCourseCompletionDetails CourseDetails = new ApiCourseCompletionDetails();
                            CourseDetails.CourseStatus = row["CourseStatus"].ToString();
                            CourseDetails.Title = row["Title"].ToString();
                            CourseDetails.CompletionDate = string.IsNullOrEmpty(row["CompletionDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["CompletionDate"].ToString());
                            CourseDetails.TotalMarks = string.IsNullOrEmpty(row["TotalMarks"].ToString()) ? 0 : float.Parse(row["TotalMarks"].ToString());
                            CourseDetails.AssessmentResult = row["AssessmentResult"].ToString();
                            CourseDetails.EmployeeCode = Security.Decrypt(row["EmployeeCode"].ToString());
                            CourseDetails.Department = row["Department"].ToString();
                            CourseDetails.Position = row["Position"].ToString();
                            CourseDetails.CertificateImageName = row["CourseSpecificImageName"].ToString();
                            CourseDetails.Designation = row["Designation"].ToString();
                            CourseDetails.AuthorityName = row["AuthorityName"].ToString();
                            CourseDetails.CourseStartDate = string.IsNullOrEmpty(row["CourseStartDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["CourseStartDate"].ToString());
                            CourseDetails.CourseEndDate = string.IsNullOrEmpty(row["CourseEndDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["CourseEndDate"].ToString());
                            CourseDetails.Grade = row["Grade"].ToString();
                            CourseDetails.RollNumber = row["RollNumber"].ToString();
                            CourseDetails.CourseCode = row["Code"].ToString();
                            CourseDetails.EmployeeCode = row["EmployeeCode2"] == DBNull.Value ? null : row["EmployeeCode2"].ToString();
                            CourseDetails.IssuedBy = row["IssuedBy"] == DBNull.Value ? null : row["IssuedBy"].ToString();

                            CourseDetails.AuthorisedBy = row["AuthorisedBy"] == DBNull.Value ? null : row["AuthorisedBy"].ToString();
                            CourseDetails.Area = row["Area"] == DBNull.Value ? null : row["Area"].ToString();
                            CourseDetails.GroupName = row["GroupName"] == DBNull.Value ? null : row["GroupName"].ToString();
                            CourseDetails.UserName = row["UserName"].ToString();
                            CourseDetails.UserId = Security.Decrypt(row["UserId"].ToString());
                            CourseDetails.UsId = Convert.ToInt32(row["UsId"].ToString());
                            CourseDetails.Id = Convert.ToInt32(row["Id"].ToString());
                            list.Add(CourseDetails);
                        }
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();

                }

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllCourseCompletionDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = 0 });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        foreach (DataRow row in dt.Rows)
                        {
                            courseData.TotalRecords = Convert.ToInt32(row["TotalRecords"].ToString());
                        }
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();

                }

                courseData.data = list;
                return courseData;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<string> AddCerificationDownloadDetails(int userId, int courseId, string OrgCode,string coursTitle)
        {
            CertificateDownloadDetails CertificateDownloadObj = await this._db.CertificateDownloadDetails.Where(c => c.CourseId == courseId && c.UserId == userId).FirstOrDefaultAsync();
            if (CertificateDownloadObj != null)
            {
                if (string.IsNullOrEmpty(CertificateDownloadObj.SerialNumber))
                {
                    CertificateDownloadObj.SerialNumber = GenerateSerialNumber();
                    this._db.CertificateDownloadDetails.Update(CertificateDownloadObj);
                    await this._db.SaveChangesAsync();
                }
                return CertificateDownloadObj.SerialNumber;
            }
            CertificateDownloadObj = new CertificateDownloadDetails();
            CertificateDownloadObj.CourseId = courseId;
            CertificateDownloadObj.UserId = userId;
            CertificateDownloadObj.CreatedDate = DateTime.UtcNow;
            CertificateDownloadObj.SerialNumber = GenerateSerialNumber();
            await this._db.CertificateDownloadDetails.AddAsync(CertificateDownloadObj);
            await this._db.SaveChangesAsync();
            await this._rewardsPointRepository.AddRewardCertificate(courseId, userId,OrgCode, coursTitle);
            return CertificateDownloadObj.SerialNumber;
        }

        private static Random random = new Random();
        public static string GenerateSerialNumber()
        {
            int length = 6;
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<List<APINodalUserDetails>> GetGroupUsers(int GroupId)
        {
            List<APINodalUserDetails> aPINodalUserDetails = new List<APINodalUserDetails>();
            var users = (from req in _db.NodalCourseRequests
                         join user in _db.UserMaster on req.UserId equals user.Id
                         where req.GroupId == GroupId && req.IsDeleted == false
                            && req.IsApprovedByNodal == true
                         select new APINodalUserDetails
                         {
                             UserId = req.UserId,
                             EmployeeCode = user.UserId,
                             UserName = user.UserName
                         });

            aPINodalUserDetails = await users.ToListAsync();
            foreach (APINodalUserDetails item in aPINodalUserDetails)
                item.EmployeeCode = Security.Decrypt(item.EmployeeCode);

            return aPINodalUserDetails;
        }

        public async Task<bool> GetCertificateDownloadStatus(int userId, int courseId)
        {
            CertificateDownloadDetails CertificateDownloadObj = await this._db.CertificateDownloadDetails.Where(c => c.CourseId == courseId && c.UserId == userId).FirstOrDefaultAsync();
            if (CertificateDownloadObj != null)
            {
                return true;
            }           
            return false;
        }
    }
}

