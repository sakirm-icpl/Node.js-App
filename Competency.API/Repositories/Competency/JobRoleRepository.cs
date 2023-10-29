// ======================================
// <copyright file="CompetenciesMasterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Competency.API.APIModel;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Services;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Competency.API.Common;
using log4net;
namespace Competency.API.Repositories.Interfaces.Competency
{
    public class JobRoleRepository : Repository<CompetencyJobRole>, IJobRoleRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobRoleRepository));
        CourseContext db;
        INotification _notification;
        private readonly IConfiguration _configuration;
        ICustomerConnectionStringRepository _customerConnection;
        IJdUploadRepository _jdUploadRepository;

        IIdentityService _identitySv;
        public JobRoleRepository(CourseContext context, IJdUploadRepository jdUploadRepository, IConfiguration configuration, INotification notification, IIdentityService identitySv, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            this.db = context;
            this._configuration = configuration;
            this._customerConnection = customerConnection;
            this._notification = notification;
            this._identitySv = identitySv;
            this._jdUploadRepository = jdUploadRepository;
        }


        public async Task<bool> ExistsRecord(int? id, string Code, string Name)
        {

            Code = Code.ToLower().Trim();
            Name = Name.ToLower().Trim();

            int Count = 0;

            if (id != null)
            {
                Count = await (from c in this.db.CompetencyJobRole
                               where c.Id != id && c.IsDeleted == false && (c.Code.ToLower().Equals(Code) && c.Name == Name)
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this.db.CompetencyJobRole
                               where c.IsDeleted == false && (c.Code.ToLower().Equals(Code) && c.Name == Name)
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;


        }


        public async Task<int> GetIdByValue(string Value)
        {
            var result = await (from c in this.db.CompetenciesMaster
                                where c.IsDeleted == false && c.CompetencyName == Value
                                select new
                                {
                                    c.Id,
                                    c.CompetencyName,

                                }).ToListAsync();
            int Id = result.Select(o => o.Id).FirstOrDefault();
            return Id;

        }

        public async Task<string> GetValueById(int Value)
        {
            var result = await (from c in this.db.CompetenciesMaster
                                where c.IsDeleted == false && c.Id == Value
                                select new
                                {
                                    c.Id,
                                    c.CompetencyName,

                                }).ToListAsync();
            string Id = result.Select(o => o.CompetencyName).FirstOrDefault();
            return Id;

        }
        public async Task<IQueryable<APIViewCompetencyJobRole>> GetCompetencyJobRole(string orgcode, int page, int pageSize, string search = null)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competencyJobRole in context.CompetencyJobRole
                                  orderby competencyJobRole.Id descending
                                  where competencyJobRole.IsDeleted == Record.NotDeleted
                                  select new APIViewCompetencyJobRole
                                  {
                                      Id = competencyJobRole.Id,
                                      Name = competencyJobRole.Name,
                                      Code = competencyJobRole.Code,
                                      Description = competencyJobRole.Description,
                                      IsDeleted = competencyJobRole.IsDeleted

                                  });


                    List<APIViewCompetencyJobRole> apiCompetencyJobRoleList = new List<APIViewCompetencyJobRole>();
                    foreach (APIViewCompetencyJobRole item in result.ToList())
                    {
                        APIViewCompetencyJobRole apiCompetencyJobRole = new APIViewCompetencyJobRole();


                        apiCompetencyJobRole.Code = item.Code;
                        apiCompetencyJobRole.Name = item.Name;
                        apiCompetencyJobRole.Description = item.Description;
                        apiCompetencyJobRole.Id = item.Id;


                        apiCompetencyJobRoleList.Add(apiCompetencyJobRole);
                    }
                    result = apiCompetencyJobRoleList.AsQueryable();


                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where((a => ((Convert.ToString(a.Name).StartsWith(search) || Convert.ToString(a.Description).StartsWith(search) || Convert.ToString(a.Code).StartsWith(search)) && (a.IsDeleted == Record.NotDeleted))));
                    }
                    if (page != -1)
                    {
                        result = result.Skip((page - 1) * pageSize);

                    }

                    if (pageSize != -1)
                    {
                        result = result.Take(pageSize);

                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async Task<APICompetencyJobRole> GetCompetencyJobRoleById(string orgcode, int JobRoleId)
        {
            try
            {
                using (var context = this.db)
                {
                    CompetencyJdUpload jdUpload = await _jdUploadRepository.GetCompetencyJdUpload(JobRoleId);
                    APICompetencyJobRole item = new APICompetencyJobRole();
                    if (jdUpload == null)
                    {
                        IQueryable<APICompetencyJobRole> result = (from competencyJobRole in context.CompetencyJobRole
                                       orderby competencyJobRole.Id descending
                                       where competencyJobRole.IsDeleted == Record.NotDeleted && competencyJobRole.Id == JobRoleId
                                       select new APICompetencyJobRole
                                       {
                                           Id = competencyJobRole.Id,
                                           Name = competencyJobRole.Name,
                                           Code = competencyJobRole.Code,
                                           Description = competencyJobRole.Description,
                                           RoleColumn1 = competencyJobRole.RoleColumn1,
                                           RoleColumn1value = Convert.ToInt32(competencyJobRole.RoleColumn1value),
                                           RoleColumn2 = competencyJobRole.RoleColumn2,
                                           RoleColumn2value = Convert.ToInt32(competencyJobRole.RoleColumn2value),
                                           Column1value = null,
                                           Column2value = null,
                                           NumberOfPositions = competencyJobRole.NumberOfPositions,
                                           CourseId = competencyJobRole.CourseId
                                       });
                        item = result.FirstOrDefault();
                    }
                    else
                    {
                        IQueryable<APICompetencyJobRole> result = (from competencyJobRole in context.CompetencyJobRole
                                      orderby competencyJobRole.Id descending
                                      where competencyJobRole.IsDeleted == Record.NotDeleted && competencyJobRole.Id == JobRoleId
                                      select new APICompetencyJobRole
                                      {
                                          Id = competencyJobRole.Id,
                                          Name = competencyJobRole.Name,
                                          Code = competencyJobRole.Code,
                                          Description = competencyJobRole.Description,
                                          RoleColumn1 = competencyJobRole.RoleColumn1,
                                          RoleColumn1value = Convert.ToInt32(competencyJobRole.RoleColumn1value),
                                          RoleColumn2 = competencyJobRole.RoleColumn2,
                                          RoleColumn2value = Convert.ToInt32(competencyJobRole.RoleColumn2value),
                                          Column1value = null,
                                          Column2value = null,
                                          NumberOfPositions = competencyJobRole.NumberOfPositions,
                                          CourseId = competencyJobRole.CourseId,
                                          FilePath = jdUpload.FilePath,
                                          FileType = jdUpload.FileType
                                      });
                        item = result.FirstOrDefault();
                    }

                    APICompetencyJobRole apiCompetencyJobRole = new APICompetencyJobRole();

                    string UserUrl = _configuration[APIHelper.UserAPI];
                    string NameById = "GetNameById";
                    string ColumnName = item.RoleColumn1;
                    int Value = item.RoleColumn1value;
                    string Apiurl = UserUrl + NameById + "/" + orgcode + "/" + ColumnName + "/" + Value;
                    HttpResponseMessage response = await APIHelper.CallGetAPI(Apiurl);
                    if (response.IsSuccessStatusCode)
                    {
                        var result1 = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result1);
                        item.Column1value = _Title == null ? null : _Title.Name;
                    }
                    if (!string.IsNullOrEmpty(item.RoleColumn2))
                    {
                        string UserUrl1 = _configuration[APIHelper.UserAPI];

                        string Apiurl1 = UserUrl1 + NameById + "/" + orgcode + "/" + item.RoleColumn2 + "/" + item.RoleColumn2value;
                        response = await APIHelper.CallGetAPI(Apiurl1);
                        if (response.IsSuccessStatusCode)
                        {
                            var result1 = await response.Content.ReadAsStringAsync();
                            Title _Title = JsonConvert.DeserializeObject<Title>(result1);
                            item.Column2value = _Title == null ? null : _Title.Name;
                        }
                    }

                    apiCompetencyJobRole.Code = item.Code;
                    apiCompetencyJobRole.Name = item.Name;
                    apiCompetencyJobRole.Column1value = item.Column1value;
                    apiCompetencyJobRole.Column2value = item.Column2value;
                    apiCompetencyJobRole.RoleColumn1 = item.RoleColumn1;
                    apiCompetencyJobRole.RoleColumn2 = item.RoleColumn2;
                    apiCompetencyJobRole.RoleColumn1value = item.RoleColumn1value;
                    apiCompetencyJobRole.RoleColumn2value = item.RoleColumn2value;
                    apiCompetencyJobRole.Description = item.Description;
                    apiCompetencyJobRole.Id = item.Id;
                    apiCompetencyJobRole.IsDeleted = item.IsDeleted;
                    apiCompetencyJobRole.NumberOfPositions = item.NumberOfPositions;
                    apiCompetencyJobRole.CourseId = item.CourseId;
                    apiCompetencyJobRole.FilePath = item.FilePath;
                    apiCompetencyJobRole.FileType = item.FileType;



                    List<APIJobRole> resultNextJobRolesval = (from NJobRoles in this.db.NextJobRoles
                                                              join c in this.db.CompetencyJobRole on NJobRoles.NextJobRoleId equals c.Id
                                                              where c.IsDeleted == false && NJobRoles.IsDeleted == false && NJobRoles.JobRoleId == item.Id
                                                              select new APIJobRole
                                                              {
                                                                  Id = c.Id,
                                                                  Name = c.Name

                                                              }).ToList();

                    List<APIJobRole> resultCompetencySkill = (from rolecompetency in this.db.RoleCompetency
                                                              join c in this.db.CompetenciesMaster on rolecompetency.CompetencyId equals c.Id
                                                              where c.IsDeleted == false && rolecompetency.IsDeleted == false && rolecompetency.JobRoleId == item.Id
                                                              select new APIJobRole
                                                              {
                                                                  Id = c.Id,
                                                                  Name = c.CompetencyName

                                                              }).ToList();

                    var coursename = (from course in db.Course where course.Id == apiCompetencyJobRole.CourseId select course.Title).FirstOrDefault();
                    apiCompetencyJobRole.CourseName = coursename;

                    apiCompetencyJobRole.NextJobRolesData = resultNextJobRolesval.ToArray();
                    apiCompetencyJobRole.CompetencySkillsData = resultCompetencySkill.ToArray();

                    return apiCompetencyJobRole;
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
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.CompetencyJobRole.Where(r => ((r.Name.StartsWith(search) || r.Code.StartsWith(search)) && (r.IsDeleted == Record.NotDeleted))).CountAsync();
            return await this.db.CompetencyJobRole.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }


        public async Task<bool> IsDependacyExist(int jobroleid)

        {
            APIJobRoleInUse apiJobRoleInUse = new APIJobRoleInUse();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();

                        parameters.Add("@JobRoleId", jobroleid);

                        IEnumerable<APIJobRoleInUse> Result = await SqlMapper.QueryAsync<APIJobRoleInUse>((SqlConnection)connection, "dbo.CheckIsJobRoleInUse", parameters, null, null, CommandType.StoredProcedure);
                        apiJobRoleInUse = Result.FirstOrDefault();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return apiJobRoleInUse.JobRoleInUse;
        }
        public async Task<IEnumerable<APIJobRole>> GetAllJobRoles()
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from competencyJobRole in context.CompetencyJobRole
                                  where competencyJobRole.IsDeleted == Record.NotDeleted
                                  select new APIJobRole
                                  {
                                      Id = competencyJobRole.Id,
                                      Name = competencyJobRole.Name


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
        public async Task<bool> Exists(int JobRoleId, int CompetencyId)
        {

            var count = await this.db.RoleCompetency.Where((p => (p.JobRoleId == JobRoleId) && (p.CompetencyId == CompetencyId) && (p.IsDeleted == Record.NotDeleted))).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> ExistsForJobRole(string name)
        {

            var count = await this.db.CompetencyJobRole.Where((p => (p.Name == name) && (p.IsDeleted == Record.NotDeleted))).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
        public async Task<List<CompetenciesMaster>> GetTypeAheadForJobRole(string search = null)
        {

            IQueryable<CompetenciesMaster> Quizzes = (from c in this.db.CompetenciesMaster
                                                      where (search == null || c.CompetencyName.StartsWith(search)) && c.IsDeleted == false

                                                      select new CompetenciesMaster
                                                      {
                                                          Id = c.Id,
                                                          CompetencyName = c.CompetencyName,
                                                          CompetencyDescription = c.CompetencyDescription,
                                                          CategoryId = c.CategoryId,
                                                          CreatedBy = c.CreatedBy,
                                                          CreatedDate = c.CreatedDate,
                                                          IsActive = c.IsActive,
                                                          IsDeleted = c.IsDeleted

                                                      });

            return await Quizzes.ToListAsync();

        }

        public async Task<bool> UpdateNextJobrole(int[] _NextJobRoles, int jobroleId, int UserId)
        {
            bool flag = false;
            foreach (var apinextJobRoles in _NextJobRoles)
            {

                var exists = (from x in db.NextJobRoles.Where(x => x.JobRoleId == jobroleId && x.NextJobRoleId == apinextJobRoles)
                              select x).ToList();
                if (exists.Count > 0)
                {
                    var existsdeleted = (from x in db.NextJobRoles.Where(x => x.JobRoleId == jobroleId && x.NextJobRoleId == apinextJobRoles && x.IsDeleted == true)
                                         select x).ToList();
                    if (existsdeleted.Count > 0)
                    {
                        try
                        {
                            NextJobRoles nextJobRoles = await this.db.NextJobRoles.Where(a => a.JobRoleId == jobroleId && a.NextJobRoleId == apinextJobRoles).FirstOrDefaultAsync();

                            nextJobRoles.IsDeleted = false;
                            nextJobRoles.ModifiedBy = UserId;
                            nextJobRoles.ModifiedDate = DateTime.UtcNow;
                            this.db.NextJobRoles.Update(nextJobRoles);
                            await this.db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    NextJobRoles nextJobRoles = new NextJobRoles();
                    nextJobRoles.JobRoleId = Convert.ToInt32(jobroleId);
                    nextJobRoles.NextJobRoleId = apinextJobRoles;
                    nextJobRoles.UserId = UserId;
                    nextJobRoles.CreatedBy = UserId;
                    nextJobRoles.CreatedDate = DateTime.UtcNow;
                    nextJobRoles.IsActive = true;
                    nextJobRoles.IsDeleted = false;
                    nextJobRoles.ModifiedBy = UserId;
                    nextJobRoles.ModifiedDate = DateTime.UtcNow;
                    await this.db.NextJobRoles.AddAsync(nextJobRoles);
                    await this.db.SaveChangesAsync();
                }

            }

            int[] existingJobRoles = (from c in this.db.NextJobRoles where c.IsDeleted == false && c.JobRoleId == jobroleId select c.NextJobRoleId).ToArray();

            var result = existingJobRoles.Except(_NextJobRoles);
            foreach (var res in result)
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        connection.Close();
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            dbContext.Database.ExecuteSqlCommand("Update course.NextJobRoles set IsDeleted = 1 where NextJobRoleId = " + res + " and JobRoleId = " + jobroleId);
                        }
                    }
                }

            }
            return flag;
        }


        public async Task<NextJobRoles> PostNextJobRoleDetails(NextJobRoles nextJobRoles)
        {
            var exists = (from x in db.NextJobRoles.Where(x => x.JobRoleId == nextJobRoles.JobRoleId && x.NextJobRoleId == nextJobRoles.NextJobRoleId && x.IsDeleted == false)
                          select x).ToList();
            if (exists.Count > 0)
            {
                return null;
            }
            else
            {
                await this.db.NextJobRoles.AddAsync(nextJobRoles);
                await this.db.SaveChangesAsync();
                return nextJobRoles;
            }
        }
        public async Task<List<APIRoles>> GetTypeAhead()
        {
            IQueryable<APIRoles> jobrole = (from c in this.db.CompetencyJobRole
                                            where c.IsDeleted == false
                                            select new APIRoles
                                            {
                                                Id = c.Id,
                                                Name = c.Name
                                            });

            return await jobrole.ToListAsync();
        }

        public async Task<IEnumerable<APIMasterTestCourse>> GetMasterTestCourseDetails(int userId, int? jobRoleId = null)
        {
            IEnumerable<APIMasterTestCourse> MasterTestCourse = new List<APIMasterTestCourse>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserId", userId);
                        parameters.Add("@JobRoleId", jobRoleId);

                        IEnumerable<APIMasterTestCourse> Result = await SqlMapper.QueryAsync<APIMasterTestCourse>((SqlConnection)connection, "dbo.GetMasterCourseDetails", parameters, null, null, CommandType.StoredProcedure);
                        MasterTestCourse = Result.ToList();
                        connection.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return MasterTestCourse;
        }


        public async Task<bool> DeleteUserCarrerJobRole(int id, int userid)
        {
            var Query = db.CareerJobRoles.Where(r => r.IsDeleted == Record.NotDeleted && r.Id == id);
            CareerJobRoles careerJobRoles = await Query.FirstOrDefaultAsync();

            if (careerJobRoles != null)
            {
                careerJobRoles.IsDeleted = true;
                careerJobRoles.ModifiedBy = userid;
                careerJobRoles.ModifiedDate = DateTime.UtcNow;
                db.Update(careerJobRoles);
                db.SaveChanges();

                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> SendNotificationForManagerApproval()
        {
            Task.Run(() => SendNotificationForManager());
            return true;
        }

        public async Task<int> SendNotificationForManager()
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        string token = _identitySv.GetToken();

                        using (var cmd = connection.CreateCommand())
                        {

                            cmd.CommandText = "GetNotificationToManagerForMasterTest";
                            cmd.CommandType = CommandType.StoredProcedure;

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            int count = 0;
                            List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                            foreach (DataRow row in dt.Rows)
                            {
                                string Message = "'{UserName}' has achieved competency for job role '{Jobrole}', Please assign master course.";
                                Message = Message.Replace("{UserName}", row["UserName"].ToString());
                                Message = Message.Replace("{Jobrole}", row["JobRoleName"].ToString());

                                ApiNotification NotificationData = new ApiNotification();
                                NotificationData.Title = "Competency Achieved By User";
                                NotificationData.Message = Message;
                                NotificationData.Type = Record.AssignMasterCourse;
                                NotificationData.UserId = Convert.ToInt32(row["reporttingManagerid"]);
                                lstApiNotification.Add(NotificationData);
                                count++;
                                if (count % Constants.BATCH_SIZE == 0)
                                {
                                    await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                    lstApiNotification.Clear();
                                }
     

                            }

                            if (lstApiNotification.Count > 0)
                            {
                                await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                lstApiNotification.Clear();
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
            }
            return 1;
        }

        public async Task<int> ScheduleRequestNotificationTo_CommonBulk(List<ApiNotification> Notification, string token)
        {
            await this._notification.ScheduleRequestNotificationTo_Common(Notification, token);
            return 1;

        }
    }

}

