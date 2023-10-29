﻿using MyCourse.API.APIModel;
using MyCourse.API.Helper;
using MyCourse.API.Model;
//using MyCourse.API.Model.ILT;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories
{
    public class DevelopmentPlanRepository : IDevelopmentPlanRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DevelopmentPlanRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        ICourseRepository _courseRepository;
        IConfiguration _configuration;
        private IAccessibilityRule _accessibilityRule;
        public DevelopmentPlanRepository(CourseContext db, IConfiguration configuration, ICustomerConnectionStringRepository customerConnection, ICourseRepository courseRepository, IAccessibilityRule accessibilityRule)
        {
            _db = db;
            _customerConnection = customerConnection;
            _courseRepository = courseRepository;
            _configuration = configuration;
            _accessibilityRule = accessibilityRule;
        }

        public async Task<int> SaveDevelopmentPlan(DevelopmentPlanForCourse development, DevelopmentPlanCourses[] developmentPlanCourses, int UserId, bool isIdp = false)
        {
            if (development == null)
            {
                return -1;
            }
            if (development.DevelopmentCode == null || string.IsNullOrEmpty(development.DevelopmentCode))
            {
                return -2;
            }
            if (development.DevelopmentName == null || string.IsNullOrEmpty(development.DevelopmentName))
            {
                return -3;
            }
            DevelopmentPlanForCourse development1 = _db.DevelopmentPlanForCourse.Where(a => a.DevelopmentCode == development.DevelopmentCode && a.IsDeleted == false).FirstOrDefault();
            if (development1 != null)
            {
                return -4;
            }
            development1 = _db.DevelopmentPlanForCourse.Where(a => a.DevelopmentName == development.DevelopmentName && a.IsDeleted == false).FirstOrDefault();
            if (development1 != null)
            {
                return -5;
            }

            if (developmentPlanCourses.Length == development.CountOfMappedCourses)
            {
                await _db.DevelopmentPlanForCourse.AddAsync(development);
                await _db.SaveChangesAsync();

                DevelopmentPlanForCourse developmentPlanForCourse = _db.DevelopmentPlanForCourse.Where(a => a.DevelopmentCode == development.DevelopmentCode && a.IsDeleted == false).FirstOrDefault();

                foreach (DevelopmentPlanCourses developmentPlanCourse in developmentPlanCourses)
                {
                    CourseMappingToDevelopment developmentPlanCourse1 = new CourseMappingToDevelopment();

                    developmentPlanCourse1.CourseId = developmentPlanCourse.CourseId;
                    developmentPlanCourse1.CreatedBy = UserId;
                    developmentPlanCourse1.CreatedDate = DateTime.Now;
                    developmentPlanCourse1.ModifiedDate = DateTime.Now;
                    developmentPlanCourse1.ModifiedBy = UserId;
                    developmentPlanCourse1.IsDeleted = false;
                    developmentPlanCourse1.sequenceNo = developmentPlanCourse.sequenceNo;
                    developmentPlanCourse1.DevelopmentPlanId = development.Id;

                    await _db.CourseMappingToDevelopment.AddAsync(developmentPlanCourse1);
                    await _db.SaveChangesAsync();
                }
                if (isIdp)
                {
                    UserDevelopmentPlanMapping userDevelopmentPlanMapping = new UserDevelopmentPlanMapping();

                    userDevelopmentPlanMapping.UserID = Convert.ToInt32(UserId);
                    userDevelopmentPlanMapping.ConditionForRules = "AND";
                    userDevelopmentPlanMapping.DevelopmentPlanid = development.Id;
                    userDevelopmentPlanMapping.CreatedBy = UserId;
                    userDevelopmentPlanMapping.ModifiedBy = UserId;
                    userDevelopmentPlanMapping.CreatedDate = DateTime.Now;
                    userDevelopmentPlanMapping.ModifiedDate = DateTime.Now;
                    this._db.UserDevelopmentPlanMapping.Add(userDevelopmentPlanMapping);
                    this._db.SaveChanges();
                }
            }
            return 0;
        }
        public async Task<IEnumerable<APIDevelopmentPlanForCourse>> GetAllDevelopmentPlan(int page, int pageSize, string search = null, string columnName = null, int? userId = null,bool isIdp =false)
        {
            List<APIDevelopmentPlanForCourse> developmentPlanForCourses = new List<APIDevelopmentPlanForCourse>();
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetAllDevelopmentPlan";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
                        cmd.Parameters.Add(new SqlParameter("@isIdp", SqlDbType.Bit) { Value = isIdp });

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {

                                var developmentPlanForCourse = new APIDevelopmentPlanForCourse
                                {
                                    Id = Convert.ToInt32(row["Id"].ToString()),
                                    CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
                                    DevelopmentCode = row["DevelopmentCode"].ToString(),
                                    DevelopmentName = row["DevelopmentName"].ToString(),
                                    AboutPlan = row["AboutPlan"].ToString(),
                                    EnforceLinearApproach = Convert.ToBoolean(row["EnforceLinearApproach"]),
                                    AllowLearningAfterExpiry = Convert.ToBoolean(row["AllowLearningAfterExpiry"]),
                                    UploadThumbnail = row["UploadThumbnail"].ToString(),
                                    CountOfMappedCourses = Convert.ToInt32(row["CountOfMappedCourses"]),
                                    TargetCompletion = Convert.ToInt32(row["TargetCompletion"]),
                                    TotalCreditPoints = Convert.ToInt32(row["TotalCreditPoints"]),
                                    Status = Convert.ToBoolean(row["Status"]),
                                    NumberofMembers = Convert.ToInt32(row["NumberofMembers"]),
                                    NumberOfRules = Convert.ToInt32(row["NumberOfRules"]),
                                    LastModifiedBy = row["LastModifiedBy"].ToString(),
                                    StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString()),
                                    EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["EndDate"].ToString()),
                                    Metadata = row["Metadata"].ToString(),
                                    ModifiedDate = Convert.ToDateTime(row["ModifiedDate"].ToString()),
                                    progressStatus = row["progressStatus"].ToString(),
                                    FeedbackId = string.IsNullOrEmpty(row["FeedbackId"].ToString()) ? (int?)null: Convert.ToInt32(row["FeedbackId"]),
                                    FeedbackName= row["FeedbackName"].ToString()
                                };
                                developmentPlanForCourses.Add(developmentPlanForCourse);
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                    return developmentPlanForCourses;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> GetAllDevelopmentPlanCount(int page, int pageSize, string search = null, string columnName = null, int? userId = null, bool isIdp = false)
        {
            int Count = 0;
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = "GetAllDevelopmentPlanCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });

                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });                       
                        cmd.Parameters.Add(new SqlParameter("@isIdp", SqlDbType.Bit) { Value = isIdp });


                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                Count = string.IsNullOrEmpty(row["count"].ToString()) ? 0 : Convert.ToInt32(row["count"].ToString());
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                    return Count;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;

        }
        public int DeleteDevelopmentPlan(string DevelopmentCode)
        {
            try
            {
                if (DevelopmentCode == null)
                {
                    return -1;
                }
                else
                {
                    DevelopmentPlanForCourse Result = GetDevelopmentPlanByTeamsCode(DevelopmentCode);

                    if (Result == null)
                    {
                        return -2;
                    }
                    else
                    {
                        Result.IsDeleted = true;
                        _db.DevelopmentPlanForCourse.Update(Result);
                        _db.SaveChanges();

                        List<CourseMappingToDevelopment> courseMappingToDevelopment = _db.CourseMappingToDevelopment.Where(a => a.DevelopmentPlanId == Result.Id && a.IsDeleted == false).ToList();

                        foreach (CourseMappingToDevelopment courseMappingToDevelopment1 in courseMappingToDevelopment)
                        {
                            courseMappingToDevelopment1.IsDeleted = true;

                            _db.CourseMappingToDevelopment.Update(courseMappingToDevelopment1);
                            _db.SaveChanges();
                        }
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public DevelopmentPlanForCourse GetDevelopmentPlanByTeamsCode(string DevelopmentCode)
        {
            if (DevelopmentCode == null)
            {
                return null;
            }
            else
            {
                DevelopmentPlanForCourse developmentPlanForCourse = _db.DevelopmentPlanForCourse.Where(a => a.DevelopmentCode == DevelopmentCode && a.IsDeleted == false).FirstOrDefault();
                return developmentPlanForCourse;
            }
        }
        public async Task<int> UpdateDevelopmentPlan(Development development, int UserId)
        {
            if (development == null)
            {
                return -1;
            }
            if (development.DevelopmentCode == null || string.IsNullOrEmpty(development.DevelopmentCode))
            {
                return -2;
            }
            if (development.DevelopmentName == null || string.IsNullOrEmpty(development.DevelopmentName))
            {
                return -3;
            }
            DevelopmentPlanForCourse developmentPlanForCourse = await _db.DevelopmentPlanForCourse.Where(a => a.DevelopmentName == development.DevelopmentName && a.DevelopmentCode != development.DevelopmentCode && a.IsDeleted == false).FirstOrDefaultAsync();

            if (developmentPlanForCourse != null)
            {
                return -4;
            }
            DevelopmentPlanForCourse olddevelopmentPlanForCourse = await _db.DevelopmentPlanForCourse.Where(a => a.DevelopmentCode == development.DevelopmentCode && a.IsDeleted == false).FirstOrDefaultAsync();

            olddevelopmentPlanForCourse.DevelopmentName = development.DevelopmentName;
            olddevelopmentPlanForCourse.Status = development.Status;
            olddevelopmentPlanForCourse.AboutPlan = development.AboutPlan;
            olddevelopmentPlanForCourse.AllowLearningAfterExpiry = development.AllowLearningAfterExpiry;
            olddevelopmentPlanForCourse.ModifiedBy = UserId;
            olddevelopmentPlanForCourse.ModifiedDate = DateTime.Now;
            olddevelopmentPlanForCourse.EnforceLinearApproach = development.EnforceLinearApproach;
            olddevelopmentPlanForCourse.TargetCompletion = development.TargetCompletion;
            olddevelopmentPlanForCourse.TotalCreditPoints = development.TotalCreditPoints;
            olddevelopmentPlanForCourse.NumberofMembers = development.NumberofMembers;
            olddevelopmentPlanForCourse.NumberOfRules = development.NumberOfRules;
            olddevelopmentPlanForCourse.UploadThumbnail = development.UploadThumbnail;
            olddevelopmentPlanForCourse.StartDate = development.StartDate;
            olddevelopmentPlanForCourse.EndDate = development.EndDate;
            olddevelopmentPlanForCourse.Metadata = development.Metadata;
            olddevelopmentPlanForCourse.FeedbackId = development.FeedbackId;
            var Result = _db.CourseMappingToDevelopment.Where(a => a.DevelopmentPlanId == olddevelopmentPlanForCourse.Id && a.IsDeleted == false);
            List<int> Courseid = new List<int>();
            foreach (var result in Result)
            {
                int count = 0;
                foreach (DevelopmentPlanCourses developmentPlanCourse in development.developmentPlanCourses)
                {
                    if (result.CourseId == developmentPlanCourse.CourseId)
                    {
                        count = 1;

                        break;
                    }
                }
                if (count == 0)
                {
                    Courseid.Add(result.CourseId);
                }
            }
            foreach (int id in Courseid)
            {

                CourseMappingToDevelopment courseMappingToDevelopment = Result.Where(a => a.CourseId == id).FirstOrDefault();
                courseMappingToDevelopment.IsDeleted = true;
                courseMappingToDevelopment.sequenceNo = 0;

                _db.CourseMappingToDevelopment.Update(courseMappingToDevelopment);
                _db.SaveChanges();

                olddevelopmentPlanForCourse.CountOfMappedCourses--;
            }
            foreach (DevelopmentPlanCourses developmentPlanCourse in development.developmentPlanCourses)
            {
                CourseMappingToDevelopment developmentPlanCourse1 = Result.Where(a => a.CourseId == developmentPlanCourse.CourseId).FirstOrDefault();

                if (developmentPlanCourse1 == null)
                {
                    developmentPlanCourse1 = _db.CourseMappingToDevelopment.Where(a => a.DevelopmentPlanId == olddevelopmentPlanForCourse.Id &&
                       a.CourseId == developmentPlanCourse.CourseId && a.IsDeleted == false
                    ).FirstOrDefault();

                    if (developmentPlanCourse1 == null)
                    {
                        developmentPlanCourse1 = new CourseMappingToDevelopment();
                        developmentPlanCourse1.CourseId = developmentPlanCourse.CourseId;
                        developmentPlanCourse1.CreatedBy = UserId;
                        developmentPlanCourse1.CreatedDate = DateTime.Now;
                        developmentPlanCourse1.ModifiedDate = DateTime.Now;
                        developmentPlanCourse1.ModifiedBy = UserId;
                        developmentPlanCourse1.IsDeleted = false;
                        developmentPlanCourse1.sequenceNo = developmentPlanCourse.sequenceNo;
                        developmentPlanCourse1.DevelopmentPlanId = olddevelopmentPlanForCourse.Id;

                        olddevelopmentPlanForCourse.CountOfMappedCourses++;

                        await _db.CourseMappingToDevelopment.AddAsync(developmentPlanCourse1);
                    }
                    else
                    {
                        developmentPlanCourse1.IsDeleted = true;
                        developmentPlanCourse1.sequenceNo = developmentPlanCourse.sequenceNo;
                        _db.CourseMappingToDevelopment.Update(developmentPlanCourse1);
                    }
                    await _db.SaveChangesAsync();
                }
                else
                    {
                       
                        developmentPlanCourse1.sequenceNo = developmentPlanCourse.sequenceNo;
                        _db.CourseMappingToDevelopment.Update(developmentPlanCourse1);
                    await _db.SaveChangesAsync();
                }
                    
            }

            this._db.DevelopmentPlanForCourse.Update(olddevelopmentPlanForCourse);
            await this._db.SaveChangesAsync();
            return 0;
        }
        public async Task<List<DevelopmentCoursesDetails>> getCourseDetailsByDevelopmentId(int developmentId)
        {
            if (developmentId == 0)
            {
                return null;
            }
            DevelopmentPlanForCourse developmentPlanForCourse = _db.DevelopmentPlanForCourse.Where(a => a.Id == developmentId && a.IsDeleted == false).FirstOrDefault();

            if (developmentPlanForCourse == null)
            {
                return null;
            }
            List<CourseMappingToDevelopment> developmentPlanCourses = _db.CourseMappingToDevelopment.Where(a => a.DevelopmentPlanId == developmentId && a.IsDeleted == false).ToList();

            if (developmentPlanCourses == null)
            {
                return null;
            }
            List<DevelopmentCoursesDetails> developmentCoursesDetails = new List<DevelopmentCoursesDetails>();

            foreach (CourseMappingToDevelopment developmentPlanCourse in developmentPlanCourses)
            {
                MyCourse.API.Model.Course course = await _courseRepository.Get(developmentPlanCourse.CourseId);
                DevelopmentCoursesDetails developmentCoursesDetails1 = new DevelopmentCoursesDetails();
                developmentCoursesDetails1.Id = course.Id;
                developmentCoursesDetails1.Code = course.Code;
                developmentCoursesDetails1.CompletionPeriodDays = course.CompletionPeriodDays;
                developmentCoursesDetails1.creditsPoints = course.CreditsPoints;
                developmentCoursesDetails1.TotalModules = course.TotalModules;
                developmentCoursesDetails1.Title = course.Title;
                developmentCoursesDetails1.sequenceNo = developmentPlanCourse.sequenceNo;
                developmentCoursesDetails.Add(developmentCoursesDetails1);
            }
            return developmentCoursesDetails.OrderBy(a=>a.sequenceNo).ToList();
        }
        public async Task<List<APIDevelopmentPlanType>> GetDevelopmentPlanAccessibility(string search)
        {
            var data = (from c in _db.DevelopmentPlanForCourse

                        where (c.DevelopmentName.Contains(search) && c.IsDeleted == false && c.Status == true)

                        select new APIDevelopmentPlanType
                        {
                            Id = c.Id,
                            DevelopmentCode = c.DevelopmentCode,
                            DevelopmentName = c.DevelopmentName
                        });
            return await data.OrderByDescending(c => c.Id).ToListAsync();
        }
        public async Task<List<Mappingparameter>> CheckmappingStatus(MappingParameters mappingParameters, int UserId)
        {
            List<Mappingparameter> rejectMappingParameter = new List<Mappingparameter>();
            Mappingparameter RejectMapping = new Mappingparameter();
            if (mappingParameters == null)
            {
                return null;
            }
            else
            {

                DevelopmentPlanForCourse developmentPlanForCourse = _db.DevelopmentPlanForCourse.Where(a => a.Id == mappingParameters.DevelopmentPlanid && a.IsDeleted == false).FirstOrDefault();
                List<DevelopmentPlanApplicableUser> aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();
                UserTeams userTeams = new UserTeams();
                List<UserMaster> userMasters = new List<UserMaster>();

                if (developmentPlanForCourse != null)
                {
                    var Mapping = _db.UserDevelopmentPlanMapping.Where(a => a.DevelopmentPlanid == developmentPlanForCourse.Id);
                    UserDevelopmentPlanMapping userDevelopmentPlanMapping = new UserDevelopmentPlanMapping();

                    switch (mappingParameters.AccessibilityParameter1.ToLower())
                    {

                        case "configurationcolumn1":
                            userDevelopmentPlanMapping.ConfigurationColumn1 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "configurationcolumn2":

                            userDevelopmentPlanMapping.ConfigurationColumn2 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "configurationcolumn3":

                            userDevelopmentPlanMapping.ConfigurationColumn3 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn4":

                            userDevelopmentPlanMapping.ConfigurationColumn4 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn5":

                            userDevelopmentPlanMapping.ConfigurationColumn5 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn6":

                            userDevelopmentPlanMapping.ConfigurationColumn6 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn7":

                            userDevelopmentPlanMapping.ConfigurationColumn7 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn8":

                            userDevelopmentPlanMapping.ConfigurationColumn8 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn9":

                            userDevelopmentPlanMapping.ConfigurationColumn9 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn10":

                            userDevelopmentPlanMapping.ConfigurationColumn10 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn11":

                            userDevelopmentPlanMapping.ConfigurationColumn11 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn12":

                            userDevelopmentPlanMapping.ConfigurationColumn12 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "configurationcolumn13":

                            userDevelopmentPlanMapping.ConfigurationColumn13 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn14":

                            userDevelopmentPlanMapping.ConfigurationColumn14 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn15":

                            userDevelopmentPlanMapping.ConfigurationColumn15 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "area":

                            userDevelopmentPlanMapping.Area = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "business":

                            userDevelopmentPlanMapping.Business = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "emailid":

                            userDevelopmentPlanMapping.EmailID = mappingParameters.AccessibilityValue1;
                            break;
                        case "location":

                            userDevelopmentPlanMapping.Location = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "group":

                            userDevelopmentPlanMapping.Group = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "userid":

                            userDevelopmentPlanMapping.UserID = Convert.ToInt32(mappingParameters.AccessibilityValue1);

                            break;
                        case "mobilenumber":

                            userDevelopmentPlanMapping.MobileNumber = mappingParameters.AccessibilityValue1;

                            break;

                        case "userteamid":

                            userDevelopmentPlanMapping.UserTeamId = Convert.ToInt32(mappingParameters.AccessibilityValue1);

                            break;

                    }




                    if (mappingParameters.AccessibilityParameter2 == null)
                    {

                        userDevelopmentPlanMapping.ConditionForRules = mappingParameters.condition1;
                        userDevelopmentPlanMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                        userDevelopmentPlanMapping.CreatedBy = UserId;
                        userDevelopmentPlanMapping.ModifiedBy = UserId;
                        userDevelopmentPlanMapping.CreatedDate = DateTime.Now;
                        userDevelopmentPlanMapping.ModifiedDate = DateTime.Now;

                        bool Result = await RuleExist(userDevelopmentPlanMapping);

                        if (!Result)
                        {
                            this._db.UserDevelopmentPlanMapping.Add(userDevelopmentPlanMapping);
                            this._db.SaveChanges();

                            developmentPlanForCourse.NumberOfRules++;
                            developmentPlanForCourse.ModifiedBy = UserId;
                            developmentPlanForCourse.ModifiedDate = DateTime.Now;
                            aPIUserMasterDetails = await GetDevelopmentPlanApplicableUserList(developmentPlanForCourse.Id);
                            developmentPlanForCourse.NumberofMembers = aPIUserMasterDetails.Count();
                            this._db.DevelopmentPlanForCourse.Update(developmentPlanForCourse);
                            this._db.SaveChanges();


                            aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();

                            if (developmentPlanForCourse != null)
                            {
                                _db.Entry(developmentPlanForCourse).State = EntityState.Detached;
                            }
                        }
                        else
                        {
                            RejectMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                            RejectMapping.AccessibilityParameter1 = mappingParameters.AccessibilityParameter1;
                            RejectMapping.ParameterValue1 = mappingParameters.AccessibilityValue1;
                            rejectMappingParameter.Add(RejectMapping);
                        }

                        if (developmentPlanForCourse != null)
                        {
                            _db.Entry(developmentPlanForCourse).State = EntityState.Detached;
                        }

                    }

                    if (mappingParameters.AccessibilityParameter2 != null)
                    {
                        switch (mappingParameters.AccessibilityParameter2.ToLower())
                        {
                            case "configurationcolumn1":
                                userDevelopmentPlanMapping.ConfigurationColumn1 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn2":
                                userDevelopmentPlanMapping.ConfigurationColumn2 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn3":
                                userDevelopmentPlanMapping.ConfigurationColumn3 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn4":
                                userDevelopmentPlanMapping.ConfigurationColumn4 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn5":
                                userDevelopmentPlanMapping.ConfigurationColumn5 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn6":
                                userDevelopmentPlanMapping.ConfigurationColumn6 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn7":
                                userDevelopmentPlanMapping.ConfigurationColumn7 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn8":
                                userDevelopmentPlanMapping.ConfigurationColumn8 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn9":
                                userDevelopmentPlanMapping.ConfigurationColumn9 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn10":
                                userDevelopmentPlanMapping.ConfigurationColumn10 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn11":
                                userDevelopmentPlanMapping.ConfigurationColumn11 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn12":
                                userDevelopmentPlanMapping.ConfigurationColumn12 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn13":

                                userDevelopmentPlanMapping.ConfigurationColumn13 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn14":

                                userDevelopmentPlanMapping.ConfigurationColumn14 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn15":

                                userDevelopmentPlanMapping.ConfigurationColumn15 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "area":
                                userDevelopmentPlanMapping.Area = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "business":
                                userDevelopmentPlanMapping.Business = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "emailid":
                                userDevelopmentPlanMapping.EmailID = mappingParameters.AccessibilityValue2;
                                break;

                            case "location":
                                userDevelopmentPlanMapping.Location = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "group":
                                userDevelopmentPlanMapping.Group = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "userid":
                                userDevelopmentPlanMapping.UserID = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "mobilenumber":
                                userDevelopmentPlanMapping.MobileNumber = mappingParameters.AccessibilityValue2;
                                break;

                            case "userteamid":

                                userDevelopmentPlanMapping.UserTeamId = Convert.ToInt32(mappingParameters.AccessibilityValue2);

                                break;
                        }


                        userDevelopmentPlanMapping.ConditionForRules = mappingParameters.condition1;
                        userDevelopmentPlanMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                        userDevelopmentPlanMapping.CreatedBy = UserId;
                        userDevelopmentPlanMapping.ModifiedBy = UserId;
                        userDevelopmentPlanMapping.CreatedDate = DateTime.Now;
                        userDevelopmentPlanMapping.ModifiedDate = DateTime.Now;

                        bool Result = await RuleExist(userDevelopmentPlanMapping);

                        if (!Result)
                        {

                            this._db.UserDevelopmentPlanMapping.Add(userDevelopmentPlanMapping);
                            this._db.SaveChanges();

                            developmentPlanForCourse.NumberOfRules++;
                            developmentPlanForCourse.ModifiedBy = UserId;
                            developmentPlanForCourse.ModifiedDate = DateTime.Now;

                            aPIUserMasterDetails = await GetDevelopmentPlanApplicableUserList(developmentPlanForCourse.Id);
                            developmentPlanForCourse.NumberofMembers = aPIUserMasterDetails.Count();

                            this._db.DevelopmentPlanForCourse.Update(developmentPlanForCourse);
                            this._db.SaveChanges();

                            aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();
                        }
                        else
                        {
                            RejectMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                            RejectMapping.AccessibilityParameter1 = mappingParameters.AccessibilityParameter1;
                            RejectMapping.ParameterValue1 = mappingParameters.AccessibilityValue1;
                            RejectMapping.AccessibilityParameter2 = mappingParameters.AccessibilityParameter2;
                            RejectMapping.ParameterValue2 = mappingParameters.AccessibilityValue2;

                            rejectMappingParameter.Add(RejectMapping);
                        }
                    }
                    else
                    {
                        if (rejectMappingParameter.Count() != 0)
                        {
                            return rejectMappingParameter;
                        }
                        return null;
                    }


                    if (developmentPlanForCourse != null)
                    {
                        _db.Entry(developmentPlanForCourse).State = EntityState.Detached;
                    }

                    return rejectMappingParameter;
                }
                else
                {
                    RejectMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                    rejectMappingParameter.Add(RejectMapping);

                    return rejectMappingParameter;
                }
            }
        }
        public async Task<bool> RuleExist(UserDevelopmentPlanMapping accessibilityRule)
        {
            IQueryable<UserDevelopmentPlanMapping> Query = this._db.UserDevelopmentPlanMapping.Where(a => a.DevelopmentPlanid == accessibilityRule.DevelopmentPlanid && a.IsDeleted == false);

            if (accessibilityRule.Area != null)
                Query = Query.Where(a => a.Area == accessibilityRule.Area);
            if (accessibilityRule.Business != null)
                Query = Query.Where(a => a.Business == accessibilityRule.Business);
            if (accessibilityRule.ConfigurationColumn1 != null)
                Query = Query.Where(a => a.ConfigurationColumn1 == accessibilityRule.ConfigurationColumn1);
            if (accessibilityRule.ConfigurationColumn2 != null)
                Query = Query.Where(a => a.ConfigurationColumn2 == accessibilityRule.ConfigurationColumn2);
            if (accessibilityRule.ConfigurationColumn3 != null)
                Query = Query.Where(a => a.ConfigurationColumn3 == accessibilityRule.ConfigurationColumn3);
            if (accessibilityRule.ConfigurationColumn4 != null)
                Query = Query.Where(a => a.ConfigurationColumn4 == accessibilityRule.ConfigurationColumn4);
            if (accessibilityRule.ConfigurationColumn5 != null)
                Query = Query.Where(a => a.ConfigurationColumn5 == accessibilityRule.ConfigurationColumn5);
            if (accessibilityRule.ConfigurationColumn6 != null)
                Query = Query.Where(a => a.ConfigurationColumn6 == accessibilityRule.ConfigurationColumn6);
            if (accessibilityRule.ConfigurationColumn7 != null)
                Query = Query.Where(a => a.ConfigurationColumn7 == accessibilityRule.ConfigurationColumn7);
            if (accessibilityRule.ConfigurationColumn8 != null)
                Query = Query.Where(a => a.ConfigurationColumn8 == accessibilityRule.ConfigurationColumn8);
            if (accessibilityRule.ConfigurationColumn9 != null)
                Query = Query.Where(a => a.ConfigurationColumn9 == accessibilityRule.ConfigurationColumn9);
            if (accessibilityRule.ConfigurationColumn10 != null)
                Query = Query.Where(a => a.ConfigurationColumn10 == accessibilityRule.ConfigurationColumn10);
            if (accessibilityRule.ConfigurationColumn11 != null)
                Query = Query.Where(a => a.ConfigurationColumn11 == accessibilityRule.ConfigurationColumn11);
            if (accessibilityRule.ConfigurationColumn12 != null)
                Query = Query.Where(a => a.ConfigurationColumn12 == accessibilityRule.ConfigurationColumn12);
            if (accessibilityRule.ConfigurationColumn13 != null)
                Query = Query.Where(a => a.ConfigurationColumn13 == accessibilityRule.ConfigurationColumn13);
            if (accessibilityRule.ConfigurationColumn14 != null)
                Query = Query.Where(a => a.ConfigurationColumn14 == accessibilityRule.ConfigurationColumn14);
            if (accessibilityRule.ConfigurationColumn15 != null)
                Query = Query.Where(a => a.ConfigurationColumn15 == accessibilityRule.ConfigurationColumn15);
            if (accessibilityRule.MobileNumber != null)
                Query = Query.Where(a => a.MobileNumber == accessibilityRule.MobileNumber);
            if (accessibilityRule.EmailID != null)
                Query = Query.Where(a => a.EmailID == accessibilityRule.EmailID);
            if (accessibilityRule.Location != null)
                Query = Query.Where(a => a.Location == accessibilityRule.Location);
            if (accessibilityRule.Group != null)
                Query = Query.Where(a => a.Group == accessibilityRule.Group);
            if (accessibilityRule.UserID != null)
                Query = Query.Where(a => a.UserID == accessibilityRule.UserID);
            if (accessibilityRule.UserTeamId != null)
                Query = Query.Where(a => a.UserTeamId == accessibilityRule.UserTeamId);


            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }
        public async Task<List<MappingParameters>> GetAccessibilityRules(int DevelopmentPlanId, string orgnizationCode, string token, int Page, int PageSize)
        {
            var Result = await (from UserDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                                join DevelopmentPlanForCourse in _db.DevelopmentPlanForCourse on UserDevelopmentPlanMapping.DevelopmentPlanid equals DevelopmentPlanForCourse.Id
                                into c
                                from DevelopmentPlanForCourse in c.DefaultIfEmpty()
                                where UserDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && UserDevelopmentPlanMapping.IsDeleted == false
                                select new
                                {
                                    UserDevelopmentPlanMapping.ConfigurationColumn1,
                                    UserDevelopmentPlanMapping.ConfigurationColumn2,
                                    UserDevelopmentPlanMapping.ConfigurationColumn3,
                                    UserDevelopmentPlanMapping.ConfigurationColumn4,
                                    UserDevelopmentPlanMapping.ConfigurationColumn5,
                                    UserDevelopmentPlanMapping.ConfigurationColumn6,
                                    UserDevelopmentPlanMapping.ConfigurationColumn7,
                                    UserDevelopmentPlanMapping.ConfigurationColumn8,
                                    UserDevelopmentPlanMapping.ConfigurationColumn9,
                                    UserDevelopmentPlanMapping.ConfigurationColumn10,
                                    UserDevelopmentPlanMapping.ConfigurationColumn11,
                                    UserDevelopmentPlanMapping.ConfigurationColumn12,
                                    UserDevelopmentPlanMapping.ConfigurationColumn13,
                                    UserDevelopmentPlanMapping.ConfigurationColumn14,
                                    UserDevelopmentPlanMapping.ConfigurationColumn15,
                                    UserDevelopmentPlanMapping.Area,
                                    UserDevelopmentPlanMapping.Business,
                                    UserDevelopmentPlanMapping.EmailID,
                                    UserDevelopmentPlanMapping.MobileNumber,
                                    UserDevelopmentPlanMapping.Location,
                                    UserDevelopmentPlanMapping.Group,
                                    UserDevelopmentPlanMapping.UserID,
                                    UserDevelopmentPlanMapping.ConditionForRules,
                                    UserDevelopmentPlanMapping.UserTeamId,
                                    UserDevelopmentPlanMapping.Id,
                                    UserDevelopmentPlanMapping.DevelopmentPlanid


                                }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            List<MappingParameters> AccessibilityRules = new List<MappingParameters>();

            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<RulesForDevelopment> Rules = new List<RulesForDevelopment>();
                int DevelopmentPlanid1 = 0;
                int Id = 0;
                int i = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("developmentplanid"))
                        DevelopmentPlanid1 = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("DevelopmentPlanid") &&
                        !rule.Name.Equals("Id"))
                    {

                        RulesForDevelopment Rule = new RulesForDevelopment
                        {
                            AccessibilityParameter = rule.Name,
                            AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                            Condition = Condition,
                            Id = Id
                        };
                        Rules.Add(Rule);

                    }
                    i++;
                }
                if (Rules.Count == 2)
                {
                    MappingParameters ApiRule = new MappingParameters
                    {
                        Id = Id,
                        DevelopmentPlanid = DevelopmentPlanid1,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                        condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValue2 = Rules.ElementAt(1).AccessibilityValue,
                        AccessibilityValueId2 = !string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(1).AccessibilityValue) : 0,


                    };

                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    MappingParameters ApiRule = new MappingParameters
                    {
                        DevelopmentPlanid = DevelopmentPlanid1,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,

                        AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,


                    };
                    AccessibilityRules.Add(ApiRule);
                }
            }

            string UserUrls = _configuration[APIHelper.UserAPI];
            string settings = "setting/1/20/";
            UserUrls += settings;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrls, token);
            List<ConfiguredColumns> ConfiguredColumns = new List<ConfiguredColumns>();
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ConfiguredColumns = JsonConvert.DeserializeObject<List<ConfiguredColumns>>(result);
            }
            foreach (MappingParameters AccessRule in AccessibilityRules)
            {
                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = AccessRule.AccessibilityParameter1;
                int Value = AccessRule.AccessibilityValueId1;
                string Apiurl = UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value;
                response = await APIHelper.CallGetAPI(Apiurl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Title _Title = JsonConvert.DeserializeObject<Title>(result);
                    if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                        AccessRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                }
                if (AccessRule.AccessibilityValueId2 != 0)
                {
                    ColumnName = AccessRule.AccessibilityParameter2;
                    Value = AccessRule.AccessibilityValueId2;
                    response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                            AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                }
                if (ConfiguredColumns.Count > 0)
                {
                    if (AccessRule.AccessibilityParameter1 == "UserID")
                    {
                        AccessRule.AccessibilityParameter1 = "UserID";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "MobileNumber")
                    {
                        AccessRule.AccessibilityParameter1 = "MobileNumber";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "EmailID")
                    {
                        AccessRule.AccessibilityParameter1 = "EmailID";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "DateOfJoining")
                    {
                        AccessRule.AccessibilityParameter1 = "Date Of Joining";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "UserTeamId")
                    {
                        AccessRule.AccessibilityParameter1 = "User Team";
                        UserTeams userTeams = _db.UserTeams.Where(a => a.Id == AccessRule.AccessibilityValueId1).FirstOrDefault();
                        AccessRule.AccessibilityValue1 = userTeams.TeamName;
                    }
                    else
                    {
                        AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                    if (AccessRule.AccessibilityParameter2 == "UserID")
                    {
                        AccessRule.AccessibilityParameter2 = "UserID";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "MobileNumber")
                    {
                        AccessRule.AccessibilityParameter2 = "MobileNumber";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "EmailID")
                    {
                        AccessRule.AccessibilityParameter2 = "EmailID";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "DateOfJoining")
                    {
                        AccessRule.AccessibilityParameter2 = "Date Of Joining";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "UserTeamId")
                    {
                        AccessRule.AccessibilityParameter2 = "User Team";
                        UserTeams userTeams = _db.UserTeams.Where(a => a.Id == AccessRule.AccessibilityValueId2).FirstOrDefault();
                        AccessRule.AccessibilityValue2 = userTeams.TeamName;
                    }
                    else
                    {
                        AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                }

            }
            return AccessibilityRules;
        }
        public async Task<int> GetAccessibilityRulesCount(int DevelopmentPlanId)
        {
            int Count = 0;
            Count = await (from UserDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                           join DevelopmentPlanForCourse in _db.DevelopmentPlanForCourse on UserDevelopmentPlanMapping.DevelopmentPlanid equals DevelopmentPlanForCourse.Id
                           into c
                           from DevelopmentPlanForCourse in c.DefaultIfEmpty()
                           where UserDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && UserDevelopmentPlanMapping.IsDeleted == false
                           select new
                           {
                               UserDevelopmentPlanMapping.ConfigurationColumn1,
                               UserDevelopmentPlanMapping.ConfigurationColumn2,
                               UserDevelopmentPlanMapping.ConfigurationColumn3,
                               UserDevelopmentPlanMapping.ConfigurationColumn4,
                               UserDevelopmentPlanMapping.ConfigurationColumn5,
                               UserDevelopmentPlanMapping.ConfigurationColumn6,
                               UserDevelopmentPlanMapping.ConfigurationColumn7,
                               UserDevelopmentPlanMapping.ConfigurationColumn8,
                               UserDevelopmentPlanMapping.ConfigurationColumn9,
                               UserDevelopmentPlanMapping.ConfigurationColumn10,
                               UserDevelopmentPlanMapping.ConfigurationColumn11,
                               UserDevelopmentPlanMapping.ConfigurationColumn12,
                               UserDevelopmentPlanMapping.ConfigurationColumn13,
                               UserDevelopmentPlanMapping.ConfigurationColumn14,
                               UserDevelopmentPlanMapping.ConfigurationColumn15,
                               UserDevelopmentPlanMapping.Area,
                               UserDevelopmentPlanMapping.Business,
                               UserDevelopmentPlanMapping.EmailID,
                               UserDevelopmentPlanMapping.MobileNumber,
                               UserDevelopmentPlanMapping.Location,
                               UserDevelopmentPlanMapping.Group,
                               UserDevelopmentPlanMapping.UserID,
                               UserDevelopmentPlanMapping.ConditionForRules,
                               UserDevelopmentPlanMapping.UserTeamId,
                               UserDevelopmentPlanMapping.Id

                           }).CountAsync();
            return Count;
        }
        public async Task<int> DeleteRule(int roleId)
        {
            UserDevelopmentPlanMapping accessibilityRule = _db.UserDevelopmentPlanMapping.Where(a => a.Id == roleId && a.IsDeleted == false).FirstOrDefault();

            if (accessibilityRule != null)
            {
                DevelopmentPlanForCourse userTeams = _db.DevelopmentPlanForCourse.Where(a => a.Id == accessibilityRule.DevelopmentPlanid && a.IsDeleted == false).FirstOrDefault();

                accessibilityRule.IsDeleted = true;
                _db.UserDevelopmentPlanMapping.Update(accessibilityRule);
                await _db.SaveChangesAsync();

                List<DevelopmentPlanApplicableUser> aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();
                List<UserMaster> userMasters = new List<UserMaster>();

                if (userTeams != null)
                {
                    if (userTeams.DevelopmentCode != null)
                    {

                        aPIUserMasterDetails = await GetDevelopmentPlanApplicableUserList(userTeams.Id);
                        userTeams.NumberofMembers = aPIUserMasterDetails.Count();
                        userTeams.NumberOfRules = userTeams.NumberOfRules - 1;
                        _db.DevelopmentPlanForCourse.Update(userTeams);
                        await _db.SaveChangesAsync();
                        return 1;
                    }
                }


                _db.DevelopmentPlanForCourse.Update(userTeams);
                await _db.SaveChangesAsync();
                return 1;
            }
            return 0;
        }
        public async Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int DevelopmentPlanId)
        {
            if (AccessibilityParameter1 == null || AccessibilityValue1 == null)
            {
                return false;
            }
            bool isvalid = true;

            if (_db.DevelopmentPlanForCourse.Where(y => y.Id == DevelopmentPlanId && y.IsDeleted == false).Count() <= 0)
            {
                isvalid = false;
                return isvalid;
            }

            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            if (AccessibilityParameter1.ToLower() != "userteamid")
                            {
                                cmd.CommandText = "CheckValidDataForUserSetting";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter1", SqlDbType.VarChar) { Value = AccessibilityParameter1 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityValue1", SqlDbType.VarChar) { Value = AccessibilityValue1 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter2", SqlDbType.VarChar) { Value = AccessibilityParameter2 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityValue2", SqlDbType.VarChar) { Value = AccessibilityValue2 });

                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count > 0)
                                {
                                    isvalid = Boolean.Parse(dt.Rows[0]["IsValid"].ToString());
                                }
                                reader.Dispose();
                                connection.Close();
                            }
                            else
                            {
                                UserTeams userTeams = _db.UserTeams.Where(a => a.Id == Convert.ToInt32(AccessibilityValue1) && a.IsDeleted == false).FirstOrDefault();
                                if (userTeams != null)
                                {
                                    isvalid = true;
                                }
                                else
                                {
                                    isvalid = false;
                                }
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return isvalid;
        }
        
        public async Task<string> GetDevelopmentPlanName(int DevelopmentPlanId)
        {
            var DevelopmentPlanName = await (from c in _db.DevelopmentPlanForCourse
                                             where c.IsDeleted == false && c.Id == DevelopmentPlanId
                                             select c.DevelopmentName).SingleOrDefaultAsync();
            return DevelopmentPlanName;
        }
        public async Task<List<APIAccessibilityRulesDevelopment>> GetAccessibilityRulesForExport(int DevelopmentPlanId, string orgnizationCode, string token, string DevelopmentPlanName)
        {
            var Result = await (from userDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                                join developmentPlanForCourse in _db.DevelopmentPlanForCourse on userDevelopmentPlanMapping.DevelopmentPlanid equals developmentPlanForCourse.Id
                                into c
                                from developmentPlanForCourse in c.DefaultIfEmpty()
                                where userDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && userDevelopmentPlanMapping.IsDeleted == false
                                select new
                                {
                                    userDevelopmentPlanMapping.ConfigurationColumn1,
                                    userDevelopmentPlanMapping.ConfigurationColumn2,
                                    userDevelopmentPlanMapping.ConfigurationColumn3,
                                    userDevelopmentPlanMapping.ConfigurationColumn4,
                                    userDevelopmentPlanMapping.ConfigurationColumn5,
                                    userDevelopmentPlanMapping.ConfigurationColumn6,
                                    userDevelopmentPlanMapping.ConfigurationColumn7,
                                    userDevelopmentPlanMapping.ConfigurationColumn8,
                                    userDevelopmentPlanMapping.ConfigurationColumn9,
                                    userDevelopmentPlanMapping.ConfigurationColumn10,
                                    userDevelopmentPlanMapping.ConfigurationColumn11,
                                    userDevelopmentPlanMapping.ConfigurationColumn12,
                                    userDevelopmentPlanMapping.ConfigurationColumn13,
                                    userDevelopmentPlanMapping.ConfigurationColumn14,
                                    userDevelopmentPlanMapping.ConfigurationColumn15,
                                    userDevelopmentPlanMapping.Area,
                                    userDevelopmentPlanMapping.Business,
                                    userDevelopmentPlanMapping.EmailID,
                                    userDevelopmentPlanMapping.MobileNumber,
                                    userDevelopmentPlanMapping.Location,
                                    userDevelopmentPlanMapping.Group,
                                    userDevelopmentPlanMapping.UserID,
                                    userDevelopmentPlanMapping.ConditionForRules,
                                    userDevelopmentPlanMapping.DevelopmentPlanid,
                                    userDevelopmentPlanMapping.Id,
                                    developmentPlanForCourse.DevelopmentName,

                                }).ToListAsync();


            var UserTeamsApplicability = await (from userDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                                                join developmentPlanForCourse in _db.DevelopmentPlanForCourse on userDevelopmentPlanMapping.DevelopmentPlanid equals developmentPlanForCourse.Id
                                                join userTeams in _db.UserTeams on userDevelopmentPlanMapping.UserTeamId equals userTeams.Id
                                                into d
                                                from userTeams in d.DefaultIfEmpty()
                                                where (userDevelopmentPlanMapping.UserTeamId != null && userDevelopmentPlanMapping.UserTeamId != 0) && userDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && userDevelopmentPlanMapping.IsDeleted == false
                                                select new
                                                {
                                                    userDevelopmentPlanMapping.DevelopmentPlanid,
                                                    userDevelopmentPlanMapping.Id,
                                                    developmentPlanForCourse.DevelopmentName,
                                                    userDevelopmentPlanMapping.UserTeamId,
                                                    userTeams.TeamName
                                                }).ToListAsync();

            List<APIAccessibilityRulesDevelopment> AccessibilityRules = new List<APIAccessibilityRulesDevelopment>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int Developmentplanid = 0;
                int Id = 0;
                int i = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("developmentplanid"))
                        Developmentplanid = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("DevelopmentName") &&
                        !rule.Name.Equals("DevelopmentPlanid") &&
                        !rule.Name.Equals("Id"))
                    {

                        Rules Rule = new Rules
                        {
                            AccessibilityParameter = rule.Name,
                            AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                            Condition = Condition
                        };
                        Rules.Add(Rule);

                    }
                    i++;
                }
                if (Rules.Count == 2)
                {
                    APIAccessibilityRulesDevelopment ApiRule = new APIAccessibilityRulesDevelopment
                    {
                        DevelopmentPlanid = Developmentplanid,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                        AccessibilityValue1 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue : null,
                        AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                        Condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValueId2 = !string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(1).AccessibilityValue) : 0,
                        AccessibilityValue2 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue : null,
                        AccessibilityValue22 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue2 : null,
                    };
                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    APIAccessibilityRulesDevelopment ApiRule = new APIAccessibilityRulesDevelopment
                    {
                        DevelopmentPlanid = Developmentplanid,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                        AccessibilityValue1 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue : null,
                        AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                    };
                    AccessibilityRules.Add(ApiRule);
                }
            }
            string UserUrls = _configuration[APIHelper.UserAPI];
            string settings = "setting/1/20/";
            UserUrls += settings;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrls, token);
            List<ConfiguredColumns> ConfiguredColumns = new List<ConfiguredColumns>();
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ConfiguredColumns = JsonConvert.DeserializeObject<List<ConfiguredColumns>>(result);
            }
            foreach (APIAccessibilityRulesDevelopment AccessRule in AccessibilityRules)
            {
                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = AccessRule.AccessibilityParameter1;
                int Value = AccessRule.AccessibilityValueId1;
                string Apiurl = UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value;
                response = await APIHelper.CallGetAPI(Apiurl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Title _Title = JsonConvert.DeserializeObject<Title>(result);
                    if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                        AccessRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                }
                if (AccessRule.AccessibilityValueId2 != 0)
                {
                    ColumnName = AccessRule.AccessibilityParameter2;
                    Value = AccessRule.AccessibilityValueId2;
                    response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                            AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                }
                if (ConfiguredColumns.Count > 0)
                {
                    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Count() > 0 ? ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault() : AccessRule.AccessibilityParameter1;
                    AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Count() > 0 ? ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault() : AccessRule.AccessibilityParameter2;
                }
            }

            if (UserTeamsApplicability != null)
            {
                foreach (var item in UserTeamsApplicability)
                {
                    int DevelopmentPlan = 0;

                    DevelopmentPlan = Int32.Parse(item.DevelopmentPlanid.ToString());

                    APIAccessibilityRulesDevelopment accessRule = new APIAccessibilityRulesDevelopment
                    {
                        Id = item.Id,
                        AccessibilityParameter1 = "User Team",
                        AccessibilityValue1 = item.TeamName,
                        AccessibilityValueId1 = Int32.Parse(item.UserTeamId.ToString()),
                        DevelopmentPlanid = DevelopmentPlan
                    };
                    AccessibilityRules.Add(accessRule);
                }
            }

            return AccessibilityRules;
        }
        public List<UserDevelopmentPlanMapping> GetRuleByUserTeams(int developmentPlanid)
        {
            List<UserDevelopmentPlanMapping> accessibilityRule = _db.UserDevelopmentPlanMapping.Where(a => a.DevelopmentPlanid == developmentPlanid && a.UserTeamId != null && a.IsDeleted == false).ToList();

            return accessibilityRule;
        }
        public async Task<List<DevelopmentPlanApplicableUser>> GetDevelopmentPlanApplicableUserList(int DevelopmentPlanId)
        {
            List<DevelopmentPlanApplicableUser> listUserApplicability = new List<DevelopmentPlanApplicableUser>();
            List<DevelopmentPlanApplicableUser> UserList1 = new List<DevelopmentPlanApplicableUser>();
            var connection = this._db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetDevelopmentPlanApplicableUserList_Export";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@DevelopmentPlanID", SqlDbType.BigInt) { Value = DevelopmentPlanId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            DevelopmentPlanApplicableUser rule = new DevelopmentPlanApplicableUser();
                            rule.UserID = Security.Decrypt(row["UserID"].ToString());
                            rule.UserName = row["UserName"].ToString();
                            listUserApplicability.Add(rule);
                        }
                    }
                    List<UserDevelopmentPlanMapping> accessibilityRule = GetRuleByUserTeams(DevelopmentPlanId);
                    List<DevelopmentPlanApplicableUser> UserListForUserTeam = new List<DevelopmentPlanApplicableUser>();


                    if (accessibilityRule != null)
                    {
                        foreach (UserDevelopmentPlanMapping accessibilityRule1 in accessibilityRule)
                        {
                            List<CourseApplicableUser> UserListForUserTeam1 = this._accessibilityRule.GetUsersForUserTeam(accessibilityRule1.UserTeamId);
                            foreach (CourseApplicableUser courseApplicableUser1 in UserListForUserTeam1)
                            {
                                DevelopmentPlanApplicableUser developmentPlanApplicableUser = new DevelopmentPlanApplicableUser();
                                developmentPlanApplicableUser.UserID = courseApplicableUser1.UserID;
                                developmentPlanApplicableUser.UserName = courseApplicableUser1.UserName;
                                UserListForUserTeam.Add(developmentPlanApplicableUser);
                            }
                        }
                        
                    }
                    listUserApplicability.AddRange(UserListForUserTeam);

                    UserList1 = listUserApplicability.GroupBy(p => new { p.UserID, p.UserName })
                    .Select(g => g.First())
                    .ToList();
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return UserList1;
        }
        public FileInfo GetApplicableUserListExcel(List<APIAccessibilityRulesDevelopment> aPIAccessibilityRules, List<DevelopmentPlanApplicableUser> courseApplicableUsers, string DevelopmentPlanName, string OrgCode)
        {

            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            string sFileName = @"DevelopmentPlanApplicableUser.xlsx";
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Development Plan Applicability");
                //First add the headers
                int row = 1, column = 1;
                worksheet.Cells[row, column].Value = "Development Plan Name";
                row++;
                worksheet.Cells[row, column].Value = DevelopmentPlanName;
                row++;
                column = 1;
                row++;
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                //{
                //    worksheet.Cells[row, column++].Value = "Accessibility Value11";
                //    worksheet.Cells[row, column].Style.Font.Bold = true;
                //}
                worksheet.Cells[row, column++].Value = "Additional Criteria";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                //{
                //    worksheet.Cells[row, column++].Value = "Accessibility Value22";
                //    worksheet.Cells[row, column].Style.Font.Bold = true;
                //}

                foreach (APIAccessibilityRulesDevelopment course in aPIAccessibilityRules)
                {
                    column = 1; row++;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter1 == null ? "-" : course.AccessibilityParameter1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue1 == null ? "-" : course.AccessibilityValue1;
                    //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    worksheet.Cells[row, column++].Value = course.AccessibilityValue11 == null ? "-" : course.AccessibilityValue11;
                    //}
                    worksheet.Cells[row, column++].Value = course.Condition1 == null ? "-" : course.Condition1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter2 == null ? "-" : course.AccessibilityParameter2;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue2 == null ? "-" : course.AccessibilityValue2;
                    if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    {
                        worksheet.Cells[row, column++].Value = course.AccessibilityValue22 == null ? "-" : course.AccessibilityValue22;
                    }
                }
                row++;
                row++;

                worksheet.Cells[row, 1].Value = "Applicable Users";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                row++;
                worksheet.Cells[row, 1].Value = "UserId";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = "UserName";
                worksheet.Cells[row, 2].Style.Font.Bold = true;

                foreach (DevelopmentPlanApplicableUser courseApplicableUser in courseApplicableUsers)
                {
                    row++; column = 1;
                    worksheet.Cells[row, column++].Value = courseApplicableUser.UserID == null ? "-" : courseApplicableUser.UserID;
                    worksheet.Cells[row, column++].Value = courseApplicableUser.UserName == null ? "-" : courseApplicableUser.UserName;

                }

                using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                {
                    rngitems.Style.Font.Bold = true;

                }

                package.Save(); //Save the workbook.

            }
            return file;

        }


        public async Task<UserApplicableDevPlanTotal> GetUserDevPlan(int userId, int page, int pageSize, string search = null, int? devplanID=null)
        {

           UserApplicableDevPlanTotal DevPlan = new UserApplicableDevPlanTotal();
            List<UserApplicableDevPlan> userdevplanList = new List<UserApplicableDevPlan>();

            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetApplicableDevPlanToUser";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });                        
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@devplanID", SqlDbType.Int) { Value = devplanID });

                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            UserApplicableDevPlan userdevplans = new UserApplicableDevPlan();
                            userdevplans.AboutPlan = row["AboutPlan"].ToString();
                            userdevplans.UploadThumbnail = row["UploadThumbnail"].ToString();
                            userdevplans.DevelopmentName = row["DevelopmentName"].ToString();
                            userdevplans.DevelopmentCode = row["DevelopmentCode"].ToString();
                            userdevplans.Status = row["Status"].ToString();
                            userdevplans.Id =Convert.ToInt32(row["Id"].ToString());
                            userdevplans.FeedbackId = string.IsNullOrEmpty(row["FeedbackId"].ToString()) ? (int?)null : Convert.ToInt32(row["FeedbackId"]);
                            userdevplans.EnableFeedback = Convert.ToBoolean(row["EnableFeedback"]);
                            userdevplans.EnforceLinearApproach = Convert.ToBoolean(row["EnforceLinearApproach"]);
                            userdevplanList.Add(userdevplans);
                        }
                        reader.Dispose();                        
                    }
                }
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetApplicableDevPlanToUser";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = 0 });
                        cmd.Parameters.Add(new SqlParameter("@devplanID", SqlDbType.Int) { Value = devplanID });

                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }                       
                        foreach (DataRow row in dt.Rows)
                        {
                            DevPlan.TotalRecords = Convert.ToInt32(row["TotalRecords"].ToString());
                        }
                        reader.Dispose();                        
                    }
                }
                DevPlan.data = userdevplanList;
                return DevPlan;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<DevPlanCoursesList>> GetDevPlanCourses(int DevPlanId, int UserId)
        {

            IQueryable<DevPlanCoursesList> Query = (from coursemapping in this._db.CourseMappingToDevelopment
                                                          join course in _db.Course on coursemapping.CourseId equals course.Id 
                                                          

                                                          orderby coursemapping.sequenceNo descending // , coursemapping.Id ascending
                                                          where  course.IsDeleted == false && coursemapping.IsDeleted==false && coursemapping.DevelopmentPlanId== DevPlanId
                                                    select new DevPlanCoursesList
                                                          {
                                                              CourseId = course.Id,
                                                              Title = course.Title,
                                                              Description = course.Description,
                                                              ThumbnailPath =course.ThumbnailPath,
                                                              Status =String.IsNullOrEmpty (_db.CourseCompletionStatus.Where(x => x.CourseId == course.Id && x.UserId == UserId).Select(x=>x.Status).FirstOrDefault())?"notstarted": _db.CourseCompletionStatus.Where(x => x.CourseId == course.Id && x.UserId == UserId).Select(x => x.Status).FirstOrDefault(),
                                                              IsExternalProvider = course.IsExternalProvider,
                                                              CourseURL = course.CourseURL,
                                                        sequenceNo=coursemapping.sequenceNo
                                                    }).AsNoTracking();

            
            List<DevPlanCoursesList> devPlanCoursesList = await Query.ToListAsync();

          
            return devPlanCoursesList;

        }

        public async Task<string> GetDevPlanNam(int? id)
        {
            return await (from c in _db.DevelopmentPlanForCourse
                          where c.IsDeleted == false && c.Id == id
                          select c.DevelopmentName).FirstOrDefaultAsync();
        }

        public async Task<bool> GetDevPlanForSequence(int devplanID)
        {
            bool flag = true;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "GetDevPlanStatusForSequence";
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.Add(new SqlParameter("@devplanID", SqlDbType.Int) { Value = devplanID });

                    await dbContext.Database.OpenConnectionAsync();
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count <= 0)
                    {
                        reader.Dispose();
                        return flag;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        flag = Convert.ToBoolean(row["DevPlanOverallStatus"]);
                    }
                    reader.Dispose();
                }
                return flag;
            }
            return flag;
        }
        public async Task<DevelopmentPlanDetails> IsdevPlanCompleted(int id,int userid )
        {
            DevelopmentPlanDetails data = new DevelopmentPlanDetails();
            bool flag = false;
            int count = await _db.CourseMappingToDevelopment.Where(a => a.DevelopmentPlanId == id).CountAsync();
            int CompletedCourseCount =await ( from coursemapping in this._db.CourseMappingToDevelopment
                                      join ccs in _db.CourseCompletionStatus on coursemapping.CourseId equals ccs.CourseId
                                      where ccs.UserId== userid && ccs.Status=="completed" && coursemapping.DevelopmentPlanId==id
                                              select new
                                        { 
                                        ccs.Id
                                        }).CountAsync();

            if (CompletedCourseCount == count)
            {
                data.DevelopmentPlanCompleted = true;

                var cdate = await (from coursemapping in this._db.CourseMappingToDevelopment
                                        join ccs in _db.CourseCompletionStatus on coursemapping.CourseId equals ccs.CourseId
                                        where ccs.UserId == userid && ccs.Status == "completed" && coursemapping.DevelopmentPlanId == id
                                   orderby ccs.Id descending
                                        select new
                                        {
                                            ccs.ModifiedDate
                                        }).FirstOrDefaultAsync();
                data.cdate = cdate.ModifiedDate.ToString("dd MMMM yyyy");
            }
            return data;
        }

        public async Task<List<APICourseDTO>> GetDevelopementPlan(string search = null)
        {
            var courses = (_db.DevelopmentPlanForCourse
                           .OrderBy(c => c.DevelopmentName)
                           .Where(c => c.IsDeleted == false &&
                           (search == null || c.DevelopmentName.StartsWith(search) || c.DevelopmentCode.StartsWith(search)))                          
                           .GroupBy(g => new { g.Id, g.DevelopmentName })
                          .Select(s => new APICourseDTO
                          {

                              Id = s.Max(f => f.Id),
                              Title = s.Max(a => a.DevelopmentName)
                          }));


            return await courses.ToListAsync();
        }

        


    }
}
