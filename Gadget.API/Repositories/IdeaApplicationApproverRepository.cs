using Gadget.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gadget.API.Helper;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Gadget.API.APIModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using log4net;

namespace Gadget.API.Repositories
{
    public class IdeaApplicationApproverRepository : Repository<IdeaAssignJury>,IIdeaApplicationApproverRepository
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IdeaApplicationApproverRepository));
        private GadgetDbContext db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;


        public IdeaApplicationApproverRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(context)
        {
            this.db = context;
            this._customerConnectionString = customerConnectionString;
            _httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;


        }
        public async Task<int> CheckDuplicateInsert(int Userid,string Region,string Jurylevel)
        {
            return await this.db.IdeaAssignJuries.Where(r => r.UserId == Userid && r.Region == Region).CountAsync();

        }
        public async Task<List<IdeaAssignJury>> GetAllAssignJuryDetails(int UserId,int page,int pagesize, string search,string searchText)
        {
            if (searchText == "null")
                searchText = null;
            if (search == "null")
                search = null;
            // List<IdeaAssignJury> assignJury = new List<IdeaAssignJury>();
            IQueryable<IdeaAssignJury> assignJury =  db.IdeaAssignJuries.Where(r => r.IsDeleted == false);
            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (search.ToLower() == "username")
                    {
                        assignJury = assignJury.Where(r => (r.UserName).StartsWith(searchText));
                    }
                    else if (search.ToLower() == "region")
                    {
                        assignJury = assignJury.Where(r => r.Region.StartsWith(searchText));
                    }
                    else if (search.ToLower() == "jurylevel")
                    {
                        assignJury = assignJury.Where(r => (r.Jurylevel).StartsWith(searchText));
                    }
                }
                else if (search == "" || search == null)
                {
                    assignJury = assignJury.Where(r => r.Region.ToLower().Contains(searchText) || r.UserName.ToLower().Contains(searchText) || r.Jurylevel.ToLower().Contains(searchText));
                }

            }
            assignJury =  assignJury.OrderByDescending(r => r.Id);

            if (page != -1)
                assignJury = assignJury.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize));
            if (pagesize != -1)
                assignJury = assignJury.Take(Convert.ToInt32(pagesize));
            return await assignJury.ToListAsync();
        }
   
        public async Task<int> GetAllAssignJuryDetailsCount(string search, string searchText)
        {
            try
            {

                int count = 0;
                //   List<IdeaAssignJury> assignJury = new List<IdeaAssignJury>();
                IQueryable<IdeaAssignJury> assignJury =  db.IdeaAssignJuries.Where(r => r.IsDeleted == false);
                count = assignJury.Count();
                if (searchText == "null")
                    searchText = null;
                if (search == "null")
                    search = null;

                if (!string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        if (search.ToLower() == "username")
                        {
                            assignJury = assignJury.Where(r => (r.UserName).StartsWith(searchText));
                        }
                        else if (search.ToLower() == "region")
                        {
                            assignJury = assignJury.Where(r => r.Region.StartsWith(searchText));
                        }
                        else if (search.ToLower() == "jurylevel")
                        {
                            assignJury = assignJury.Where(r => (r.Jurylevel).StartsWith(searchText));
                        }
                    }
                    else if (search == "" || search == null)
                    {
                        assignJury = assignJury.Where(r => r.Region.ToLower().Contains(searchText) || r.UserName.ToLower().Contains(searchText) || r.Jurylevel.ToLower().Contains(searchText));
                    }
                    count = await assignJury.CountAsync();
                }
                
                    
                return count;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
        }
        public async Task<APIResponse> DeleteJury(int Id)
        {
            IdeaAssignJury ideaAssign = await this.db.IdeaAssignJuries.Where(r => r.Id == Id  && r.IsDeleted== false).FirstOrDefaultAsync();
            APIResponse objApiResponse = new APIResponse();
            if (ideaAssign != null)
            {
                ideaAssign.IsDeleted = true;
                await this.Update(ideaAssign);
                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "Sucess";
            }
            else
            {
                objApiResponse.StatusCode = 412;
                objApiResponse.Description = "Jury is already deleted.";
            }
            return objApiResponse;
        }

        public async Task<int> AssignApplicationToJuries()
        {
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "AssignApplicationToJuries";
                        cmd.CommandType = CommandType.StoredProcedure;
                       
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            return 1;
        }
        public async Task<List<APIIdeaGetAllAppToJury>> GetAllApplicationForJuery(int UserId, int page, int pagesize, string search, string searchText)
        {
            try
            {
                APIIdeaGetAllAppToJury application = new APIIdeaGetAllAppToJury();
                List<APIIdeaGetAllAppToJury> applications = new List<APIIdeaGetAllAppToJury>();
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllApplicationAssignedToJury";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pagesize });
                        cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.VarChar) { Value = searchText });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                application = new APIIdeaGetAllAppToJury
                                {
                                    ApplicationId = string.IsNullOrEmpty(row["ApplicationId"].ToString()) ? 0 : int.Parse(row["ApplicationId"].ToString()),
                                    ApplicationCode = row["ApplicationCode"].ToString(),
                                    UserBusiness = row["Business"].ToString(),
                                    JuryRegion = row["JuryRegion"].ToString(),
                                    UserRegion = row["Region"].ToString(),
                                   
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
        public async Task<int> GetAllApplicationForJueryCount(int UserId,string filter = null, string search = null)
        {
            int Count = 0;
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllApplicationAssignedToJuryCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
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

                                APIIdeaGetAllAppToJury obj = new APIIdeaGetAllAppToJury();

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
        public async Task<int> CheckandUpdate(IdeaApplicationJuryAssocation ideaApplication,int Userid)
        {
            var Data = await this.db.IdeaApplicationJuryAssocation.Where(r => r.JuryId == Userid && r.ApplicationId == ideaApplication.ApplicationId).FirstOrDefaultAsync();
            if (Data != null)
            {
                Data.JuryScore = ideaApplication.JuryScore;
                Data.JuryComments = ideaApplication.JuryComments;
                this.db.IdeaApplicationJuryAssocation.Update(Data);
                await this.db.SaveChangesAsync();
                return 1;

            }
            else
            {
                return 2;
            }
        }
        public async Task<APIResponse> CheckProgressApplicationStatus(int JuryId, int ApplicationId)
        {
            try
            {
                int JuryScoreCount = await this.db.IdeaApplicationJuryAssocation.Where(r => r.ApplicationId == ApplicationId && r.JuryScore >= 0 && r.IsDeleted == false).Select(r => r.Id).CountAsync();
                APIResponse objApiResponse = new APIResponse();
                ProjectApplicationDetails details = await this.db.ProjectApplicationDetails.Where(r => r.ApplicationId == ApplicationId).FirstOrDefaultAsync();

                if (JuryScoreCount == 3)
                {
                   // List<APIIdeaJuryScore> aPIIdeaJuries = new List<APIIdeaJuryScore>();
                   List<double> aPIIdeaJuries = await db.IdeaApplicationJuryAssocation.Where(r => r.ApplicationId == ApplicationId && r.JuryScore >= 0 && r.IsDeleted == false).Select(r => r.JuryScore).ToListAsync();
                    details.AvgScore = aPIIdeaJuries.Sum() / 3;
                    details.AssignmentStatus = "Completed";
                    this.db.ProjectApplicationDetails.Update(details);
                    await this.db.SaveChangesAsync();
                    objApiResponse.StatusCode = 200;
                    objApiResponse.Description = "Completed";
                    return objApiResponse;
                }
                else
                {
                    details.AssignmentStatus = "Inprogress";
                    this.db.ProjectApplicationDetails.Update(details);
                    await this.db.SaveChangesAsync();
                    objApiResponse.StatusCode = 200;
                    objApiResponse.Description = "Inprogress";
                }
               
                return objApiResponse;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
        }

        public async Task<APIResponse> PostStatusforadmin(APIApplicationStatusFromAdmin statusFromAdmin)
        {
            ProjectApplicationDetails details = await this.db.ProjectApplicationDetails.Where(r => r.Id == statusFromAdmin.Id && r.IsDeleted == false).FirstOrDefaultAsync();
            APIResponse objApiResponse = new APIResponse();
            if (details != null)
            {
                details.FinalStatus = statusFromAdmin.Status;
                this.db.ProjectApplicationDetails.Update(details);
                await this.db.SaveChangesAsync();
                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "Sucess";
            }
            else
            {
                objApiResponse.StatusCode = 413;
                objApiResponse.Description = "Invalid Data.";
            }
            return objApiResponse;
        }

    }
}
