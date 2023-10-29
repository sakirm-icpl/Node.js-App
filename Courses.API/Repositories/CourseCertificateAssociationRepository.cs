using Courses.API.APIModel;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using log4net;
using System.IO;
using Dapper;
using Courses.API.Helper.Metadata;

namespace Courses.API.Repositories
{
    public class CourseCertificateAssociationRepository : Repository<CourseCertificateAssociation>, ICourseCertificateAssociationRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseCertificateAssociationRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;
        ICustomerConnectionStringRepository _customerConnection;
        private ITLSHelper _tLSHelper;
        public CourseCertificateAssociationRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnection, ITLSHelper tLSHelper) : base(context)
        {
            this._db = context;
            this._configuration = configuration;
            this._customerConnection = customerConnection;
            _tLSHelper = tLSHelper;

    }

        public async Task<IEnumerable<APICourseCertificateAssociation>> GetAll(int page, int pageSize, string search = null)
        {
            var Query = (from course in _db.Course
                                               join cca in _db.CourseCertificateAssociation on course.Id equals cca.CourseID
                                                 where course.IsDeleted == false && course.IsActive == true
                                               select new APICourseCertificateAssociation
                                               { Id=cca.Id,CourseID = cca.CourseID, CourseName = course.Title, CertificateImageName= cca.CertificateImageName,Date=cca.Date });
                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(courseModule => courseModule.CourseName.StartsWith(search.ToLower()));
                }
                Query = Query.OrderByDescending(r => r.Id);
               


                if (page != -1)
                    Query = Query.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    Query = Query.Take(pageSize);
          
            return await Query.ToListAsync();


        }

        public async Task<bool> Exists(string CertificateImageName,int courseId,int? recordId,string OrgCode)
        {
            var count = await _db.CourseCertificateAssociation.Where(y => (y.CertificateImageName == CertificateImageName && y.CourseID == courseId)).CountAsync();
            if(count > 0)
                return true;
            return false;
        }
        
        public async Task<CourseCertificateAssociation> CertificateExists(int courseId)
        {
            return _db.CourseCertificateAssociation.Where(y => y.CourseID == courseId).FirstOrDefault();
        }

        public async Task<int> Count(string search = null)
        {
            IQueryable<CourseCertificateAssociation> Query = this._db.CourseCertificateAssociation;

            if (!string.IsNullOrWhiteSpace(search) && (search != "null"))
            {
                Query = Query.Where(r => ((r.CertificateImageName.Contains(search))));

            }
            return await Query.CountAsync();
        }
       
        public async Task<List<APICourseCertificateTypeHead>> GetAllCertificateNames(string search=null)
        {
            try
            {
                IQueryable<APICourseCertificateTypeHead> result = (from certificates in this._db.CourseCertificateAssociation
                                                                  
                                                                   select new APICourseCertificateTypeHead
                                                                   {
                                                                       Name = certificates.CertificateImageName,
                                                                       Id = certificates.Id
                                                                   });
                if (!string.IsNullOrEmpty(search))
                    result = result.Where(a => a.Name.StartsWith(search));
                foreach (APICourseCertificateTypeHead aPICourseCertificateTypeHead in result)
                {
                    APICourseCertificateTypeHead aPICourseCertificateType = new APICourseCertificateTypeHead();
                    aPICourseCertificateType.Name = aPICourseCertificateTypeHead.Name;
                    int index = aPICourseCertificateType.Name.LastIndexOf('/');
                    string CertificateImageName = null;
                    if (index != -1)
                        CertificateImageName = aPICourseCertificateType.Name.Substring(index + 1);
                    else if (index == -1)
                        CertificateImageName = aPICourseCertificateType.Name.Substring(index + 1);
                    aPICourseCertificateType.Name = CertificateImageName;
                    aPICourseCertificateType.Id = aPICourseCertificateTypeHead.Id;
                   
                }

                return await result.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<CourseCertificateAuthority> GetCourseCertificateAuthorities(int Id)
        {
            var query = (from courseCertificateAuthority in _db.CourseCertificateAuthority
                         where courseCertificateAuthority.Id == Id
                         select new CourseCertificateAuthority
                         {
                            Id= courseCertificateAuthority.Id,
                            CourseId= courseCertificateAuthority.CourseId,
                            UserID= courseCertificateAuthority.UserID,
                            DesignationID= courseCertificateAuthority.DesignationID,
                            IsDeleted= courseCertificateAuthority.IsDeleted
                         });
            return query.FirstOrDefault();
        }
        public async Task<int> DeleteRule(int Id)
        {
            try
            {
                List<CourseCertificateAuthority> courseCertificates = new List<CourseCertificateAuthority>();
             CourseCertificateAuthority courseCertificateAuthorities = await this.GetCourseCertificateAuthorities(Id);
                courseCertificateAuthorities.IsActive = false;
                courseCertificateAuthorities.IsDeleted = true;
                courseCertificateAuthorities.CreatedBy = 1;
                courseCertificateAuthorities.CreatedDate = DateTime.Now;
                courseCertificateAuthorities.ModifiedBy = 1;
                courseCertificateAuthorities.ModifiedDate = DateTime.Now;
                this._db.CourseCertificateAuthority.Update(courseCertificateAuthorities);
                this._db.SaveChanges();
            
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 1;
        }

        public async Task<CourseCertificateAuthority> GetById(int Id)
        {
            try
            {
                IQueryable<CourseCertificateAuthority> result = (from certificateAuthority in this._db.CourseCertificateAuthority
                                                                   where certificateAuthority.Id==Id && certificateAuthority.IsDeleted==false
                                                                   select new CourseCertificateAuthority
                                                                   {
                                                                       CourseId = certificateAuthority.CourseId,
                                                                       Id = certificateAuthority.Id,
                                                                       UserID=certificateAuthority.UserID,
                                                                       DesignationID=certificateAuthority.DesignationID

                                                                   });
               
                return await result.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<bool> ExistsCourseCertificate(int CourseID, int UserID, int DesignationId)
        {
            int Count = 0;
            
            {
                Count = await (from c in _db.CourseCertificateAuthority
                               where c.IsDeleted == false && c.UserID == UserID && c.CourseId == CourseID && c.DesignationID==DesignationId
                               select new
                               { c.Id }).CountAsync();
            }
           
            if (Count > 0)
                return true;
            return false;
        }
        public async Task<CourseCertificateAuthority> AddcourseCertificate(CourseCertificateAuthority courseCertificateAuthority)
        {
          
            this._db.CourseCertificateAuthority.Add(courseCertificateAuthority);
            await this._db.SaveChangesAsync();
            return (courseCertificateAuthority);
        }

        public async Task<CertificationUpload> CerticationUpload(ApiCertificationUpload apiCertificationUpload, int UserId)
        {
            try
            {
                if (apiCertificationUpload.UserId == null)
                {
                    apiCertificationUpload.UserId = UserId;
                }

                CertificationUpload certicationUpload = new CertificationUpload();

                certicationUpload.CreatedBy = UserId;
                certicationUpload.ModifiedBy = UserId;
                certicationUpload.CreatedDate = DateTime.Now;
                certicationUpload.ModifiedDate = DateTime.Now;
                certicationUpload.Category = apiCertificationUpload.Category;
                certicationUpload.Type = apiCertificationUpload.Type;
                certicationUpload.StartDate = apiCertificationUpload.StartDate;
                certicationUpload.EndDate = apiCertificationUpload.EndDate;
                certicationUpload.TrainingCode = apiCertificationUpload.TrainingCode;
                certicationUpload.BatchNo = apiCertificationUpload.BatchNo;
                certicationUpload.TrainingName = apiCertificationUpload.TrainingName;
                certicationUpload.PassPercentage = apiCertificationUpload.PassPercentage;
                certicationUpload.TrainingMode = apiCertificationUpload.TrainingMode;
                certicationUpload.TestScore = apiCertificationUpload.TestScore;
                certicationUpload.NoofSessions = apiCertificationUpload.NoofSessions;
                certicationUpload.Result = apiCertificationUpload.Result;
                certicationUpload.TotalNoHours = apiCertificationUpload.TotalNoHours;
                certicationUpload.PartnerName = apiCertificationUpload.PartnerName;
                certicationUpload.Cost = apiCertificationUpload.Cost;
                certicationUpload.CertificatePath = apiCertificationUpload.CertificatePath;
                certicationUpload.Remark = apiCertificationUpload.Remark;
                certicationUpload.UserId = (int)apiCertificationUpload.UserId;
                certicationUpload.Username = apiCertificationUpload.Username;
                certicationUpload.IsDeleted = false;

                this._db.CertificationUpload.Add(certicationUpload);
                await this._db.SaveChangesAsync();
                return (certicationUpload);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<CourseCertificateAuthority> UpdatecourseCertificate(CourseCertificateAuthority courseCertificateAuthority)
        {

            this._db.CourseCertificateAuthority.Update(courseCertificateAuthority);
            await this._db.SaveChangesAsync();
            return (courseCertificateAuthority);
        }
        public async Task<IEnumerable<APICourseCertificateAuthorityDetails>> GetAllCourseCertificateAssociation(int page, int pageSize, string search=null, string searchText=null)
        {
            List<APICourseCertificateAuthorityDetails> CourseCertificateAuthorityList = new List<APICourseCertificateAuthorityDetails>();
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
                            cmd.CommandText = "GetAllCourseCertificateAuthority";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

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
                                APICourseCertificateAuthorityDetails CourseCertificateAuthority = new APICourseCertificateAuthorityDetails();
                                CourseCertificateAuthority.UserID = string.IsNullOrEmpty(row["UserMasterId"].ToString()) ? 0 : int.Parse(row["UserMasterId"].ToString());
                                CourseCertificateAuthority.CourseCertificateId = string.IsNullOrEmpty(row["CourseCertificateId"].ToString()) ? 0 : int.Parse(row["CourseCertificateId"].ToString());
                                CourseCertificateAuthority.CourseName = row["CourseName"].ToString();
                                CourseCertificateAuthority.AuthoristionName=string.IsNullOrEmpty(row["AuthoristionName"].ToString()) ? null : row["AuthoristionName"].ToString();
                                CourseCertificateAuthority.AuthoristionDesignationName = row["AuthoristionDesignationName"].ToString();
                                CourseCertificateAuthority.CourseId =string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                CourseCertificateAuthority.DesignationID = string.IsNullOrEmpty(row["DesignationId"].ToString()) ? 0 : int.Parse(row["DesignationId"].ToString());
                                CourseCertificateAuthority.CreatedDate = row["CreatedDate"].ToString();
                                CourseCertificateAuthorityList.Add(CourseCertificateAuthority);
                            }
                                reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseCertificateAuthorityList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetAllCourseCertificateAssociationCount(string search=null, string filter = null)
        {
            int Count = 0;
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
                            cmd.CommandText = "GetAllCourseCertificateAuthorityCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = filter });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return 0;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                Count = int.Parse(row["Count"].ToString());

                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                 return Count; 
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseCertificateExport>> GetAllCourseCertificateAuthoritiesForExport(int UserId)
        {
            List<APICourseCertificateExport> apiCourseCertificateList = new List<APICourseCertificateExport>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        APICourseCertificateExport apiCourseCertificate = null;
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetCourseCertificateAuthorityExport";
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    apiCourseCertificate = new APICourseCertificateExport
                                    {

                                        UserId = Security.Decrypt(row["UserId"].ToString()),
                                        CourseName = row["CourseName"].ToString(),
                                        AuthoristionName = row["AuthoristionName"].ToString(),
                                        AuthoristionDesignationName = row["AuthoristionDesignationName"].ToString(),
                                        CreatedDate = row["CreatedDate"].ToString()
                                    };
                                    apiCourseCertificateList.Add(apiCourseCertificate);
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                        return apiCourseCertificateList;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<IEnumerable<CertificationUpload>> GetUserCertificatesByUserId(int userId, int page, int pageSize, string filter=null,string search=null)
        {
            try
            {
                IQueryable<CertificationUpload> userCertificates = this._db.CertificationUpload;
                string userRole = (from role in _db.UserMaster
                                where role.Id == userId && role.IsDeleted == false
                                select role.UserRole).First().ToString();
                if (userRole == "EU")
                {
/*                    userCertificates = (from userCerts in _db.CertificationUpload
                                            where userCerts.UserId == userId && userCerts.IsDeleted == false
                                            select userCerts);*/
                    userCertificates = userCertificates.Where(c => c.UserId == userId && c.IsDeleted == false);
                }
                else
                {
/*                    userCertificates = (from userCerts in _db.CertificationUpload
                                            where userCerts.IsDeleted == false
                                            select userCerts);*/
                    userCertificates = userCertificates.Where(c => c.IsDeleted == false);
                }
                

                if (!string.IsNullOrEmpty(filter) && filter != "null")
                {
                    if (!string.IsNullOrEmpty(search) && search != "null")
                    {
                        switch (filter)
                        {
                            case "username":
                                userCertificates = userCertificates.Where(c => c.Username.StartsWith(search));
                                break;
                            case "trainingName":
                                userCertificates = userCertificates.Where(c => c.TrainingName.StartsWith(search));
                                break;
                            case "trainingCode":
                                userCertificates = userCertificates.Where(c => c.TrainingCode.StartsWith(search));
                                break;
                            default:
                                //userCertificates = userCertificates.Where(certName => certName.TrainingName.StartsWith(search.ToLower()) || certName.TrainingCode.StartsWith(search.ToLower()));
                                break;
                        }
                    }
                }

/*                if (!string.IsNullOrEmpty(search) && search != "null")
                {
                    userCertificates = userCertificates.Where(certName => certName.TrainingName.StartsWith(search.ToLower()) || certName.TrainingCode.StartsWith(search.ToLower()));
                }*/


                userCertificates = userCertificates.OrderByDescending(r => r.Id);

                if (page != -1)
                    userCertificates = userCertificates.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    userCertificates = userCertificates.Take(pageSize);

                return await userCertificates.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetUserCertificatesByUserIdCount(int userId,string filter = null,string search = null)
        {
            IQueryable<CertificationUpload> Query = this._db.CertificationUpload;

            string userRole = (from role in _db.UserMaster
                               where role.Id == userId && role.IsDeleted == false
                               select role.UserRole).First().ToString();
            if (userRole == "EU")
            {
                Query = Query.Where(c => c.UserId == userId && c.IsDeleted==false);
            }
            else
            {
                Query = Query.Where(c => c.IsDeleted == false);
            }

            

            if (!string.IsNullOrEmpty(filter) && filter != "null")
            {
                if (!string.IsNullOrEmpty(search) && search != "null")
                {
                    switch (filter)
                    {
                        case "username":
                            Query = Query.Where(c => c.Username.StartsWith(search));
                            break;
                        case "trainingName":
                            Query = Query.Where(c => c.TrainingName.StartsWith(search));
                            break;
                        case "trainingCode":
                            Query = Query.Where(c => c.TrainingCode.StartsWith(search));
                            break;
                        default:
                            //userCertificates = userCertificates.Where(certName => certName.TrainingName.StartsWith(search.ToLower()) || certName.TrainingCode.StartsWith(search.ToLower()));
                            break;
                    }
                }
            }


/*            if (!string.IsNullOrEmpty(search) && (search != "null"))
            {
                Query = Query.Where(r => ((r.TrainingName.Contains(search)) || (r.TrainingCode.Contains(search))) && (r.IsDeleted==false));

            }
            else
            {
                Query = Query.Where(r => (r.IsDeleted == false));
            }*/
            

            return await Query.CountAsync();
        }


        public async Task<CertificationUpload> GetCertificateById(int Id)
        {
            var query = (from courseExtCertificate in _db.CertificationUpload
                         where courseExtCertificate.Id == Id
                         select courseExtCertificate);
            return query.FirstOrDefault();
        }

        public async Task<CertificationUpload> UpdateIntExtCourseCertificate(CertificationUpload courseCertificate)
        {

            this._db.CertificationUpload.Update(courseCertificate);
            await this._db.SaveChangesAsync();
            return (courseCertificate);
        }

        public async Task<int> DeleteCertificate(int certificateId, int userId)
        {
            try
            {

                CertificationUpload courseCertificate = await this.GetCertificateById(certificateId);
                courseCertificate.IsDeleted = true;
                courseCertificate.ModifiedBy = userId;
                courseCertificate.ModifiedDate = DateTime.Now;
                this._db.CertificationUpload.Update(courseCertificate);
                this._db.SaveChanges();
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }

        #region Training Details Catalog

        public async Task<TrainingDetailsCatalog> PostTrainingDetailsCatalog(APITrainingDetailsCatalog apiTrainingDetailsCatalog, int UserId)
        {
            try
            {
/*                if (apiTrainingDetailsCatalog.UserId == null)
                {
                    apiTrainingDetailsCatalog.UserId = UserId;
                }*/

                TrainingDetailsCatalog trainingDetailsCatalog = new TrainingDetailsCatalog();

                trainingDetailsCatalog.CreatedBy = UserId;
                trainingDetailsCatalog.ModifiedBy = UserId;
                trainingDetailsCatalog.CreatedDate = DateTime.Now;
                trainingDetailsCatalog.ModifiedDate = DateTime.Now;
                trainingDetailsCatalog.TrainingCode = apiTrainingDetailsCatalog.TrainingCode;
                trainingDetailsCatalog.TrainingName = apiTrainingDetailsCatalog.TrainingName;
                trainingDetailsCatalog.IsDeleted = false;

                this._db.TrainingDetailsCatalog.Add(trainingDetailsCatalog);
                await this._db.SaveChangesAsync();
                return (trainingDetailsCatalog);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APITrainingDetailsTypeAhead>> GetTrainingNameTypeAhead(string search = null)
        {
            var trainingdetails = (_db.TrainingDetailsCatalog
                           .OrderBy(c => c.TrainingName)
                           .Where(c => c.IsDeleted == false &&
                           (search == null || c.TrainingName.StartsWith(search) || c.TrainingCode.StartsWith(search)))
                           .GroupBy(g => new { g.Id, g.TrainingName })
                          .Select(s => new APITrainingDetailsTypeAhead
                          {

                              Id = s.Max(f => f.Id),
                              TrainingName = s.Max(f => f.TrainingName),
                              TrainingCode = s.Max(f => f.TrainingCode)
                              
                          }));


            return await trainingdetails.ToListAsync();
        }

        public async Task<IEnumerable<TrainingDetailsCatalog>> GetTrainingDetails(int userId, int page, int pageSize, string filter = null, string search = null)
        {
            try
            {
                IQueryable<TrainingDetailsCatalog> trainingDetails = this._db.TrainingDetailsCatalog;

                trainingDetails = trainingDetails.Where(t => t.IsDeleted == false);

                if (!string.IsNullOrEmpty(filter) && filter != "null")
                {
                    if (!string.IsNullOrEmpty(search) && search != "null")
                    {
                        switch (filter)
                        {
                            case "trainingCode":
                                trainingDetails = trainingDetails.Where(c => c.TrainingCode.StartsWith(search));
                                break;
                            case "trainingName":
                                trainingDetails = trainingDetails.Where(c => c.TrainingName.StartsWith(search));
                                break;
                            default:
                                //userCertificates = userCertificates.Where(certName => certName.TrainingName.StartsWith(search.ToLower()) || certName.TrainingCode.StartsWith(search.ToLower()));
                                break;
                        }
                    }
                }


                trainingDetails = trainingDetails.OrderByDescending(r => r.Id);

                if (page != -1)
                    trainingDetails = trainingDetails.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    trainingDetails = trainingDetails.Take(pageSize);

                return await trainingDetails.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetTrainingDetailsCount(int userId, string filter = null, string search = null)
        {
            IQueryable<TrainingDetailsCatalog> Query = this._db.TrainingDetailsCatalog;

            Query = Query.Where(x => x.IsDeleted == false);

            if (!string.IsNullOrEmpty(filter) && filter != "null")
            {
                if (!string.IsNullOrEmpty(search) && search != "null")
                {
                    switch (filter)
                    {
                        case "trainingCode":
                            Query = Query.Where(c => c.TrainingCode.StartsWith(search));
                            break;
                        case "trainingName":
                            Query = Query.Where(c => c.TrainingName.StartsWith(search));
                            break;
                        default:
                            break;
                    }
                }
            }


            return await Query.CountAsync();
        }

        public async Task<TrainingDetailsCatalog> GetTrainingDetailsById(int Id)
        {
            var query = (from trainingDetails in _db.TrainingDetailsCatalog
                         where trainingDetails.Id == Id
                         select trainingDetails);
            return query.FirstOrDefault();
        }

        public async Task<TrainingDetailsCatalog> UpdateTrainingDetailsCatalog(TrainingDetailsCatalog trainingDetailsCatalog)
        {

            this._db.TrainingDetailsCatalog.Update(trainingDetailsCatalog);
            await this._db.SaveChangesAsync();
            return (trainingDetailsCatalog);
        }

        public async Task<int> TrainingDetails(int trainingdetailsId, int userId)
        {
            try
            {

                TrainingDetailsCatalog trainingDetailsCatalog = await this.GetTrainingDetailsById(trainingdetailsId);
                trainingDetailsCatalog.IsDeleted = true;
                trainingDetailsCatalog.ModifiedBy = userId;
                trainingDetailsCatalog.ModifiedDate = DateTime.Now;
                this._db.TrainingDetailsCatalog.Update(trainingDetailsCatalog);
                this._db.SaveChanges();
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }

        #endregion

        public async Task<FileInfo> ExportExtCertificateReport(int UserId, string OrgCode, string Search = null, string SearchText = null, bool IsExport = false)
        {
            IEnumerable<APIExtCertificateReport> aPIExtCertificateReport = await GetExtCertificateReport(UserId, Search, SearchText, true);
            FileInfo fileInfo = await GetExtCertificateReportExcel(aPIExtCertificateReport, OrgCode);
            return fileInfo;
        }

        public async Task<List<APIExtCertificateReport>> GetExtCertificateReport(int UserId, string Search = null, string SearchText = null, bool IsExport = false)
        {

            List<APIExtCertificateReport> aPIExtCertificateReport = new List<APIExtCertificateReport>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        DynamicParameters parameters = new DynamicParameters();
                        //parameters.Add("@UserId", UserId);
                        //parameters.Add("@Page", Page);
                        //parameters.Add("@PageSize", PageSize);
                        parameters.Add("@Search", Search);
                        parameters.Add("@SearchText", SearchText);
                        parameters.Add("@IsExport", IsExport);
                        var result = await SqlMapper.QueryAsync<APIExtCertificateReport>((SqlConnection)connection, "[dbo].[ExportExtCertificateReport]", parameters, null, null, CommandType.StoredProcedure);
                        aPIExtCertificateReport = result.ToList();
                        connection.Close();
                    }
                }
                aPIExtCertificateReport.ForEach(obj =>
                {
                    obj.UserId = !string.IsNullOrEmpty(obj.UserId) ? obj.UserId.Decrypt() : null;

                });
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return aPIExtCertificateReport;

        }

        private async Task<FileInfo> GetExtCertificateReportExcel(IEnumerable<APIExtCertificateReport> aPIExtCertificateReport, string OrgCode)
        {
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();
            List<string> ExportHeader = GetExtCertificateReportHeader();
            ExportData.Add(RowNumber, ExportHeader);
            
            foreach (APIExtCertificateReport extCert in aPIExtCertificateReport)
            {
                List<string> DataRow = new List<string>();
                DataRow = GetExtCertificateReportRowData(extCert);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }

            FileInfo fileInfo = _tLSHelper.GenerateExcelFile(FileName.ExtCertificateReport, ExportData);
            return fileInfo;
        }
        private List<string> GetExtCertificateReportRowData(APIExtCertificateReport courseRequest)
        {
            List<string> ExportData = new List<string>()
            {
                courseRequest.UserId.Decrypt(),
                courseRequest.Username,
                courseRequest.Category,
                courseRequest.Type,
                courseRequest.StartDate.ToString(),
                courseRequest.EndDate.ToString(),
                courseRequest.TrainingCode,
                courseRequest.TrainingName,
                courseRequest.TestScore,
                courseRequest.Result,
                courseRequest.Remark,
                courseRequest.TotalNoHours,
                courseRequest.PartnerName,
                courseRequest.Cost.ToString()
            };
            return ExportData;
        }
        private List<string> GetExtCertificateReportHeader()
        {
            List<string> ExportHeader = new List<string>()
            {
                HeaderName.UserId,
                HeaderName.UserName,
                HeaderName.Category,
                HeaderName.Type,
                HeaderName.StartDate,
                HeaderName.EndDate,
                HeaderName.TrainingCode,
                HeaderName.TrainingName,
                HeaderName.TestScore,
                HeaderName.Result,
                HeaderName.Remark,
                HeaderName.TotalNoHours,
                HeaderName.PartnerName,
                HeaderName.Cost
            };
            return ExportHeader;
        }

    }
}

