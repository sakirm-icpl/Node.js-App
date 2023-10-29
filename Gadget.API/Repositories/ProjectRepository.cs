using Gadget.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Gadget.API.Helper;
using Gadget.API.APIModel;
using Microsoft.AspNetCore.Http;
using System.IO;
using log4net;
using Microsoft.Extensions.Configuration;
using AutoMapper;

namespace Gadget.API.Repositories
{

    public class ProjectRepository : Repository<ProjectMaster>,IProjectRepository
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProjectRepository));
        private GadgetDbContext db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;


        public ProjectRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) :base(context)
        {
            this.db = context;
            this._customerConnectionString = customerConnectionString;
            _httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;


        }
        public async Task<int> GetCountByUser(int Userid)
        {
            return await this.db.ProjectMaster.Where(r => r.CreatedBy == Userid).CountAsync();

        }
        public async Task<int> GetCountByUserApplication(int Userid)
        {
            int Count;

            Count = (from  project in db.ProjectTeamDetails
                     join app in db.ProjectApplicationDetails on project.Id equals app.ApplicationId
                     where app.CreatedBy == Userid && app.Status == true
                     select new { project.Id}).Count();


            return  Count;

        }

        public async Task<List<APITileDetails>> GetTileDetails(string search)

        {
            List<APITileDetails> tilelist = new List<APITileDetails>();
            try
            {
                IQueryable<APITileDetails> Query = (from mediaLibrary in db.MediaLibrary
                                                    join mediaLibraryAlbum in db.MediaLibraryAlbum on mediaLibrary.AlbumId equals mediaLibraryAlbum.Id
                                                    where mediaLibrary.IsDeleted == false
                                                    select new APITileDetails
                                                    {
                                                        Name = mediaLibrary.Album,
                                                        Id = mediaLibrary.AlbumId,
                                                        Type = "MediaLibrary",
                                                        Tag = mediaLibrary.Metadata,
                                                        File = null,
                                                        PublishedDate = DateTime.UtcNow,
                                                        VolumeNumber = 0,
                                                        Icon = null
                                                    });

                if (!string.IsNullOrEmpty(search))
                {


                    Query = Query.Where(v => v.Tag.ToLower().Contains(search.ToLower()));
                    Query = Query.OrderByDescending(v => v.Id);
                }


                IQueryable<APITileDetails> QueryPublication = (from publication in db.Publications

                                                               where publication.IsDeleted == false
                                                               select new APITileDetails
                                                               {
                                                                   Name = publication.Publication,
                                                                   Id = publication.Id,
                                                                   Type = "Publication",
                                                                   Tag = publication.Metadata,
                                                                   File = publication.File,
                                                                   PublishedDate = publication.PublishedDate,
                                                                   VolumeNumber = publication.VolumeNumber,
                                                                   Icon = publication.Icon
                                                               });

                if (!string.IsNullOrEmpty(search))
                {


                    QueryPublication = QueryPublication.Where(v => v.Tag.ToLower().Contains(search.ToLower()));
                    QueryPublication = QueryPublication.OrderByDescending(v => v.Id);
                }

                List<APITileDetails> publicationlist =await QueryPublication.ToListAsync();
                List<APITileDetails> medialist =await Query.ToListAsync();

                List<APITileDetails> combinedata = publicationlist.Concat(medialist).ToList();
                //IQueryable<APITileDetails> combinedata = Query.Concat(QueryPublication);


                //return await combinedata.ToListAsync();
                return combinedata;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
            }
            return null;

            //return tilelist;

        }
        public async Task<APIGetProjectTeam> SaveTeamMemberDetails(APIProjectTeamDetails aPIProjectTeamDetails,int Userid)
        {
            ProjectTeamDetails projectTeamDetails = new ProjectTeamDetails();
            APIGetProjectTeam aPIGetProjectTeam = new APIGetProjectTeam();
            //int UserId = 0;
            //string inputuserid = aPIProjectTeamDetails.UserId;
            //try
            //{
            //    using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
            //    {
            //        using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
            //        {
            //            cmd.CommandText = "GetUserDetailsByUserID";
            //            cmd.CommandType = CommandType.StoredProcedure;
            //            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = inputuserid });
            //            await dbContext.Database.OpenConnectionAsync();
            //            DbDataReader reader = await cmd.ExecuteReaderAsync();
            //            DataTable dt = new DataTable();
            //            dt.Load(reader);
            //            if (dt.Rows.Count <= 0)
            //            {
            //                reader.Dispose();
            //                await dbContext.Database.CloseConnectionAsync();
            //                return null;
            //            }
            //            foreach (DataRow row in dt.Rows)
            //            {
            //                UserId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
            //            }
            //            reader.Dispose();
            //            await dbContext.Database.CloseConnectionAsync();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //int ischeck = 0;
            //ischeck = await this.db.ProjectApplicationDetails.Where(a => a.CreatedBy == Userid && a.Status == false).Select(v => v.Id).FirstOrDefaultAsync();
            //if (ischeck != 0)
            //{
            //    return null;
            //}
            ProjectTeamDetails projectappcode = await this.db.ProjectTeamDetails.Where(a => a.IsDeleted == false).OrderByDescending(a => a.Id).FirstOrDefaultAsync();

            if (projectappcode == null)
            {
                aPIGetProjectTeam.ApplicationCode = "KAIZ0001";
            }
            else
            {
                aPIGetProjectTeam.ApplicationCode = "KAIZ000" + (projectappcode.Id + 1);
            }
            projectTeamDetails.UserId = Userid;
            projectTeamDetails.IsDeleted = false;
            projectTeamDetails.CreatedBy = Userid;
            projectTeamDetails.CreatedDate = DateTime.Now;
            projectTeamDetails.TeamMember1 = aPIProjectTeamDetails.TeamMember1;
            projectTeamDetails.TeamMember2 = aPIProjectTeamDetails.TeamMember2;
            projectTeamDetails.TeamMember3 = aPIProjectTeamDetails.TeamMember3;
            projectTeamDetails.TeamMember4 = aPIProjectTeamDetails.TeamMember4;
            projectTeamDetails.ApplicationCode = aPIGetProjectTeam.ApplicationCode;
            await this.db.ProjectTeamDetails.AddAsync(projectTeamDetails);
            db.SaveChanges();

            return aPIGetProjectTeam;
            
        }

        public async Task<string> SaveFile(IFormFile uploadedFile, string fileType, string OrganizationCode,string ApplicationCode)
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string FilePath = string.Empty;
                string ReturnFilePath = string.Empty;
                string FileName = string.Empty;
                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                
                fileDir = Path.Combine(fileDir, OrganizationCode, fileType);
                ReturnFilePath = Path.Combine(OrganizationCode, fileType);

                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }
                FileName = DateTime.Now.Ticks + ApplicationCode + uploadedFile.FileName.Trim();
                FilePath = Path.Combine(fileDir, FileName);
                ReturnFilePath = Path.Combine(ReturnFilePath, FileName);

                FilePath = string.Concat(FilePath.Split(' '));
                ReturnFilePath = string.Concat(ReturnFilePath.Split(' '));

                using (var fs = new FileStream(Path.Combine(FilePath), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fs);
                }
                return ReturnFilePath;
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return "";
            }
        }
        public async Task<APISaveProjectApplication> SaveProjectApplication(APISaveProjectApplication aPISaveProject, int Userid)
        {

            ProjectApplicationDetails projectApplication = new ProjectApplicationDetails();

            int ApplicationId = 0;
             ApplicationId = await db.ProjectTeamDetails.Where(a => a.ApplicationCode == aPISaveProject.ApplicationCode && a.IsDeleted == false).Select
                (v => v.Id).FirstOrDefaultAsync();
            if (ApplicationId == 0)
                return null;

            var obj = await db.ProjectApplicationDetails.Where(a => a.ApplicationId == ApplicationId).FirstOrDefaultAsync();
            if (obj != null)
            {
                obj.Category = aPISaveProject.category;
                obj.TimePeriod = aPISaveProject.timePeriod;
                obj.Answer1 = aPISaveProject.Answer1;
                obj.Answer2 = aPISaveProject.Answer2;
                obj.Answer3 = aPISaveProject.Answer3;
                obj.Answer4 = aPISaveProject.Answer4;
                obj.FilePath1 = aPISaveProject.afterFilePath;
                obj.FilePath2 = aPISaveProject.beforeFilePath;
                obj.Scope = aPISaveProject.scopandplan;
                obj.RefinedClassification = aPISaveProject.refineClassification;
                obj.KaizenClassified = aPISaveProject.kaizenCategory;
                obj.Status = aPISaveProject.status;
                this.db.ProjectApplicationDetails.Update(obj);
                db.SaveChanges();

            }
            else
            {
                // ProjectApplicationDetails _project = Mapper.Map<ProjectApplicationDetails>(aPISaveProject);
                projectApplication.ApplicationId = ApplicationId;
                projectApplication.Category = aPISaveProject.category;
                projectApplication.TimePeriod = aPISaveProject.timePeriod;
                projectApplication.Answer1 = aPISaveProject.Answer1;
                projectApplication.Answer2 = aPISaveProject.Answer2;
                projectApplication.Answer3 = aPISaveProject.Answer3;
                projectApplication.Answer4 = aPISaveProject.Answer4;
                projectApplication.CreatedBy = Userid;
                projectApplication.CreatedDate = DateTime.Now;
                projectApplication.FilePath1 = aPISaveProject.afterFilePath;
                projectApplication.FilePath2 = aPISaveProject.beforeFilePath;
                projectApplication.FileName1 = aPISaveProject.ApplicationCode + "_01";
                projectApplication.FileName2 = aPISaveProject.ApplicationCode + "_02";
                projectApplication.IsDeleted = false;
                projectApplication.ModifiedBy = Userid;
                projectApplication.ModifiedDate = DateTime.Now;
                projectApplication.Scope = aPISaveProject.scopandplan;
                projectApplication.RefinedClassification = aPISaveProject.refineClassification;
                projectApplication.KaizenClassified = aPISaveProject.kaizenCategory;
                projectApplication.Status = aPISaveProject.status;
                projectApplication.AssignmentStatus = "NA";
                projectApplication.AvgScore = 0;
                await this.db.ProjectApplicationDetails.AddAsync(projectApplication);
                db.SaveChanges();

            }
            return aPISaveProject;
        }

        public async Task<APIGetProjectAppDetails> GetProjectUserAppDetails(int UserId)
        {
            APIGetProjectAppDetails aPIGet = new APIGetProjectAppDetails();
            try
            {
                IQueryable<APIGetProjectAppDetails> Data = (from teamdetails in db.ProjectTeamDetails
                              join projectapp in db.ProjectApplicationDetails on teamdetails.Id equals projectapp.ApplicationId
                              where teamdetails.UserId == UserId && teamdetails.IsDeleted == false //&& projectapp.Status == false 
                              orderby projectapp.Id descending
                              select new APIGetProjectAppDetails
                              {
                                  Id = projectapp.Id,
                                  ApplicationCode = teamdetails.ApplicationCode,
                                  category = projectapp.Category,
                                  timePeriod = projectapp.TimePeriod,
                                  Answer1 = projectapp.Answer1,
                                  Answer2 = projectapp.Answer2,
                                  Answer3 = projectapp.Answer3,
                                  Answer4 = projectapp.Answer4,
                                  scopandplan = projectapp.Scope,
                                  afterFileName = projectapp.FileName1,
                                  afterFilePath = projectapp.FilePath1,
                                  beforeFileName = projectapp.FileName2,
                                  beforeFilePath = projectapp.FilePath2,
                                  refineClassification = projectapp.RefinedClassification,
                                  kaizenCategory = projectapp.KaizenClassified,
                                  status = projectapp.Status
                              });
                Data = Data.Where(v => v.status == false );
                if (Data == null || Data.Count() == 0)
                {
                    return new APIGetProjectAppDetails();
                }
                //Data = Data.OrderByDescending(v => v.Id);


                aPIGet = await Data.FirstOrDefaultAsync();
                return aPIGet;

            
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<List<APIGetUserProjectReport>> GetUserProjectReport(int UserId)
        {
            List<APIGetUserProjectReport> tilelist = new List<APIGetUserProjectReport>();
            try
            {
                IQueryable<APIGetUserProjectReport> Query = (from project in db.ProjectMaster
                                                             where project.IsDeleted == false && project.CreatedBy == UserId
                                                             select new APIGetUserProjectReport
                                                             {
                                                                 Id = project.Id,
                                                                 ApplicationCode = project.Id.ToString(),
                                                                 Type = "Nomination",
                                                                 StageOfJourney = "Nomination",
                                                                 Status = "Stage completed",
                                                                 LastDate = new DateTime(2020, 07, 31),
                                                                 ActualDate = project.CreatedDate
                                                             });


                Query = Query.OrderBy(v => v.Id);



                IQueryable<APIGetUserProjectReport> QueryApplication = (from app in db.ProjectApplicationDetails
                                                                        join projectdetails in db.ProjectTeamDetails on app.ApplicationId equals projectdetails.Id
                                                                        where app.IsDeleted == false && app.CreatedBy == UserId && app.Status == true
                                                                        select new APIGetUserProjectReport
                                                                        {
                                                                            Id = app.ApplicationId,
                                                                            ApplicationCode = projectdetails.ApplicationCode,
                                                                            Type = "Application Form",
                                                                            StageOfJourney = "Implemented kaizen application",
                                                                            Status = "Results will be announced by September 14th",
                                                                            LastDate = new DateTime(2020, 09, 10),
                                                                            ActualDate = app.ModifiedDate
                                                                        });


                QueryApplication = QueryApplication.OrderBy(v => v.Id);


                List<APIGetUserProjectReport> applicationlist = await QueryApplication.ToListAsync();
                List<APIGetUserProjectReport> projectlist = await Query.ToListAsync();

                List<APIGetUserProjectReport> combinedata = projectlist.Concat(applicationlist).ToList();
                //IQueryable<APITileDetails> combinedata = Query.Concat(QueryPublication);


                //return await combinedata.ToListAsync();
                return combinedata;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            }
        
        public async Task<APIProjectMaster> GetNominationUserDetails(int Id)
        {
            IQueryable<APIProjectMaster>  aPI = (from project in db.ProjectMaster
                                    where project.IsDeleted == false && project.Id == Id
                                    select new APIProjectMaster
                                    {
                                        IsTeamEntry = project.IsTeamEntry,
                                        TeamSize = project.TeamSize,
                                        Answer1 = project.Answer1,
                                        Answer2 = project.Answer2,
                                        Answer3 = project.Answer3,
                                        CategoryCode = project.CategoryCode,
                                        RowGuid = project.RowGuid
                                    });
            return await aPI.FirstOrDefaultAsync();
        }
        public async Task<APIGetProjectAppDetails> GetApplicationDetailsbyId(int Id, int UserId)
        {
            int flag = 0;
            flag = await db.IdeaApplicationJuryAssocation.Where(r => r.ApplicationId == Id && r.JuryId == UserId).Select(r => r.Id).CountAsync();
            if (flag == 1)
            {
                IQueryable<APIGetProjectAppDetails> aPISave = (from app in db.ProjectApplicationDetails
                                                               join project in db.ProjectTeamDetails on app.ApplicationId equals project.Id
                                                               join ideaass in db.IdeaApplicationJuryAssocation on app.ApplicationId equals ideaass.ApplicationId
                                                               where  (app.IsDeleted == false && app.ApplicationId == Id && ideaass.JuryId == UserId)
                                                               select new APIGetProjectAppDetails
                                                               {
                                                                   Id = app.Id,
                                                                   ApplicationCode = project.ApplicationCode,
                                                                   category = app.Category,
                                                                   timePeriod = app.TimePeriod,
                                                                   Answer1 = app.Answer1,
                                                                   Answer2 = app.Answer2,
                                                                   Answer3 = app.Answer3,
                                                                   Answer4 = app.Answer4,
                                                                   beforeFileName = app.FileName1,
                                                                   beforeFilePath = app.FilePath1,
                                                                   afterFileName = app.FileName2,
                                                                   afterFilePath = app.FilePath2,
                                                                   scopandplan = app.Scope,
                                                                   kaizenCategory = app.KaizenClassified,
                                                                   refineClassification = app.RefinedClassification,
                                                                   status = app.Status,
                                                                   CreatedDate = app.ModifiedDate,
                                                                   //JuryStatus = app.AssignmentStatus
                                                                   JuryStatus = ideaass.JuryComments != null || ideaass.JuryScore != -1 ? "Completed" : "Inprogress",
                                                                   UserId = app.CreatedBy
                                                               });
                
                return await aPISave.FirstOrDefaultAsync();

            }
            else
            {
                IQueryable<APIGetProjectAppDetails> aPISave = (from app in db.ProjectApplicationDetails
                                                               join project in db.ProjectTeamDetails on app.ApplicationId equals project.Id
                                                              // join ideaass in db.IdeaApplicationJuryAssocation on app.ApplicationId equals ideaass.ApplicationId
                                                               where (app.IsDeleted == false && app.ApplicationId == Id) 
                                                               select new APIGetProjectAppDetails
                                                               {
                                                                   Id = app.Id,
                                                                   ApplicationCode = project.ApplicationCode,
                                                                   category = app.Category,
                                                                   timePeriod = app.TimePeriod,
                                                                   Answer1 = app.Answer1,
                                                                   Answer2 = app.Answer2,
                                                                   Answer3 = app.Answer3,
                                                                   Answer4 = app.Answer4,
                                                                   beforeFileName = app.FileName1,
                                                                   beforeFilePath = app.FilePath1,
                                                                   afterFileName = app.FileName2,
                                                                   afterFilePath = app.FilePath2,
                                                                   scopandplan = app.Scope,
                                                                   kaizenCategory = app.KaizenClassified,
                                                                   refineClassification = app.RefinedClassification,
                                                                   status = app.Status,
                                                                   CreatedDate = app.ModifiedDate,
                                                                   UserId = project.CreatedBy,
                                                                   JuryStatus = "Inprogress"
                                                                   //JuryStatus = app.AssignmentStatus
                                                                  // JuryStatus = ideaass.JuryComments != null || ideaass.JuryScore != -1 ? "Completed" : "Inprogress"
                                                               });
                return await aPISave.FirstOrDefaultAsync();
            }

            
        }
        public async Task<List<APIGetAllProjectApplicationList>> GetAll(int userid, int page , int pageSize , string filter = null, string search = null)
        {
            try
            {
                
                APIGetAllProjectApplicationList application = new APIGetAllProjectApplicationList();
                List<APIGetAllProjectApplicationList> applications = new List<APIGetAllProjectApplicationList>();
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllProjectApplicationList";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userid });
                        cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.VarChar) { Value = filter });
                        cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.VarChar) { Value = search });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                application = new APIGetAllProjectApplicationList
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    ApplicationId = string.IsNullOrEmpty(row["ApplicationId"].ToString()) ? 0 : int.Parse(row["ApplicationId"].ToString()),
                                    ApplicationCode = row["ApplicationCode"].ToString(),
                                    UserName = row["UserName"].ToString(),
                                    UserId = (row["UserId"].ToString()).Decrypt(),
                                    Business = row["Business"].ToString(),
                                    UserRegion = row["Region"].ToString(),
                                    StoreCode = row["Location"].ToString(),
                                    FirstJuryName = row["JuryName1"].ToString(),
                                    FirstScore = string.IsNullOrEmpty(row["JuryScore1"].ToString()) ? 0 : double.Parse(row["JuryScore1"].ToString()),
                                    SecondJuryName = row["JuryName2"].ToString(),
                                    SecondScore = string.IsNullOrEmpty(row["JuryScore2"].ToString()) ? 0 : double.Parse(row["JuryScore2"].ToString()),
                                    ThirdJuryName = row["JuryName3"].ToString(),
                                    ThirdScore = string.IsNullOrEmpty(row["JuryScore3"].ToString()) ? 0 : double.Parse(row["JuryScore3"].ToString()),
                                    AvgScore = string.IsNullOrEmpty(row["AvgScore"].ToString()) ? 0 : double.Parse(row["AvgScore"].ToString()),
                                    Status = row["AssignmentStatus"].ToString(),
                                    Type = "Application Form",
                                    FinalStatus = row["FinalStatus"].ToString()
                                    
                                };
                                applications.Add(application);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
                return applications;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
            
        }

        public async Task<int> GetAllCount(string filter = null, string search = null)
        {
            int Count = 0;
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllProjectApplicationListCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = filter });
                        cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = search });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {

                                APIGetAllProjectApplicationList obj = new APIGetAllProjectApplicationList();

                                Count = int.Parse(row["Count"].ToString());

                            }
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;

        }

    }
}

