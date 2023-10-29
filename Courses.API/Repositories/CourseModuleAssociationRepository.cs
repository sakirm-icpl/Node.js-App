using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Repositories
{
    public class CourseModuleAssociationRepository : Repository<CourseModuleAssociation>, ICourseModuleAssociationRepository
    {
        ICustomerConnectionStringRepository _customerConnection;
        private CourseContext _db;
        public CourseModuleAssociationRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
        }
        public IQueryable<CourseModuleAssociation> GetAssociationCourseModule(int CourseID, int ModuleId)
        {
            return _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId);
        }
        public async Task<bool> Exist(int CourseID, int ModuleId)
        {
            if (await _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId).Select(c => c.Id).CountAsync() > 0)
                return true;
            return false;
        }
        public async Task<int?> GetAssessmentId(int CourseID, int ModuleId)
        {
            int? AssessmentId = await _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId).Select(c => c.AssessmentId).SingleOrDefaultAsync();
            return AssessmentId;
        }
        public async Task<int?> GetFeedbackId(int CourseID, int ModuleId)
        {
            int? FeedbackId = await _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId).Select(c => c.FeedbackId).SingleOrDefaultAsync();
            return FeedbackId;
        }

        public async Task<bool> IsAssementExist(int CourseID, int ModuleId)
        {
            int? AssessmentId = await _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId).Select(c => c.AssessmentId).SingleOrDefaultAsync();
            if (AssessmentId == null || AssessmentId == 0)
                return false;
            return true;
        }
        public async Task<bool> IsFeedbackExist(int CourseID, int ModuleId)
        {
            int? FeedbackId = await _db.CourseModuleAssociation.Where(r => r.CourseId == CourseID && r.ModuleId == ModuleId).Select(c => c.FeedbackId).FirstOrDefaultAsync();
            if (FeedbackId == null || FeedbackId == 0)
                return false;
            return true;
        }
        public async Task<List<TypeAhead>> GetModelTypeAhead(int courseId, string search = null)
        {
            IQueryable<TypeAhead> Query = (from module in _db.Module
                                           join courseModule in _db.CourseModuleAssociation on module.Id equals courseModule.ModuleId
                                           into temp
                                           from courseModule in temp.DefaultIfEmpty()
                                           where module.IsDeleted == false && module.IsActive == true && courseModule.CourseId == courseId
                                           select new TypeAhead
                                           { Id = module.Id, Title = module.Name });
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(m => m.Title.StartsWith(search.ToLower()));
            }
            Query = Query.OrderByDescending(r => r.Id);
            return await Query.ToListAsync();
        }

        public async Task<int> AdjustSequence(List<ApiCourseModuleSequence> apiCourseModuleList, int courseId)
        {
            List<CourseModuleAssociation> ModuleList = await this._db.CourseModuleAssociation.Where(c => c.CourseId == courseId).ToListAsync();
            foreach (var module in ModuleList)
            {
                module.SequenceNo = apiCourseModuleList.Where(m => m.ModuleId == module.ModuleId).Select(s => s.SequenceNo).FirstOrDefault();
            }
            this._db.CourseModuleAssociation.UpdateRange(ModuleList);
            await this._db.SaveChangesAsync();
            return 1;
        }

        public async Task CourseModuleAuditlog(List<CourseModuleAssociation> oldModule,string action)
        {
            try
            {
                
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        foreach (CourseModuleAssociation obj in oldModule)
                        {
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "InsertCourseModuleAssociationAuditlog";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = obj.Id });
                                cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = obj.CourseId });
                                cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = obj.ModuleId });
                                cmd.Parameters.Add(new SqlParameter("@AssessmentId", SqlDbType.Int) { Value = obj.AssessmentId });
                                cmd.Parameters.Add(new SqlParameter("@FeedbackId", SqlDbType.Int) { Value = obj.FeedbackId });
                                cmd.Parameters.Add(new SqlParameter("@IsAssessment", SqlDbType.Bit) { Value = obj.IsAssessment });
                                cmd.Parameters.Add(new SqlParameter("@IsFeedback", SqlDbType.Bit) { Value = obj.IsFeedback });
                                cmd.Parameters.Add(new SqlParameter("@IsPreAssessment", SqlDbType.Bit) { Value = obj.IsPreAssessment });
                                cmd.Parameters.Add(new SqlParameter("@PreAssessmentId", SqlDbType.Int) { Value = obj.PreAssessmentId });
                                cmd.Parameters.Add(new SqlParameter("@SectionId", SqlDbType.Int) { Value = obj.SectionId });
                                cmd.Parameters.Add(new SqlParameter("@SequenceNo", SqlDbType.Int) { Value = obj.SequenceNo });
                                cmd.Parameters.Add(new SqlParameter("@Isdeleted", SqlDbType.Bit) { Value = obj.Isdeleted });
                                cmd.Parameters.Add(new SqlParameter("@CompletionPeriodDays", SqlDbType.Int) { Value = obj.CompletionPeriodDays });
                                cmd.Parameters.Add(new SqlParameter("@Action", SqlDbType.NVarChar) { Value = action });
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                reader.Dispose();
                            }
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

        public async Task CourseModuleAssociationAuditlog(IList<CourseModuleAssociation> modules,string action)
        {
            try
            {

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        foreach (CourseModuleAssociation obj in modules)
                        {
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "InsertCourseModuleAssociationAuditlog";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = obj.Id });
                                cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = obj.CourseId });
                                cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = obj.ModuleId });
                                cmd.Parameters.Add(new SqlParameter("@AssessmentId", SqlDbType.Int) { Value = obj.AssessmentId });
                                cmd.Parameters.Add(new SqlParameter("@FeedbackId", SqlDbType.Int) { Value = obj.FeedbackId });
                                cmd.Parameters.Add(new SqlParameter("@IsAssessment", SqlDbType.Bit) { Value = obj.IsAssessment });
                                cmd.Parameters.Add(new SqlParameter("@IsFeedback", SqlDbType.Bit) { Value = obj.IsFeedback });
                                cmd.Parameters.Add(new SqlParameter("@IsPreAssessment", SqlDbType.Bit) { Value = obj.IsPreAssessment });
                                cmd.Parameters.Add(new SqlParameter("@PreAssessmentId", SqlDbType.Int) { Value = obj.PreAssessmentId });
                                cmd.Parameters.Add(new SqlParameter("@SectionId", SqlDbType.Int) { Value = obj.SectionId });
                                cmd.Parameters.Add(new SqlParameter("@SequenceNo", SqlDbType.Int) { Value = obj.SequenceNo });
                                cmd.Parameters.Add(new SqlParameter("@Isdeleted", SqlDbType.Bit) { Value = obj.Isdeleted });
                                cmd.Parameters.Add(new SqlParameter("@CompletionPeriodDays", SqlDbType.Int) { Value = obj.CompletionPeriodDays });
                                cmd.Parameters.Add(new SqlParameter("@Action", SqlDbType.NVarChar) { Value = action });
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                reader.Dispose();
                            }
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
    }
}
