using AutoMapper;
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
using log4net;
namespace Courses.API.Repositories
{
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;

        public ModuleRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
        }
        public async Task<bool> Exists(string name, int? moduleId = null)
        {
            IQueryable<Module> Query = _db.Module;
            if (moduleId != null)
                Query = Query.Where(m => m.Id != moduleId);
            Query = Query.Where(m => m.IsDeleted == false && m.Name.ToLower().Equals(name));
            if (await Query.CountAsync() > 0)
                return true;
            return false;
        }

        public async Task<List<APIModuleInput>> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            IQueryable<Module> Query = _db.Module;
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(columnName))
            {
                if (columnName.ToLower().Equals("moduletype"))
                    Query = Query.Where(r => r.ModuleType.StartsWith(search));
                if (columnName.ToLower().Equals("name"))
                    Query = Query.Where(r => r.Name.Contains(search));
            }
            else
            {
                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(r => r.Name.Contains(search));
                }
            }
            Query = Query.Where(r => r.IsDeleted == false);
            Query = Query.OrderByDescending(r => r.Id);

            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                Query = Query.Take(pageSize);


            List<Module> course = await Query.ToListAsync();
            List<APIModuleInput> module = new List<APIModuleInput>();
            module = Mapper.Map<List<APIModuleInput>>(course);

            foreach (APIModuleInput item in module)
            {
                ////OPTIMIZATION COMMENT - ALL VALUES SHOULD BE FETCHED USING A SINGLE QUERY.
                int[] modulelcmsassociation = await (from c in this._db.ModuleLcmsAssociation
                                                     where c.IsDeleted == false && c.ModuleId == item.Id
                                                     select Convert.ToInt32(c.LCMSId)).ToArrayAsync();
                item.MultilingualLCMSId = modulelcmsassociation;
            }

            return module;
        }

        public async Task<int> count(string search = null, string columnName = null)
        {
            IQueryable<Module> Query = _db.Module;
            if (!string.IsNullOrEmpty(search) && string.IsNullOrEmpty(columnName))
            {
                Query = Query.Where(r => r.Name.Contains(search));

            }
            else if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(columnName))
            {
                if (columnName.ToLower().Equals("moduletype"))
                    Query = Query.Where(r => r.ModuleType.Contains(search));
                if (columnName.ToLower().Equals("name"))
                    Query = Query.Where(r => r.Name.Contains(search));
            }

            return await Query.Where(r => r.IsDeleted == false).CountAsync();
        }


        public async Task<string> Upload(string filepath)

        {
            throw new NotImplementedException();
        }



        public Task<List<Tuple<Module, APICourseDTO>>> GetModulewithCourseAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TypeAhead>> GetModelTypeAhead(string search, string searchText)
        {
            IQueryable<Module> Query = _db.Module.Where(m => m.IsDeleted == false && m.IsActive == true);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(m => (m.CourseType.ToLower() == search.ToLower()) && m.Name.Contains(searchText));
            }
            Query = Query.OrderByDescending(r => r.Id);
            return await Query.Select(r => new TypeAhead { Id = r.Id, Title = r.Name }).ToListAsync();
        }

        public async Task<List<ILTCourseTypeAhead>> GetModuleByCourse(int CourseId, int UserId, string OrganisationCode)
        {


            List<ILTCourseTypeAhead> ModuleList = new List<ILTCourseTypeAhead>();
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
                            cmd.CommandText = "GetModuleByCourse";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.NVarChar) { Value = OrganisationCode });

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
                                ILTCourseTypeAhead aPIModule = new ILTCourseTypeAhead();

                                aPIModule.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                aPIModule.Title = row["Title"].ToString();
                                aPIModule.Type = row["Type"].ToString();
                                aPIModule.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : int.Parse(row["CourseFee"].ToString());
                                aPIModule.Currency = row["Currency"].ToString();

                                ModuleList.Add(aPIModule);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ModuleList;
        }

        public async Task<List<APIModule>> GetForAssessmentCourse(int page, int pageSize, string search = null)
        {
            List<APIModule> ModuleList = new List<APIModule>();
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
                            cmd.CommandText = "GetForAssessmentCourse";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = search });

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
                                APIModule aPIModule = new APIModule();

                                aPIModule.AssessmentId = string.IsNullOrEmpty(row["AssessmentId"].ToString()) ? 0 : int.Parse(row["AssessmentId"].ToString());
                                aPIModule.AssessmentType = row["AssessmentType"].ToString();
                                aPIModule.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                                aPIModule.CourseType = row["CourseType"].ToString();
                                aPIModule.CreditPoints = string.IsNullOrEmpty(row["CreditPoints"].ToString()) ? 0 : int.Parse(row["CreditPoints"].ToString());
                                aPIModule.Description = row["Description"].ToString();
                                aPIModule.Duration = string.IsNullOrEmpty(row["Duration"].ToString()) ? 0 : int.Parse(row["Duration"].ToString());
                                aPIModule.FeedbackId = string.IsNullOrEmpty(row["FeedbackId"].ToString()) ? 0 : int.Parse(row["FeedbackId"].ToString());
                                aPIModule.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                aPIModule.IsActive = Convert.ToBoolean(row["IsActive"].ToString());
                                aPIModule.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : Convert.ToBoolean(row["IsAssessment"].ToString());
                                aPIModule.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : Convert.ToBoolean(row["IsFeedback"].ToString());
                                aPIModule.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : Convert.ToBoolean(row["IsPreAssessment"].ToString());
                                aPIModule.LCMSId = string.IsNullOrEmpty(row["LCMSId"].ToString()) ? 0 : int.Parse(row["LCMSId"].ToString());
                                aPIModule.ModuleType = row["ModuleType"].ToString();
                                aPIModule.Name = row["Name"].ToString();
                                aPIModule.PreAssessmentId = string.IsNullOrEmpty(row["PreAssessmentId"].ToString()) ? 0 : int.Parse(row["PreAssessmentId"].ToString());
                                aPIModule.SectionId = string.IsNullOrEmpty(row["SectionId"].ToString()) ? 0 : int.Parse(row["SectionId"].ToString());
                                aPIModule.IsNegativeMarking = string.IsNullOrEmpty(row["IsNegativeMarking"].ToString()) ? false : Convert.ToBoolean(row["IsNegativeMarking"].ToString());

                                ModuleList.Add(aPIModule);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ModuleList;
        }

        public async Task<List<Module>> GetForCourse(int page, int pageSize, string moduletype = null, string columnName = null, string searchstring = null)
        {
            IQueryable<Module> Query = _db.Module.Where(m => m.IsDeleted == false && m.IsActive == true);
            if (!string.IsNullOrEmpty(searchstring))
            {
                Query = Query.Where(r => r.Name.Contains(searchstring));

            }

            if (!string.IsNullOrEmpty(moduletype))
            {
                if (moduletype.ToLower().Equals("elearning"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("elearning"));
                }
                else if (moduletype.ToLower().Equals("vilt"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("vilt"));
                }
                else if (moduletype.ToLower().Equals("classroom"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("Classroom"));
                }
                else if (moduletype.ToLower().Equals("feedback"))
                {
                    Query = Query.Where(r => r.CourseType.Equals("Feedback"));
                }
                else if (moduletype.ToLower().Equals("assessment") || moduletype.ToLower().Equals("Certification"))
                {
                    Query = Query.Where(r => r.CourseType.Equals("Assessment"));
                }
                else if (moduletype.ToLower().Equals("memo"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("memo"));
                }
                else if (moduletype.ToLower().Equals("assignment"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("Assignment"));
                }
                else
                {
                    Query = Query.Where(r => r.CourseType != "Feedback");
                }
            }
            Query = Query.OrderByDescending(r => r.Id);

            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                Query = Query.Take(pageSize);


            var course = await Query.ToListAsync();

            return course;
        }

        public async Task<int> coursesmodule_count(string moduletype = null, string columnName = null, string searchstring = null)
        {
            IQueryable<Module> Query = _db.Module;
            if (!string.IsNullOrEmpty(searchstring))
            {
                Query = Query.Where(r => r.Name.Contains(searchstring));

            }
            if (!string.IsNullOrEmpty(moduletype))
            {
                if (moduletype.ToLower().Equals("elearning"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("elearning"));
                }
                else if (moduletype.ToLower().Equals("vilt"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("vilt"));
                }
                else if (moduletype.ToLower().Equals("classroom"))
                {
                    Query = Query.Where(r => r.CourseType.Contains("Classroom"));
                }
                else if (moduletype.ToLower().Equals("feedback"))
                {
                    Query = Query.Where(r => r.CourseType.Equals("Feedback"));
                }
                else if (moduletype.ToLower().Equals("assignment"))
                {
                    Query = Query.Where(r => r.CourseType.Equals("Assignment"));
                }
                else if (moduletype.ToLower().Equals("assessment") || moduletype.ToLower().Equals("Certification"))
                {
                    Query = Query.Where(r => r.CourseType.Equals("Assessment"));
                }
                else
                {
                    Query = Query.Where(r => r.CourseType != "Feedback");
                }
            }

            return await Query.Where(r => r.IsDeleted == false && r.IsActive == true).CountAsync();
        }

        public async Task<object> GetFeedbackConfigurationId(int courseId, int feedbackId, int? moduleId = null)
        {
            return await (from association in _db.CourseModuleAssociation
                          join course in _db.Course on association.CourseId equals course.Id
                          into c
                          from course in c.DefaultIfEmpty()
                          join module in _db.Module on association.ModuleId equals module.Id
                          into m
                          from module in m.DefaultIfEmpty()
                          join lcms in _db.LCMS on module.LCMSId equals lcms.Id
                          into l
                          from lcms in l.DefaultIfEmpty()
                          where (association.CourseId == courseId && association.ModuleId == feedbackId && course.IsDeleted == false)
                          select new
                          {
                              course.Title,
                              module.Name,
                              lcms.FeedbackSheetConfigID
                          }).FirstOrDefaultAsync();
        }

        public async Task<bool> IsDependacyExist(int moduleId)
        {
            int Count = await (from
                                association in _db.CourseModuleAssociation
                               where ((association.ModuleId == moduleId && association.Isdeleted == false) || (association.AssessmentId == moduleId && association.Isdeleted == false)
                               || (association.PreAssessmentId == moduleId && association.Isdeleted == false) || (association.FeedbackId == moduleId && association.Isdeleted == false)
                              )

                               select new { association.Id }).CountAsync();
            if (Count > 0)
                return true;
            return false;
        }

        public async Task<List<TypeAhead>> GetModuleByCourses(int CourseId)
        {
            var Query = await

                (
                from CMS in this._db.CourseModuleAssociation

                join module in this._db.Module on CMS.ModuleId equals module.Id

                where CMS.CourseId == CourseId



                select new TypeAhead
                {
                    Id = module.Id,
                    Title = module.Name
                }


                ).OrderBy(r => r.Id).ToListAsync();

            return Query;
        }

        public async Task<List<TypeAhead>> GetModulesForAssessmentCourses(int CourseId)
        {
            var Query = await

                (
                from CMS in this._db.CourseModuleAssociation

                join module in this._db.Module on CMS.ModuleId equals module.Id

                where CMS.CourseId == CourseId && CMS.Isdeleted == false && (CMS.IsAssessment == true || module.ModuleType == "Assessment")

                select new TypeAhead
                {
                    Id = module.Id,
                    Title = module.Name
                }


                ).OrderBy(r => r.Id).ToListAsync();

            return Query;
        }

        public async Task<List<TypeAhead>> GetTopicByModules(int ModuleId)
        {
            var Query = await

                (
                from MTA in this._db.ModuleTopicAssociation

                join TopicMaster in this._db.TopicMaster on MTA.TopicId equals TopicMaster.ID

                where MTA.ModuleId == ModuleId

                select new TypeAhead
                {
                    Id = TopicMaster.ID,
                    Title = TopicMaster.TopicName
                }


                ).OrderBy(r => r.Id).ToListAsync();

            return Query;
        }

        public async Task<object> AddModuleLcmsData(ModuleLcmsAssociation moduleLcms)
        {

            _db.ModuleLcmsAssociation.Update(moduleLcms);
            await this._db.SaveChangesAsync();

            return moduleLcms;
        }
        public async Task<ModuleLcmsAssociation> GetModuleLcmsData(int moduleid)
        {
            var Query = _db.ModuleLcmsAssociation.Where(r => r.IsDeleted == Record.NotDeleted && r.ModuleId == moduleid);
            ModuleLcmsAssociation ModuleLcmsAssociationDetails = await Query.FirstOrDefaultAsync();
            return ModuleLcmsAssociationDetails;
        }
        public async Task<LCMS> getLcmsById(int lcmsId,string metadata)
        {
            LCMS updateLcmsData =await _db.LCMS.Where(r => r.IsDeleted == Record.NotDeleted && r.Id == lcmsId).FirstOrDefaultAsync();
            if (updateLcmsData != null)
            {
                updateLcmsData.MetaData = metadata;

                _db.LCMS.Update(updateLcmsData);
                await this._db.SaveChangesAsync();
            }
            return updateLcmsData;
        }
        public async Task<bool> UpdateModuleLcmsAssociation(int[] MultilingualLCMSId, int moduleId)
        {
            bool flag = false;
            try
            {

                foreach (int LCMSID in MultilingualLCMSId)
                {
                    ////OPTIMIZATION COMMENT - ALL VALUES SHOULD BE FETCHED USING A SINGLE QUERY.
                    var count = await this._db.ModuleLcmsAssociation.Where((p => (p.ModuleId == moduleId) && (p.LCMSId == LCMSID) && (p.IsDeleted == Record.NotDeleted))).CountAsync();
                    if (count > 0)
                        continue;
                    else
                    {
                        ModuleLcmsAssociation moduleLcms = new ModuleLcmsAssociation();
                        moduleLcms.CreatedDate = DateTime.UtcNow;
                        moduleLcms.ModifiedDate = DateTime.UtcNow;
                        moduleLcms.CreatedBy = 0;
                        moduleLcms.ModuleId = moduleId;
                        moduleLcms.LCMSId = Convert.ToInt32(LCMSID);
                        await this.AddModuleLcmsData(moduleLcms);
                    }

                }

                int[] MultilingualLCMSIdUpdated = (from c in _db.ModuleLcmsAssociation
                                                   where c.IsDeleted == false && c.ModuleId == moduleId
                                                   select Convert.ToInt32(c.LCMSId)).ToArray();

                var result = MultilingualLCMSIdUpdated.Except(MultilingualLCMSId);
                foreach (var res in result)
                {
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                dbContext.Database.ExecuteSqlCommand("Update Course.ModuleLcmsAssociation set IsDeleted = 1 where LCMSId = " + res + " and ModuleId = " + moduleId);
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return flag;
        }

        public async Task<APITotalModuleData> GetV2(ApiCourseModule apiCourseModule,int userId, string userRole)
        {

            APITotalModuleData aPITotalModuleData = new APITotalModuleData();
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();

            var Query = (from mod in _db.Module
                         join user in _db.UserMaster on mod.ModifiedBy equals user.Id into um
                         from user in um.DefaultIfEmpty()
                         join umd in _db.UserMasterDetails on mod.CreatedBy equals umd.UserMasterId into umddetails
                         from umd in umddetails.DefaultIfEmpty()

                         where mod.IsDeleted == false
                         select new APIModuleData
                         {
                             Name = mod.Name,
                             Id = mod.Id,
                             IsActive = mod.IsActive,
                             ModuleType = mod.ModuleType,                            
                             Description = mod.Description,
                             CreditPoints = mod.CreditPoints,
                             LCMSId = mod.LCMSId,
                             IsMultilingual = mod.IsMultilingual,
                             CourseType = mod.CourseType,
                             UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                             AreaId = umd.AreaId,
                             LocationId = umd.LocationId,
                             GroupId = umd.GroupId,
                             BusinessId = umd.BusinessId,
                             CreatedBy = mod.CreatedBy,
                             UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (mod.CreatedBy == userId) ? true : false : true
                         }).AsNoTracking();

            var authorQuery = (from mod in _db.Module
                               join cma in _db.CourseModuleAssociation on mod.Id equals cma.ModuleId
                               join course in _db.Course on cma.CourseId equals course.Id
                               join author in _db.CourseAuthorAssociation on course.Id equals author.CourseId
                         join user in _db.UserMaster on mod.ModifiedBy equals user.Id into um
                         from user in um.DefaultIfEmpty()
                         join umd in _db.UserMasterDetails on mod.CreatedBy equals umd.UserMasterId into umddetails
                         from umd in umddetails.DefaultIfEmpty()

                         where mod.IsDeleted == false && author.UserId==userId
                               select new APIModuleData
                         {
                             Name = mod.Name,
                             Id = mod.Id,
                             IsActive = mod.IsActive,
                             ModuleType = mod.ModuleType,
                             Description = mod.Description,
                             CreditPoints = mod.CreditPoints,
                             LCMSId = mod.LCMSId,
                             IsMultilingual = mod.IsMultilingual,
                             CourseType = mod.CourseType,
                             UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                             AreaId = umd.AreaId,
                             LocationId = umd.LocationId,
                             GroupId = umd.GroupId,
                             BusinessId = umd.BusinessId,
                             CreatedBy = mod.CreatedBy,
                             UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (mod.CreatedBy == userId) ? true : false : true
                         }).AsNoTracking();

            if ( !string.IsNullOrEmpty(apiCourseModule.columnName))
            {
                if ( !string.IsNullOrEmpty(apiCourseModule.columnName) && apiCourseModule.columnName.ToLower()=="coursetype")
                {
                    if (!string.IsNullOrEmpty(apiCourseModule.search))
                    {
                        string moduletype = apiCourseModule.search;
                        if (moduletype.ToLower().Equals("elearning"))
                        {
                            Query = Query.Where(r => r.CourseType.Contains("elearning"));
                        }
                        else if (moduletype.ToLower().Equals("vilt"))
                        {
                            Query = Query.Where(r => r.CourseType.Contains("vilt"));
                        }
                        else if (moduletype.ToLower().Equals("classroom"))
                        {
                            Query = Query.Where(r => r.CourseType.Contains("Classroom"));
                        }
                        else if (moduletype.ToLower().Equals("feedback"))
                        {
                            Query = Query.Where(r => r.CourseType.Equals("Feedback"));
                        }
                        else if (moduletype.ToLower().Equals("assessment") || moduletype.ToLower().Equals("Certification"))
                        {
                            Query = Query.Where(r => r.CourseType.Equals("Assessment"));
                        }
                        else if (moduletype.ToLower().Equals("memo"))
                        {
                            Query = Query.Where(r => r.CourseType.Contains("memo"));
                        }
                        else if (moduletype.ToLower().Equals("assignment"))
                        {
                            Query = Query.Where(r => r.CourseType.Contains("Assignment"));
                        }
                    }
                    else
                    {
                        Query = Query.Where(r => r.CourseType != "Feedback");
                    }
                    if(!string.IsNullOrEmpty(apiCourseModule.searchString))
                    Query = Query.Where(r => r.Name.Contains(apiCourseModule.searchString));
                }
                else
                {
                    if (apiCourseModule.columnName.ToLower().Equals("moduletype"))
                        Query = Query.Where(r => r.ModuleType.StartsWith(apiCourseModule.search));
                    if (apiCourseModule.columnName.ToLower().Equals("name"))
                        Query = Query.Where(r => r.Name.Contains(apiCourseModule.search));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(apiCourseModule.searchString))
                {
                    Query = Query.Where(r => r.Name.Contains(apiCourseModule.searchString));
                }
            }

            if ( !string.IsNullOrEmpty(apiCourseModule.columnName))
            {
                if ( !string.IsNullOrEmpty(apiCourseModule.columnName) && apiCourseModule.columnName.ToLower() == "coursetype")
                {
                    if (!string.IsNullOrEmpty(apiCourseModule.search))
                    {
                        string moduletype = apiCourseModule.search;
                        if (moduletype.ToLower().Equals("elearning"))
                        {
                            authorQuery = authorQuery.Where(r => r.CourseType.Contains("elearning"));
                        }
                        else if (moduletype.ToLower().Equals("vilt"))
                        {
                            authorQuery = authorQuery.Where(r => r.CourseType.Contains("vilt"));
                        }
                        else if (moduletype.ToLower().Equals("classroom"))
                        {
                            authorQuery = authorQuery.Where(r => r.CourseType.Contains("Classroom"));
                        }
                        else if (moduletype.ToLower().Equals("feedback"))
                        {
                            authorQuery = authorQuery.Where(r => r.CourseType.Equals("Feedback"));
                        }
                        else if (moduletype.ToLower().Equals("assessment") || moduletype.ToLower().Equals("Certification"))
                        {
                            authorQuery = authorQuery.Where(r => r.CourseType.Equals("Assessment"));
                        }
                        else if (moduletype.ToLower().Equals("memo"))
                        {
                            authorQuery = authorQuery.Where(r => r.CourseType.Contains("memo"));
                        }
                        else if (moduletype.ToLower().Equals("assignment"))
                        {
                            authorQuery = authorQuery.Where(r => r.CourseType.Contains("Assignment"));
                        }
                    }
                    else
                    {
                        authorQuery = authorQuery.Where(r => r.CourseType != "Feedback");
                    }
                    if (!string.IsNullOrEmpty(apiCourseModule.searchString))
                        authorQuery = authorQuery.Where(r => r.Name.Contains(apiCourseModule.searchString));
                }
                else
                {
                    if (apiCourseModule.columnName.ToLower().Equals("moduletype"))
                        authorQuery = authorQuery.Where(r => r.ModuleType.StartsWith(apiCourseModule.search));
                    if (apiCourseModule.columnName.ToLower().Equals("name"))
                        authorQuery = authorQuery.Where(r => r.Name.Contains(apiCourseModule.search));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(apiCourseModule.searchString))
                {
                    authorQuery = authorQuery.Where(r => r.Name.Contains(apiCourseModule.searchString));
                }
            }

            if (userRole == UserRoles.BA)
            {
                Query = Query.Where(r => r.BusinessId == userdetails.BusinessId);
            }
            if (userRole == UserRoles.GA)
            {
                Query = Query.Where(r => r.GroupId == userdetails.GroupId);
            }
            if (userRole == UserRoles.LA)
            {
                Query = Query.Where(r => r.LocationId == userdetails.LocationId);
            }
            if (userRole == UserRoles.AA)
            {
                Query = Query.Where(r => r.AreaId == userdetails.AreaId);
            }
            if (apiCourseModule.showAllData == false && (userRole != UserRoles.CA))
            {
                Query = Query.Where(r => r.CreatedBy == userId);
            }

            var queryResult = Query.Union(authorQuery);

            // Query = Query.Where(r => r.IsDeleted == false);
            aPITotalModuleData.TotalRecords = await queryResult.Distinct().CountAsync();

            queryResult = queryResult.OrderByDescending(r => r.Id);

            if (apiCourseModule.page != -1)
                queryResult = queryResult.Skip((apiCourseModule.page - 1) * apiCourseModule.pageSize);

            if (apiCourseModule.pageSize != -1)
                queryResult = queryResult.Take(apiCourseModule.pageSize);


            List<APIModuleData> module =await queryResult.Distinct().ToListAsync();



            foreach (APIModuleData item in module)
            {
                ////OPTIMIZATION COMMENT - ALL VALUES SHOULD BE FETCHED USING A SINGLE QUERY.
                int[] modulelcmsassociation = await (from c in this._db.ModuleLcmsAssociation
                                                     where c.IsDeleted == false && c.ModuleId == item.Id
                                                     select Convert.ToInt32(c.LCMSId)).ToArrayAsync();
                item.MultilingualLCMSId = modulelcmsassociation;
            }

            aPITotalModuleData.Data = module;
            return aPITotalModuleData;
        }


    }
}

