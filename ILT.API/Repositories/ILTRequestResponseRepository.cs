using AutoMapper;
using ILT.API.Helper;
using ILT.API.Model.ILT;
using ILT.API.APIModel;
using ILT.API.Model;
using ILT.API.Repositories.Interfaces;
using ILT.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ILT.API.Common;
using log4net;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using Google.Apis.Calendar.v3.Data;
using ILT.API.Models;

namespace ILT.API.Repositories
{
    public class ILTRequestResponseRepository : Repository<ILTRequestResponse>, IILTRequestResponse
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTRequestResponseRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        INotification _notification;
        IIdentityService _identitySv;
        IEmail _email;
        private IConfiguration _configuration;
        IOnlineWebinarRepository _onlineWebinarRepository;
        public ILTRequestResponseRepository(CourseContext context, IIdentityService identitySv, IEmail email
                                      , INotification notification,
            IOnlineWebinarRepository onlineWebinarRepository,
        ICustomerConnectionStringRepository customerConnection,
            IConfiguration configuration) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
            this._notification = notification;
            this._identitySv = identitySv;
            this._email = email;
            this._configuration = configuration;
            this._onlineWebinarRepository = onlineWebinarRepository;
        }
        public async Task<List<APIILTRequestResponse>> GetRequest(int UserId, string RoleCode, int page, int pageSize, string searchParameter = null, string searchText = null)
        {
            List<APIILTRequestResponse> ILTRequestResponseList = new List<APIILTRequestResponse>();
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
                            cmd.CommandText = "GetAllRequest";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = RoleCode });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@SearchParameter", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APIILTRequestResponse ILTRequestResponse = new APIILTRequestResponse();
                                ILTRequestResponse.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                ILTRequestResponse.ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString());
                                ILTRequestResponse.ScheduleCode = row["ScheduleCode"].ToString();
                                ILTRequestResponse.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                ILTRequestResponse.ModuleName = row["Name"].ToString();
                                ILTRequestResponse.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                ILTRequestResponse.CourseName = row["Title"].ToString();
                                ILTRequestResponse.UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString());
                                ILTRequestResponse.UserName = row["UserName"].ToString();
                                ILTRequestResponse.TrainingRequesStatus = row["TrainingRequestStatus"].ToString();
                                ILTRequestResponse.EmailId = Security.EncryptForUI(Security.Decrypt(row["EmailId"].ToString()));
                                if (!DBNull.Value.Equals(row["IsExpired"]))
                                    ILTRequestResponse.IsExpired = Convert.ToBoolean(row["IsExpired"]);
                                ILTRequestResponseList.Add(ILTRequestResponse);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ILTRequestResponseList;
        }

        public async Task<int> GetRequestedUserCount(int UserId, string RoleCode, string searchParameter = null, string searchText = null)
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
                            cmd.CommandText = "GetAllRequestedUserCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = RoleCode });
                            cmd.Parameters.Add(new SqlParameter("@SearchParameter", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

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
                                Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
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
            return Count;
        }

        public async Task<APIILTRequestRsponse> GetAllRequestDetails(int moduleId, int courseId, int userId)
        {
            List<APIILTRequest> ILTRequestResponseList = new List<APIILTRequest>();
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
                            cmd.CommandText = "GetAllScheduleDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });

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
                                APIILTRequest ILTRequestResponse = new APIILTRequest();

                                ILTRequestResponse.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                ILTRequestResponse.ModuleID = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                ILTRequestResponse.ModuleName = row["Name"].ToString();
                                ILTRequestResponse.ModuleDescription = row["Description"].ToString();
                                ILTRequestResponse.StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString());
                                ILTRequestResponse.EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["EndDate"].ToString());
                                ILTRequestResponse.StartTime = string.IsNullOrEmpty(row["StartTime"].ToString()) ? null : row["StartTime"].ToString();
                                ILTRequestResponse.EndTime = string.IsNullOrEmpty(row["EndTime"].ToString()) ? null : row["EndTime"].ToString();
                                ILTRequestResponse.RegistrationEndDate = string.IsNullOrEmpty(row["RegistrationEndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                ILTRequestResponse.ScheduleCode = row["ScheduleCode"].ToString();
                                ILTRequestResponse.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                ILTRequestResponse.PlaceName = row["PlaceName"].ToString();
                                ILTRequestResponse.TrainerType = row["TrainerType"].ToString();
                                ILTRequestResponse.TrainingRequesStatus = row["TrainingRequestStatus"].ToString();
                                ILTRequestResponse.AcademyAgencyID = string.IsNullOrEmpty(row["AcademyAgencyID"].ToString()) ? 0 : int.Parse(row["AcademyAgencyID"].ToString());
                                ILTRequestResponse.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                ILTRequestResponse.AcademyTrainerID = string.IsNullOrEmpty(row["AcademyTrainerID"].ToString()) ? 0 : int.Parse(row["AcademyTrainerID"].ToString());
                                ILTRequestResponse.AcademyTrainerName = row["AcademyTrainerName"].ToString();
                                ILTRequestResponse.TrainerDescription = row["TrainerDescription"].ToString();
                                ILTRequestResponse.ScheduleType = row["ScheduleType"].ToString();
                                ILTRequestResponse.Purpose = row["Purpose"].ToString();
                                ILTRequestResponse.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                ILTRequestResponse.CourseName = row["Title"].ToString();
                                ILTRequestResponse.AgencyTrainerName = row["AgencyTrainerName"].ToString();
                                ILTRequestResponse.City = row["Cityname"].ToString();
                                ILTRequestResponse.SeatCapacity = row["SeatCapacity"].ToString();
                                ILTRequestResponse.ContactNumber = row["ContactNumber"].ToString();
                                ILTRequestResponse.postalAddress = row["PostalAddress"].ToString();
                                ILTRequestResponse.ContactPersonName = row["ContactPerson"].ToString();
                                ILTRequestResponse.PlaceType = row["PlaceType"].ToString();
                                ILTRequestResponse.EventLogo = row["EventLogo"].ToString();
                                ILTRequestResponse.OverallStatus = row["OverallStatus"].ToString();
                                ILTRequestResponse.Currency = row["Currency"].ToString();
                                ILTRequestResponse.Cost = row["CourseFee"].ToString();
                                if (!DBNull.Value.Equals(row["IsTrainer"]))
                                    ILTRequestResponse.IsTrainer = Convert.ToBoolean(row["IsTrainer"]);
                                ILTRequestResponse.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : int.Parse(row["BatchId"].ToString());
                                ILTRequestResponse.BatchCode = Convert.ToString(row["BatchCode"]);
                                ILTRequestResponse.BatchName = Convert.ToString(row["BatchName"]);
                                ILTRequestResponseList.Add(ILTRequestResponse);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }

                APIILTRequestRsponse aPIILTRequestsList = new APIILTRequestRsponse();
                aPIILTRequestsList = ILTRequestResponseList.GroupBy(a => a.ModuleID).Select(a => a.FirstOrDefault()).Select(r => new APIILTRequestRsponse
                {
                    ModuleID = r.ModuleID,
                    ModuleName = r.ModuleName,
                    ModuleDescription = r.ModuleDescription,
                    OverallStatus = r.OverallStatus
                }).FirstOrDefault();

                List<APIILTRequestRsponse> rsponsesList = new List<APIILTRequestRsponse>();
                rsponsesList.Add(aPIILTRequestsList);

                foreach (APIILTRequestRsponse details in rsponsesList)
                {
                    aPIILTRequestsList.TopicList = (from TopicMaster in this._db.TopicMaster
                                                    join ModuleTopicAssociation in this._db.ModuleTopicAssociation on TopicMaster.ID equals ModuleTopicAssociation.TopicId
                                                    where ModuleTopicAssociation.ModuleId == moduleId && ModuleTopicAssociation.IsActive == true && ModuleTopicAssociation.IsDeleted == Record.NotDeleted
                                                    select new APIModel.APITopicList
                                                    {
                                                        TopicId = ModuleTopicAssociation.TopicId,
                                                        TopicName = TopicMaster.TopicName
                                                    }).ToArray();
                }

                aPIILTRequestsList.APIRequestScheduleDetails =
                    ILTRequestResponseList.Where(a => a.ModuleID == aPIILTRequestsList.ModuleID && (
                    (a.TrainingRequesStatus.ToLower() == "rejected")
                    || (a.TrainingRequesStatus.ToLower() == "requested")
                    || (a.TrainingRequesStatus.ToLower() == "waiting")
                    || (a.TrainingRequesStatus.ToLower() == "expired")
                    || (a.TrainingRequesStatus == "")) && a.IsTrainer == false)
                    .Select(ILTSchedule => new APIRequestScheduleDetails
                    {
                        ID = ILTSchedule.ID,
                        StartDate = ILTSchedule.StartDate,
                        EndDate = ILTSchedule.EndDate,
                        StartTime = ILTSchedule.StartTime,
                        EndTime = ILTSchedule.EndTime,
                        RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                        ScheduleCode = ILTSchedule.ScheduleCode,
                        PlaceID = ILTSchedule.PlaceID,
                        PlaceName = ILTSchedule.PlaceName,
                        TrainerType = ILTSchedule.TrainerType,
                        TrainingRequesStatus = ILTSchedule.TrainingRequesStatus, //.ToLower() == "rejected" ? "" : ILTSchedule.TrainingRequesStatus,commented to skip resend schedule request after rejection  
                        AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                        AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                        AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                        AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                        TrainerDescription = ILTSchedule.TrainerDescription,
                        ScheduleType = ILTSchedule.ScheduleType,
                        Purpose = ILTSchedule.Purpose,
                        CourseID = ILTSchedule.CourseID,
                        CourseName = ILTSchedule.CourseName,
                        AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                        City = ILTSchedule.City,
                        SeatCapacity = ILTSchedule.SeatCapacity,
                        ContactNumber = ILTSchedule.ContactNumber,
                        postalAddress = ILTSchedule.postalAddress,
                        ContactPersonName = ILTSchedule.ContactPersonName,
                        PlaceType = ILTSchedule.PlaceType,
                        EventLogo = ILTSchedule.EventLogo,
                        Currency = ILTSchedule.Currency,
                        Cost = ILTSchedule.Cost,
                        IsTrainer = ILTSchedule.IsTrainer,
                        BatchId = ILTSchedule.BatchId,
                        BatchCode = ILTSchedule.BatchCode,
                        BatchName = ILTSchedule.BatchName
                    }).ToList();

                aPIILTRequestsList.APIRequestScheduleDetailsForRegistered =
               ILTRequestResponseList.Where(i => (i.TrainingRequesStatus.ToLower().Equals("registered")
                                            || i.TrainingRequesStatus.ToLower().Equals("availability")
                                            || i.TrainingRequesStatus.ToLower().Equals("unavailability")
                                            || i.TrainingRequesStatus.ToLower().Equals("approved")) && i.IsTrainer == false)
                                            .Select(ILTSchedule => new APIRequestScheduleDetails
                                            {
                                                ID = ILTSchedule.ID,
                                                StartDate = ILTSchedule.StartDate,
                                                EndDate = ILTSchedule.EndDate,
                                                StartTime = ILTSchedule.StartTime,
                                                EndTime = ILTSchedule.EndTime,
                                                RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                                                ScheduleCode = ILTSchedule.ScheduleCode,
                                                PlaceID = ILTSchedule.PlaceID,
                                                PlaceName = ILTSchedule.PlaceName,
                                                TrainerType = ILTSchedule.TrainerType,
                                                TrainingRequesStatus = ILTSchedule.TrainingRequesStatus,
                                                AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                                                AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                                                AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                                                AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                                                TrainerDescription = ILTSchedule.TrainerDescription,
                                                ScheduleType = ILTSchedule.ScheduleType,
                                                Purpose = ILTSchedule.Purpose,
                                                CourseID = ILTSchedule.CourseID,
                                                CourseName = ILTSchedule.CourseName,
                                                AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                                                City = ILTSchedule.City,
                                                SeatCapacity = ILTSchedule.SeatCapacity,
                                                ContactNumber = ILTSchedule.ContactNumber,
                                                postalAddress = ILTSchedule.postalAddress,
                                                ContactPersonName = ILTSchedule.ContactPersonName,
                                                PlaceType = ILTSchedule.PlaceType,
                                                EventLogo = ILTSchedule.EventLogo,
                                                Currency = ILTSchedule.Currency,
                                                Cost = ILTSchedule.Cost,
                                                IsTrainer = ILTSchedule.IsTrainer,
                                                BatchId = ILTSchedule.BatchId,
                                                BatchCode = ILTSchedule.BatchCode,
                                                BatchName = ILTSchedule.BatchName
                                            }).FirstOrDefault();

                var IsNominated = aPIILTRequestsList.APIRequestScheduleDetails.Where(a => (a.TrainingRequesStatus.ToLower() == "registered" || (a.TrainingRequesStatus.ToLower() == "rejected"))).FirstOrDefault();
                if (IsNominated != null)
                    aPIILTRequestsList.IsNominated = true;

                return aPIILTRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<APIILTRequestRsponse> GetWNSAllRequestDetails(int moduleId, int courseId, int userId)
        {
            List<APIILTRequest> ILTRequestResponseList = new List<APIILTRequest>();
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
                            cmd.CommandText = "GetWNSAllScheduleDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });

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
                                APIILTRequest ILTRequestResponse = new APIILTRequest();

                                ILTRequestResponse.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                ILTRequestResponse.ModuleID = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                ILTRequestResponse.ModuleName = row["Name"].ToString();
                                ILTRequestResponse.ModuleDescription = row["Description"].ToString();
                                ILTRequestResponse.StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString());
                                ILTRequestResponse.EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["EndDate"].ToString());
                                ILTRequestResponse.StartTime = string.IsNullOrEmpty(row["StartTime"].ToString()) ? null : row["StartTime"].ToString();
                                ILTRequestResponse.EndTime = string.IsNullOrEmpty(row["EndTime"].ToString()) ? null : row["EndTime"].ToString();
                                ILTRequestResponse.RegistrationEndDate = string.IsNullOrEmpty(row["RegistrationEndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                ILTRequestResponse.ScheduleCode = row["ScheduleCode"].ToString();
                                ILTRequestResponse.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                ILTRequestResponse.PlaceName = row["PlaceName"].ToString();
                                ILTRequestResponse.TrainerType = row["TrainerType"].ToString();
                                ILTRequestResponse.TrainingRequesStatus = row["TrainingRequestStatus"].ToString();
                                ILTRequestResponse.AcademyAgencyID = string.IsNullOrEmpty(row["AcademyAgencyID"].ToString()) ? 0 : int.Parse(row["AcademyAgencyID"].ToString());
                                ILTRequestResponse.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                ILTRequestResponse.AcademyTrainerID = string.IsNullOrEmpty(row["AcademyTrainerID"].ToString()) ? 0 : int.Parse(row["AcademyTrainerID"].ToString());
                                ILTRequestResponse.AcademyTrainerName = row["AcademyTrainerName"].ToString();
                                ILTRequestResponse.TrainerDescription = row["TrainerDescription"].ToString();
                                ILTRequestResponse.ScheduleType = row["ScheduleType"].ToString();
                                ILTRequestResponse.Purpose = row["Purpose"].ToString();
                                ILTRequestResponse.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                ILTRequestResponse.CourseName = row["Title"].ToString();
                                ILTRequestResponse.AgencyTrainerName = row["AgencyTrainerName"].ToString();
                                ILTRequestResponse.City = row["Cityname"].ToString();
                                ILTRequestResponse.SeatCapacity = row["SeatCapacity"].ToString();
                                ILTRequestResponse.ContactNumber = row["ContactNumber"].ToString();
                                ILTRequestResponse.postalAddress = row["PostalAddress"].ToString();
                                ILTRequestResponse.ContactPersonName = row["ContactPerson"].ToString();
                                ILTRequestResponse.PlaceType = row["PlaceType"].ToString();
                                ILTRequestResponse.EventLogo = row["EventLogo"].ToString();
                                ILTRequestResponse.OverallStatus = row["OverallStatus"].ToString();
                                ILTRequestResponse.Currency = row["Currency"].ToString();
                                ILTRequestResponse.Cost = row["CourseFee"].ToString();
                                if (!DBNull.Value.Equals(row["IsTrainer"]))
                                    ILTRequestResponse.IsTrainer = Convert.ToBoolean(row["IsTrainer"]);
                                ILTRequestResponse.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : int.Parse(row["BatchId"].ToString());
                                ILTRequestResponse.BatchCode = Convert.ToString(row["BatchCode"]);
                                ILTRequestResponse.BatchName = Convert.ToString(row["BatchName"]);
                                if (!DBNull.Value.Equals(row["RequestApproval"]))
                                    ILTRequestResponse.RequestApproval = Convert.ToBoolean(row["RequestApproval"]);
                                ILTRequestResponseList.Add(ILTRequestResponse);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }

                APIILTRequestRsponse aPIILTRequestsList = new APIILTRequestRsponse();
                aPIILTRequestsList = ILTRequestResponseList.GroupBy(a => a.ModuleID).Select(a => a.FirstOrDefault()).Select(r => new APIILTRequestRsponse
                {
                    ModuleID = r.ModuleID,
                    ModuleName = r.ModuleName,
                    ModuleDescription = r.ModuleDescription,
                    OverallStatus = r.OverallStatus
                }).FirstOrDefault();

                List<APIILTRequestRsponse> rsponsesList = new List<APIILTRequestRsponse>();
                rsponsesList.Add(aPIILTRequestsList);

                foreach (APIILTRequestRsponse details in rsponsesList)
                {
                    aPIILTRequestsList.TopicList = (from TopicMaster in this._db.TopicMaster
                                                    join ModuleTopicAssociation in this._db.ModuleTopicAssociation on TopicMaster.ID equals ModuleTopicAssociation.TopicId
                                                    where ModuleTopicAssociation.ModuleId == moduleId && ModuleTopicAssociation.IsActive == true && ModuleTopicAssociation.IsDeleted == Record.NotDeleted
                                                    select new APIModel.APITopicList
                                                    {
                                                        TopicId = ModuleTopicAssociation.TopicId,
                                                        TopicName = TopicMaster.TopicName
                                                    }).ToArray();
                }

                aPIILTRequestsList.APIRequestScheduleDetails =
                    ILTRequestResponseList.Where(a => a.ModuleID == aPIILTRequestsList.ModuleID && (a.IsTrainer == false))
                    .Select(ILTSchedule => new APIRequestScheduleDetails
                    {
                        ID = ILTSchedule.ID,
                        StartDate = ILTSchedule.StartDate,
                        EndDate = ILTSchedule.EndDate,
                        StartTime = ILTSchedule.StartTime,
                        EndTime = ILTSchedule.EndTime,
                        RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                        ScheduleCode = ILTSchedule.ScheduleCode,
                        PlaceID = ILTSchedule.PlaceID,
                        PlaceName = ILTSchedule.PlaceName,
                        TrainerType = ILTSchedule.TrainerType,
                        // TrainingRequesStatus = ILTSchedule.TrainingRequesStatus.ToLower() == "rejected" ? "" : ILTSchedule.TrainingRequesStatus,
                        TrainingRequesStatus = ILTSchedule.TrainingRequesStatus,
                        AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                        AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                        AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                        AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                        TrainerDescription = ILTSchedule.TrainerDescription,
                        ScheduleType = ILTSchedule.ScheduleType,
                        Purpose = ILTSchedule.Purpose,
                        CourseID = ILTSchedule.CourseID,
                        CourseName = ILTSchedule.CourseName,
                        AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                        City = ILTSchedule.City,
                        SeatCapacity = ILTSchedule.SeatCapacity,
                        ContactNumber = ILTSchedule.ContactNumber,
                        postalAddress = ILTSchedule.postalAddress,
                        ContactPersonName = ILTSchedule.ContactPersonName,
                        PlaceType = ILTSchedule.PlaceType,
                        EventLogo = ILTSchedule.EventLogo,
                        Currency = ILTSchedule.Currency,
                        Cost = ILTSchedule.Cost,
                        IsTrainer = ILTSchedule.IsTrainer,
                        BatchId = ILTSchedule.BatchId,
                        BatchCode = ILTSchedule.BatchCode,
                        BatchName = ILTSchedule.BatchName,
                        OverallStatus = ILTSchedule.OverallStatus,
                        RequestApproval = ILTSchedule.RequestApproval
                    }).ToList();

                aPIILTRequestsList.APIRequestScheduleDetailsForRegistered =
               ILTRequestResponseList.Where(i => (i.TrainingRequesStatus.ToLower().Equals("registered")
                                            || i.TrainingRequesStatus.ToLower().Equals("availability")
                                            || i.TrainingRequesStatus.ToLower().Equals("rejected")
                                            || i.TrainingRequesStatus.ToLower().Equals("approved")) && i.IsTrainer == false)
                                            .Select(ILTSchedule => new APIRequestScheduleDetails
                                            {
                                                ID = ILTSchedule.ID,
                                                StartDate = ILTSchedule.StartDate,
                                                EndDate = ILTSchedule.EndDate,
                                                StartTime = ILTSchedule.StartTime,
                                                EndTime = ILTSchedule.EndTime,
                                                RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                                                ScheduleCode = ILTSchedule.ScheduleCode,
                                                PlaceID = ILTSchedule.PlaceID,
                                                PlaceName = ILTSchedule.PlaceName,
                                                TrainerType = ILTSchedule.TrainerType,
                                                TrainingRequesStatus = ILTSchedule.TrainingRequesStatus,
                                                AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                                                AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                                                AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                                                AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                                                TrainerDescription = ILTSchedule.TrainerDescription,
                                                ScheduleType = ILTSchedule.ScheduleType,
                                                Purpose = ILTSchedule.Purpose,
                                                CourseID = ILTSchedule.CourseID,
                                                CourseName = ILTSchedule.CourseName,
                                                AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                                                City = ILTSchedule.City,
                                                SeatCapacity = ILTSchedule.SeatCapacity,
                                                ContactNumber = ILTSchedule.ContactNumber,
                                                postalAddress = ILTSchedule.postalAddress,
                                                ContactPersonName = ILTSchedule.ContactPersonName,
                                                PlaceType = ILTSchedule.PlaceType,
                                                EventLogo = ILTSchedule.EventLogo,
                                                Currency = ILTSchedule.Currency,
                                                Cost = ILTSchedule.Cost,
                                                IsTrainer = ILTSchedule.IsTrainer,
                                                BatchId = ILTSchedule.BatchId,
                                                BatchCode = ILTSchedule.BatchCode,
                                                BatchName = ILTSchedule.BatchName,
                                                OverallStatus = ILTSchedule.OverallStatus,
                                                RequestApproval = ILTSchedule.RequestApproval
                                            }).FirstOrDefault();

                var IsNominated = aPIILTRequestsList.APIRequestScheduleDetails.Where(a => (a.TrainingRequesStatus.ToLower() == "registered" || (a.TrainingRequesStatus.ToLower() == "rejected"))).FirstOrDefault();
                if (IsNominated != null)
                    aPIILTRequestsList.IsNominated = true;

                return aPIILTRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<List<APIILTBatchResponse>> GetBatchesForRequest(int CourseId, int UserId)
        {
            List<APIILTBatchPreResponse> aPIILTBatchPreResponseList = new List<APIILTBatchPreResponse>();
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
                            cmd.CommandText = "[dbo].[GetAllBatchesForRequest]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = UserId });

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
                                APIILTBatchPreResponse aPIILTBatchPreResponse = new APIILTBatchPreResponse();

                                aPIILTBatchPreResponse.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : int.Parse(row["BatchId"].ToString());
                                aPIILTBatchPreResponse.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                aPIILTBatchPreResponse.BatchCode = row["BatchCode"].ToString();
                                aPIILTBatchPreResponse.BatchName = row["BatchName"].ToString();
                                aPIILTBatchPreResponse.CourseCode = row["CourseCode"].ToString();
                                aPIILTBatchPreResponse.CourseName = row["CourseName"].ToString();
                                aPIILTBatchPreResponse.StartDate = Convert.ToDateTime(row["StartDate"].ToString());
                                aPIILTBatchPreResponse.EndDate = Convert.ToDateTime(row["EndDate"].ToString());
                                aPIILTBatchPreResponse.StartTime = TimeSpan.Parse(row["StartTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTBatchPreResponse.EndTime = TimeSpan.Parse(row["EndTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTBatchPreResponse.Description = row["Description"].ToString();
                                aPIILTBatchPreResponse.RegionName = row["RegionName"].ToString();

                                aPIILTBatchPreResponse.ScheduleId = string.IsNullOrEmpty(row["ScheduleId"].ToString()) ? 0 : int.Parse(row["ScheduleId"].ToString());
                                aPIILTBatchPreResponse.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                aPIILTBatchPreResponse.ModuleName = row["ModuleName"].ToString();
                                aPIILTBatchPreResponse.ScheduleCode = row["ScheduleCode"].ToString();
                                aPIILTBatchPreResponse.ScheduleStartDate = Convert.ToDateTime(row["ScheduleStartDate"].ToString());
                                aPIILTBatchPreResponse.ScheduleEndDate = Convert.ToDateTime(row["ScheduleEndDate"].ToString());
                                aPIILTBatchPreResponse.ScheduleRegistrationEndDate = Convert.ToDateTime(row["ScheduleRegistrationEndDate"].ToString());
                                aPIILTBatchPreResponse.ScheduleStartTime = TimeSpan.Parse(row["ScheduleStartTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTBatchPreResponse.ScheduleEndTime = TimeSpan.Parse(row["ScheduleEndTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTBatchPreResponse.ScheduleType = row["ScheduleType"].ToString();
                                aPIILTBatchPreResponse.TrainingRequestStatus = row["TrainingRequestStatus"].ToString();
                                aPIILTBatchPreResponse.NominationStatus = row["NominationStatus"].ToString();
                                aPIILTBatchPreResponse.AttendanceStatus = row["AttendanceStatus"].ToString();
                                aPIILTBatchPreResponse.ScheduleType = row["ScheduleType"].ToString();
                                aPIILTBatchPreResponse.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                aPIILTBatchPreResponse.PlaceName = row["PlaceName"].ToString();
                                aPIILTBatchPreResponse.SeatCapacity = string.IsNullOrEmpty(row["SeatCapacity"].ToString()) ? 0 : int.Parse(row["SeatCapacity"].ToString());
                                aPIILTBatchPreResponse.PlaceType = row["PlaceType"].ToString();
                                aPIILTBatchPreResponse.ContactPerson = row["ContactPerson"].ToString();
                                aPIILTBatchPreResponse.ContactNumber = row["ContactNumber"].ToString();
                                aPIILTBatchPreResponse.CityName = row["CityName"].ToString();
                                aPIILTBatchPreResponse.PostalAddress = row["PostalAddress"].ToString();
                                aPIILTBatchPreResponse.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                aPIILTBatchPreResponse.IsTrainer = bool.Parse(row["IsTrainer"].ToString());
                                aPIILTBatchPreResponseList.Add(aPIILTBatchPreResponse);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }

                List<APIILTBatchResponse> aPIILTBatchResponseList = new List<APIILTBatchResponse>();
                if (aPIILTBatchPreResponseList.Where(x => x.IsTrainer == true).Count() > 0)
                    return aPIILTBatchResponseList;

                aPIILTBatchResponseList = aPIILTBatchPreResponseList.GroupBy(g => new
                {
                    g.BatchId,
                    g.CourseId,
                    g.BatchCode,
                    g.BatchName,
                    g.CourseCode,
                    g.CourseName,
                    g.StartDate,
                    g.EndDate,
                    g.StartTime,
                    g.EndTime,
                    g.Description,
                    g.RegionName
                }).Select(batch => batch.FirstOrDefault()).Select(batch => new APIILTBatchResponse
                {
                    BatchId = batch.BatchId,
                    CourseId = batch.CourseId,
                    BatchCode = batch.BatchCode,
                    BatchName = batch.BatchName,
                    CourseCode = batch.CourseCode,
                    CourseName = batch.CourseName,
                    StartDate = batch.StartDate,
                    EndDate = batch.EndDate,
                    StartTime = batch.StartTime,
                    EndTime = batch.EndTime,
                    Description = batch.Description,
                    RegionName = batch.RegionName
                }).ToList();

                foreach (APIILTBatchResponse item in aPIILTBatchResponseList)
                {
                    List<APIScheduleDetails> APIScheduleDetailsList = new List<APIScheduleDetails>();
                    APIScheduleDetailsList = aPIILTBatchPreResponseList.Where(x => x.CourseId == item.CourseId && x.BatchId == item.BatchId)
                                            .Select(schedule => new APIScheduleDetails
                                            {
                                                ScheduleId = schedule.ScheduleId,
                                                ModuleId = schedule.ModuleId,
                                                CourseCode = schedule.CourseCode,
                                                CourseName = schedule.CourseName,
                                                ModuleName = schedule.ModuleName,
                                                ScheduleCode = schedule.ScheduleCode,
                                                ScheduleStartDate = schedule.ScheduleStartDate,
                                                ScheduleEndDate = schedule.ScheduleEndDate,
                                                ScheduleRegistrationEndDate = schedule.ScheduleRegistrationEndDate,
                                                ScheduleStartTime = schedule.ScheduleStartTime,
                                                ScheduleEndTime = schedule.ScheduleEndTime,
                                                ScheduleType = schedule.ScheduleType,
                                                TrainingRequestStatus = schedule.TrainingRequestStatus,
                                                NominationStatus = schedule.NominationStatus,
                                                AttendanceStatus = schedule.AttendanceStatus,
                                                PlaceID = schedule.PlaceID,
                                                PlaceName = schedule.PlaceName,
                                                SeatCapacity = schedule.SeatCapacity,
                                                PlaceType = schedule.PlaceType,
                                                ContactPerson = schedule.ContactPerson,
                                                ContactNumber = schedule.ContactNumber,
                                                CityName = schedule.CityName,
                                                PostalAddress = schedule.PostalAddress,
                                                AcademyAgencyID = schedule.AcademyAgencyID,
                                                AcademyAgencyName = schedule.AcademyAgencyName
                                            }).ToList();
                    item.BatchAttendanceStatus = APIScheduleDetailsList.Where(x => string.IsNullOrEmpty(x.AttendanceStatus)).Count() == APIScheduleDetailsList.Count() ? null : (APIScheduleDetailsList.Where(x => x.AttendanceStatus == "Completed").Count() == APIScheduleDetailsList.Count()) ? "Completed" : "Inprogress";
                    item.BatchNominationStatus = APIScheduleDetailsList.Where(x => string.IsNullOrEmpty(x.NominationStatus)).Count() > 0 ? null : "Nominated";
                    item.BatchRequestStatus = APIScheduleDetailsList.Where(x => string.IsNullOrEmpty(x.TrainingRequestStatus)).Count() == APIScheduleDetailsList.Count() ? null : (APIScheduleDetailsList.Where(x => x.TrainingRequestStatus == "Requested").Count() == APIScheduleDetailsList.Count()) ? "Requested" : (APIScheduleDetailsList.Where(x => x.TrainingRequestStatus == "Waiting").Count() == APIScheduleDetailsList.Count()) ? "Waiting" : (APIScheduleDetailsList.Where(x => x.TrainingRequestStatus == "Expired").Count() == APIScheduleDetailsList.Count()) ? "Expired" : (APIScheduleDetailsList.Where(x => x.TrainingRequestStatus == "Rejected").Count() == APIScheduleDetailsList.Count()) ? null : "Registered";
                    item.APIScheduleDetailsList = APIScheduleDetailsList;
                }

                if (aPIILTBatchResponseList.Where(x => x.BatchRequestStatus == "Requested" || x.BatchNominationStatus == "Nominated").Count() > 0)
                    aPIILTBatchResponseList = aPIILTBatchResponseList.Where(x => x.BatchRequestStatus == "Requested" || x.BatchNominationStatus == "Nominated").ToList();

                return aPIILTBatchResponseList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<APIILTRequestRsponse> GetRequestDetails(int moduleId, int courseId, int userId)
        {
            List<APIILTRequest> ILTRequestResponseList = new List<APIILTRequest>();
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
                            cmd.CommandText = "GetRequestedScheduleDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });

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
                                APIILTRequest ILTRequestResponse = new APIILTRequest();

                                ILTRequestResponse.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                ILTRequestResponse.ModuleID = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                ILTRequestResponse.ModuleName = row["Name"].ToString();
                                ILTRequestResponse.ModuleDescription = row["Description"].ToString();
                                ILTRequestResponse.StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString());
                                ILTRequestResponse.EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["EndDate"].ToString());
                                ILTRequestResponse.StartTime = string.IsNullOrEmpty(row["StartTime"].ToString()) ? null : row["StartTime"].ToString();
                                ILTRequestResponse.EndTime = string.IsNullOrEmpty(row["EndTime"].ToString()) ? null : row["EndTime"].ToString();
                                ILTRequestResponse.RegistrationEndDate = string.IsNullOrEmpty(row["RegistrationEndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                ILTRequestResponse.ScheduleCode = row["ScheduleCode"].ToString();
                                ILTRequestResponse.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                ILTRequestResponse.PlaceName = row["PlaceName"].ToString();
                                ILTRequestResponse.TrainerType = row["TrainerType"].ToString();
                                ILTRequestResponse.TrainingRequesStatus = row["TrainingRequestStatus"].ToString();
                                ILTRequestResponse.AcademyAgencyID = string.IsNullOrEmpty(row["AcademyAgencyID"].ToString()) ? 0 : int.Parse(row["AcademyAgencyID"].ToString());
                                ILTRequestResponse.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                ILTRequestResponse.AcademyTrainerID = string.IsNullOrEmpty(row["AcademyTrainerID"].ToString()) ? 0 : int.Parse(row["AcademyTrainerID"].ToString());
                                ILTRequestResponse.AcademyTrainerName = row["AcademyTrainerName"].ToString();
                                ILTRequestResponse.TrainerDescription = row["TrainerDescription"].ToString();
                                ILTRequestResponse.ScheduleType = row["ScheduleType"].ToString();
                                ILTRequestResponse.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                ILTRequestResponse.CourseName = row["Title"].ToString();
                                ILTRequestResponse.AgencyTrainerName = row["AgencyTrainerName"].ToString();
                                ILTRequestResponse.City = row["Cityname"].ToString();
                                ILTRequestResponse.SeatCapacity = row["SeatCapacity"].ToString();
                                ILTRequestResponse.ContactNumber = row["ContactNumber"].ToString();
                                ILTRequestResponse.postalAddress = row["PostalAddress"].ToString();
                                ILTRequestResponse.ContactPersonName = row["ContactPerson"].ToString();
                                ILTRequestResponse.PlaceType = row["PlaceType"].ToString();
                                ILTRequestResponse.EventLogo = row["EventLogo"].ToString();

                                ILTRequestResponseList.Add(ILTRequestResponse);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }

                APIILTRequestRsponse aPIILTRequestsList = new APIILTRequestRsponse();
                aPIILTRequestsList = ILTRequestResponseList.GroupBy(a => a.ModuleID).Select(a => a.FirstOrDefault()).Select(r => new APIILTRequestRsponse
                {
                    ModuleID = r.ModuleID,
                    ModuleName = r.ModuleName,
                    ModuleDescription = r.ModuleDescription
                }).FirstOrDefault();

                aPIILTRequestsList.APIRequestScheduleDetails =
                    ILTRequestResponseList.Where(a => a.ModuleID == aPIILTRequestsList.ModuleID && (
                    (a.TrainingRequesStatus.ToLower() == "rejected")
                    || (a.TrainingRequesStatus.ToLower() == "requested")
                    || (a.TrainingRequesStatus.ToLower() == "waiting")
                    || (a.TrainingRequesStatus == "")))
                    .Select(ILTSchedule => new APIRequestScheduleDetails
                    {
                        ID = ILTSchedule.ID,
                        StartDate = ILTSchedule.StartDate,
                        EndDate = ILTSchedule.EndDate,
                        StartTime = ILTSchedule.StartTime,
                        EndTime = ILTSchedule.EndTime,
                        RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                        ScheduleCode = ILTSchedule.ScheduleCode,
                        PlaceID = ILTSchedule.PlaceID,
                        PlaceName = ILTSchedule.PlaceName,
                        TrainerType = ILTSchedule.TrainerType,
                        TrainingRequesStatus = ILTSchedule.TrainingRequesStatus,
                        AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                        AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                        AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                        AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                        TrainerDescription = ILTSchedule.TrainerDescription,
                        ScheduleType = ILTSchedule.ScheduleType,
                        CourseID = ILTSchedule.CourseID,
                        CourseName = ILTSchedule.CourseName,
                        AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                        City = ILTSchedule.City,
                        SeatCapacity = ILTSchedule.SeatCapacity,
                        ContactNumber = ILTSchedule.ContactNumber,
                        postalAddress = ILTSchedule.postalAddress,
                        ContactPersonName = ILTSchedule.ContactPersonName,
                        PlaceType = ILTSchedule.PlaceType,
                        EventLogo = ILTSchedule.EventLogo

                    }).ToList();

                aPIILTRequestsList.APIRequestScheduleDetailsForRegistered =
               ILTRequestResponseList.Where(i => i.TrainingRequesStatus.ToLower().Equals("registered")
                                            || i.TrainingRequesStatus.ToLower().Equals("availability")
                                            || i.TrainingRequesStatus.ToLower().Equals("unavailability")
                                            || i.TrainingRequesStatus.ToLower().Equals("approved"))
                                            .Select(ILTSchedule => new APIRequestScheduleDetails
                                            {
                                                ID = ILTSchedule.ID,
                                                StartDate = ILTSchedule.StartDate,
                                                EndDate = ILTSchedule.EndDate,
                                                StartTime = ILTSchedule.StartTime,
                                                EndTime = ILTSchedule.EndTime,
                                                RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                                                ScheduleCode = ILTSchedule.ScheduleCode,
                                                PlaceID = ILTSchedule.PlaceID,
                                                PlaceName = ILTSchedule.PlaceName,
                                                TrainerType = ILTSchedule.TrainerType,
                                                TrainingRequesStatus = ILTSchedule.TrainingRequesStatus,
                                                AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                                                AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                                                AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                                                AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                                                TrainerDescription = ILTSchedule.TrainerDescription,
                                                ScheduleType = ILTSchedule.ScheduleType,
                                                CourseID = ILTSchedule.CourseID,
                                                CourseName = ILTSchedule.CourseName,
                                                AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                                                City = ILTSchedule.City,
                                                SeatCapacity = ILTSchedule.SeatCapacity,
                                                ContactNumber = ILTSchedule.ContactNumber,
                                                postalAddress = ILTSchedule.postalAddress,
                                                ContactPersonName = ILTSchedule.ContactPersonName,
                                                PlaceType = ILTSchedule.PlaceType,
                                                EventLogo = ILTSchedule.EventLogo

                                            }).FirstOrDefault();

                var IsNominated = aPIILTRequestsList.APIRequestScheduleDetails.Where(a => (a.TrainingRequesStatus.ToLower() == "registered") || (a.TrainingRequesStatus.ToLower() == "rejected")).FirstOrDefault();
                if (IsNominated != null)
                    aPIILTRequestsList.IsNominated = true;

                return aPIILTRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<string> PostDropOutRequest(APIILTRequestResponse aPIILTRequestResponse, int UserId, string OrganizationCode)
        {

            ILTRequestResponse objILTAvailable = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                        && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                        && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                        && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                        && a.UserID == UserId
                                                                                                        && a.TrainingRequesStatus == "Availability")
                                                                                                .FirstOrDefaultAsync();




            if (aPIILTRequestResponse.TrainingRequesStatus.ToLower() == "unavailability")
            {
                if (objILTAvailable == null)
                {
                    ILTRequestResponse objILTResponse = Mapper.Map<ILTRequestResponse>(aPIILTRequestResponse);
                    objILTResponse.UserID = UserId;
                    objILTResponse.CreatedBy = UserId;
                    objILTResponse.ModifiedBy = UserId;
                    objILTResponse.CreatedDate = DateTime.UtcNow;
                    objILTResponse.ModifiedDate = DateTime.UtcNow;
                    objILTResponse.IsDeleted = false;
                    objILTResponse.IsActive = true;
                    objILTResponse.BatchID = 0;
                    objILTResponse.ID = 0;
                    await this.Add(objILTResponse);
                }
                else
                {
                    objILTAvailable.TrainingRequesStatus = "Unavailability";
                    objILTAvailable.ModifiedDate = DateTime.UtcNow;
                    await this.Update(objILTAvailable);
                }

                ILTRequestResponse objILTRequestResponse = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.TrainingRequesStatus == "Waiting")
                                                                                                 .FirstOrDefaultAsync();
                if (objILTRequestResponse != null)
                {
                    ILTRequestResponse firstILTRequestResponseForWaiting = await this._db.ILTRequestResponse.AsNoTracking().Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.TrainingRequesStatus == "Waiting").OrderBy(a => a.ID)
                                                                                                 .FirstOrDefaultAsync();

                    firstILTRequestResponseForWaiting.IsActive = false;
                    firstILTRequestResponseForWaiting.IsDeleted = true;
                    await this.Update(firstILTRequestResponseForWaiting);

                    ILTRequestResponse objILTResponseRequest = new ILTRequestResponse();
                    objILTResponseRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    objILTResponseRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    objILTResponseRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    objILTResponseRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                    objILTResponseRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    objILTResponseRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                    objILTResponseRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                    objILTResponseRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                    objILTResponseRequest.TrainingRequesStatus = "Requested";
                    objILTResponseRequest.IsActive = true;
                    objILTResponseRequest.IsDeleted = false;
                    objILTResponseRequest.ID = 0;
                    await this.Add(objILTResponseRequest);

                    ILTRequestResponse objILTApprovedRequest = new ILTRequestResponse();
                    objILTApprovedRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    objILTApprovedRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    objILTApprovedRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    objILTApprovedRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                    objILTApprovedRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    objILTApprovedRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                    objILTApprovedRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                    objILTApprovedRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                    objILTApprovedRequest.TrainingRequesStatus = "Approved";
                    objILTApprovedRequest.IsActive = true;
                    objILTApprovedRequest.IsDeleted = false;
                    objILTApprovedRequest.ID = 0;
                    await this.Add(objILTApprovedRequest);

                    ILTRequestResponse objILTAvailabilityRequest = new ILTRequestResponse();
                    objILTAvailabilityRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    objILTAvailabilityRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    objILTAvailabilityRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    objILTAvailabilityRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                    objILTAvailabilityRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    objILTAvailabilityRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                    objILTAvailabilityRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                    objILTAvailabilityRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                    objILTAvailabilityRequest.TrainingRequesStatus = "Availability";
                    objILTAvailabilityRequest.IsActive = true;
                    objILTAvailabilityRequest.IsDeleted = false;
                    objILTAvailabilityRequest.ID = 0;
                    await this.Add(objILTAvailabilityRequest);


                    string Otp = null;
                    bool flag = false;

                    while (flag == false)
                    {
                        Otp = GenerateRandomPassword();
                        List<TrainingNomination> existingOTP = this._db.TrainingNomination.Where(a => a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                  && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                  && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                  && a.IsActive == true
                                                                                                  && a.IsDeleted == Record.NotDeleted
                                                                                                  && a.IsActiveNomination == true).ToList();
                        if (existingOTP != null)
                        {
                            flag = true;
                        }
                    }
                    TrainingNomination obj = new TrainingNomination();
                    TrainingNomination lastRequestCode = await _db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true).OrderByDescending(a => a.ID).FirstOrDefaultAsync();
                    if (lastRequestCode == null)
                    {
                        obj.RequestCode = "RQ1";
                    }
                    else
                    {
                        obj.RequestCode = "RQ" + (lastRequestCode.ID + 1);
                    }
                    obj.ScheduleID = aPIILTRequestResponse.ScheduleID;
                    obj.UserID = firstILTRequestResponseForWaiting.UserID;
                    obj.TrainingRequestStatus = "Registered";
                    obj.ModuleID = aPIILTRequestResponse.ModuleID;
                    obj.CourseID = aPIILTRequestResponse.CourseID;
                    obj.OTP = Otp;
                    obj.CreatedBy = UserId;
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.ModifiedBy = UserId;
                    obj.ModifiedDate = DateTime.UtcNow;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    obj.IsActiveNomination = true;
                    _db.TrainingNomination.Add(obj);
                    await _db.SaveChangesAsync();

                }

                TrainingNomination updateForUnavailability = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.UserID == UserId && a.TrainingRequestStatus == "Registered"
                                                                                                         && a.IsActiveNomination == true).OrderByDescending(a => a.ID).FirstOrDefaultAsync();
                if (updateForUnavailability != null)
                {
                    updateForUnavailability.TrainingRequestStatus = "Unregistered";
                    updateForUnavailability.ModifiedBy = UserId;
                    updateForUnavailability.ModifiedDate = DateTime.UtcNow;
                    updateForUnavailability.IsActive = true;
                    updateForUnavailability.IsDeleted = false;
                    updateForUnavailability.IsActiveNomination = false;

                    this._db.TrainingNomination.Update(updateForUnavailability);
                    this._db.SaveChanges();

                }


            }
            return "Success";
        }
        public async Task<ILTRequestResponse> GetRequestResponse(APIILTRequestResponse aPIILTRequestResponse, int UserId)
        {
            ILTRequestResponse oldRequestResponce = await this._db.ILTRequestResponse.AsNoTracking().Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                      && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                      && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                      && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                      && a.UserID == UserId
                                                                                                     ).OrderBy(a => a.ID)
                                                                                             .FirstOrDefaultAsync();
            return oldRequestResponce;
        }

        public async Task<string> PostRequest(APIILTRequestResponse aPIILTRequestResponse, int UserId, string OrganizationCode)
        {
            var IltSchedule = await this._db.ILTSchedule.Where(a => a.IsActive == true && a.IsDeleted == false && a.ID == aPIILTRequestResponse.ScheduleID)
                                                                    .Select(ilt => new { ilt.ScheduleCode, ilt.PlaceID, ilt.EndDate, ilt.EndTime, ilt.StartDate, ilt.StartTime }).FirstOrDefaultAsync();

            if (aPIILTRequestResponse.TrainingRequesStatus == "Requested")
            {
                bool IsUserBusy = false;
                bool CanNominate = false;
                string ErrorMessage = string.Empty;
                string ScheduleCode = string.Empty;
                DataTable dt1 = new DataTable();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "CheckUserIsBusyInSchedule";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@ScheduleId", SqlDbType.Int) { Value = aPIILTRequestResponse.ScheduleID });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dt1.Load(reader);

                            reader.Dispose();
                            connection.Close();
                        }
                    }
                }
                if (dt1 != null && dt1.Rows.Count > 0)
                {
                    if (!DBNull.Value.Equals(dt1.Rows[0]["IsUserBusy"]))
                    {
                        IsUserBusy = Convert.ToBoolean(dt1.Rows[0]["IsUserBusy"]);
                        ScheduleCode = Convert.ToString(dt1.Rows[0]["ScheduleCode"]);
                    }
                    if (!DBNull.Value.Equals(dt1.Rows[0]["CanNominate"]))
                        CanNominate = Convert.ToBoolean(dt1.Rows[0]["CanNominate"]);
                    if (!DBNull.Value.Equals(dt1.Rows[0]["ErrorMessage"]))
                        ErrorMessage = Convert.ToString(dt1.Rows[0]["ErrorMessage"]);
                }
                if (IsUserBusy && string.IsNullOrEmpty(ErrorMessage))
                    return "Cannot request as you are busy in another schedule " + ScheduleCode + " at this time.";
                else if (!string.Equals(OrganizationCode, "canh", StringComparison.CurrentCultureIgnoreCase)
                         && !string.Equals(OrganizationCode, "tcns", StringComparison.CurrentCultureIgnoreCase)
                         && !string.Equals(OrganizationCode, "tablez", StringComparison.CurrentCultureIgnoreCase)
                         && !string.Equals(OrganizationCode, "ujjivan", StringComparison.CurrentCultureIgnoreCase)
                         && !CanNominate && !string.IsNullOrEmpty(ErrorMessage))
                    return ErrorMessage.Replace("Cannot nominate", "Cannot request");

                if (IltSchedule.EndDate.Date < DateTime.UtcNow.Date || (IltSchedule.EndDate.Date == DateTime.UtcNow.Date && IltSchedule.EndTime < DateTime.Now.TimeOfDay))
                    return "Cannot request as schedule " + IltSchedule.ScheduleCode + " end date & time is over.";
                else if (IltSchedule.StartDate.Date < DateTime.UtcNow.Date || (IltSchedule.StartDate.Date == DateTime.UtcNow.Date && IltSchedule.StartTime < DateTime.Now.TimeOfDay))
                    return "Cannot request as schedule " + IltSchedule.ScheduleCode + " is already started.";
            }
            var SCHEDULE_SEATCAPACITY = await GetMasterConfigurableParameterValue("SCHEDULE_SEATCAPACITY");
            string AccomodationCapacity = "0";
            int PlaceID = IltSchedule.PlaceID;
            if (Convert.ToString(SCHEDULE_SEATCAPACITY).ToLower() == "yes")
            {
                AccomodationCapacity = Convert.ToString(await this._db.ILTSchedule.Where(a => a.ID == aPIILTRequestResponse.ScheduleID).Select(a => a.ScheduleCapacity).FirstOrDefaultAsync());
            }
            else
            {
                AccomodationCapacity = await this._db.TrainingPlace.Where(a => a.IsActive == true && a.IsDeleted == false && a.Id == PlaceID)
                                                                       .Select(ilt => ilt.AccommodationCapacity).FirstOrDefaultAsync();
            }
            int CountRegistered = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                               && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                               && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                               && a.CourseID == aPIILTRequestResponse.CourseID
                                                                               && a.TrainingRequestStatus == "Registered"
                                                                               && a.IsActiveNomination == true)
                                                                               .Select(n => n.ID).CountAsync();

            int CountUnavailable = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                               && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                               && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                               && a.CourseID == aPIILTRequestResponse.CourseID
                                                                               && a.TrainingRequesStatus == "Unavailability")
                                                                               .Select(n => n.ID).CountAsync();

            int CountReject = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                               && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                               && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                               && a.CourseID == aPIILTRequestResponse.CourseID
                                                                               && a.TrainingRequesStatus == "Rejected")
                                                                               .Select(n => n.ID).CountAsync();

            int CountRequest = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                               && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                               && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                               && a.CourseID == aPIILTRequestResponse.CourseID
                                                                               && a.TrainingRequesStatus == "Requested")
                                                                               .Select(n => n.ID).CountAsync();

            ILTRequestResponse objILTResponse = Mapper.Map<ILTRequestResponse>(aPIILTRequestResponse);
            objILTResponse.UserID = UserId;
            objILTResponse.CreatedBy = UserId;
            objILTResponse.ModifiedBy = UserId;
            objILTResponse.CreatedDate = DateTime.UtcNow;
            objILTResponse.ModifiedDate = DateTime.UtcNow;
            objILTResponse.IsDeleted = false;
            objILTResponse.IsActive = true;
            objILTResponse.BatchID = 0;
            if (aPIILTRequestResponse.TrainingRequesStatus.ToLower() == "requested")
            {
                if (PlaceID != 0 || Convert.ToInt32(AccomodationCapacity) != 0) // check for vilt 
                {
                    if (OrganizationCode.ToLower().Contains("wns"))
                    {
                        if (Convert.ToInt32(AccomodationCapacity) <= CountRegistered)
                        {
                            objILTResponse.TrainingRequesStatus = "Waiting";
                        }
                    }
                    else
                    {
                        if ((Convert.ToInt32(AccomodationCapacity)) <= ((CountRegistered + CountRequest) - (CountUnavailable + CountReject)))
                        {
                            objILTResponse.TrainingRequesStatus = "Waiting";
                        }
                    }
                }

                objILTResponse.ID = 0;
                await this.Add(objILTResponse);

                // code for notification //
                if (objILTResponse.TrainingRequesStatus != "Waiting")
                {
                    ILTSchedule objILTSchedule = new ILTSchedule();

                    objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == aPIILTRequestResponse.ScheduleID).FirstOrDefaultAsync();
                    string venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
                    string trainingName = this._db.Module.Where(a => a.Id == aPIILTRequestResponse.ModuleID).Select(a => a.Name).FirstOrDefault();

                    string title = "Send Schedule Request";
                    string token = _identitySv.GetToken();
                    int UserIDToSend = UserId;
                    string Type = Record.Enrollment1;
                    string Message = "Your training request for schedule '{ScheduleCode}' of module '{ModuleName}' has been successfully submitted for approval.";
                    Message = Message.Replace("{ScheduleCode}", objILTSchedule.ScheduleCode);
                    Message = Message.Replace("{ModuleName}", trainingName);
                    await ScheduleRequestNotificationTo_Common(objILTResponse.CourseID, objILTResponse.ScheduleID, title, token, UserIDToSend, Message, Type);

                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "GetAllSupervisorsAndDirector";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.BigInt) { Value = objILTResponse.UserID });
                                cmd.Parameters.Add(new SqlParameter("@Flag", SqlDbType.Int) { Value = 2 });


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
                                    string UserName = string.IsNullOrEmpty(row["UserName"].ToString()) ? null : Security.Decrypt(row["UserName"].ToString());
                                    string EmailID = string.IsNullOrEmpty(row["EmailID"].ToString()) ? null : Security.Decrypt(row["EmailID"].ToString());
                                    int UserIdtoSend = string.IsNullOrEmpty(row["UserId"].ToString()) ? 0 : int.Parse(row["UserId"].ToString());
                                    string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                    string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());

                                    string Message_LM = "You have a pending training request for '{ScheduleCode}' of '{ModuleName}' awaiting your approval.";
                                    Message_LM = Message_LM.Replace("{ScheduleCode}", objILTSchedule.ScheduleCode);
                                    Message_LM = Message_LM.Replace("{ModuleName}", trainingName);

                                    ApiNotification Notification = new ApiNotification();
                                    Notification.Title = title;
                                    Notification.Message = Message_LM;
                                    Notification.Url = TlsUrl.NotificationAPost + objILTResponse.CourseID + '/' + objILTResponse.ScheduleID;
                                    Notification.Type = Record.ScheduleCancel;
                                    Notification.UserId = UserIdtoSend;
                                    lstApiNotification.Add(Notification);
                                    count++;
                                    if (count % Constants.BATCH_SIZE == 0)
                                    {
                                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                        lstApiNotification.Clear();
                                    }

                                    await _email.ScheduleRequestedMail(OrganizationCode, EndUserEmailId, EmailID, null, UserName, trainingName, EndUserName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);
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

                    if (string.Equals(OrganizationCode, "bandhan", StringComparison.CurrentCultureIgnoreCase))
                    {
                        List<APINominateUserSMS> SMSListManager = new List<APINominateUserSMS>();
                        var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_ILT");

                        string urlSMS = string.Empty;
                        List<UserListForSMS> userListForSMS = new List<UserListForSMS>();
                        if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                        {
                            string CourseTitle = _db.Course.Where(x => x.Id == aPIILTRequestResponse.CourseID).Select(d => d.Title).FirstOrDefault();
                            string EndUserName = string.Empty;

                            urlSMS = _configuration[Configuration.NotificationApi];

                            using (var dbContext = this._customerConnection.GetDbContext())
                            {
                                using (var connection = dbContext.Database.GetDbConnection())
                                {
                                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                        connection.Open();

                                    DynamicParameters parameters = new DynamicParameters();
                                    parameters.Add("@UserId", objILTResponse.UserID);
                                    var result = await SqlMapper.QueryAsync<UserListForSMS>((SqlConnection)connection, "[dbo].[GetManagerAndSkipLevelManagerDetailsByUserId]", parameters, null, null, CommandType.StoredProcedure);
                                    userListForSMS = result.ToList();
                                    connection.Close();
                                }
                            }
                            UserListForSMS EndUser = userListForSMS.Where(x => x.Id == objILTResponse.UserID).FirstOrDefault();
                            userListForSMS.Remove(EndUser);
                            foreach (UserListForSMS item in userListForSMS)
                            {
                                APINominateUserSMS objSMSManager = new APINominateUserSMS();
                                objSMSManager.CourseTitle = CourseTitle;
                                objSMSManager.UserName = EndUser.UserName;
                                objSMSManager.StartDate = objILTSchedule.StartDate;
                                objSMSManager.MobileNumber = !string.IsNullOrEmpty(item.MobileNumber) ? Security.Decrypt(item.MobileNumber) : null;
                                objSMSManager.organizationCode = OrganizationCode;
                                objSMSManager.UserID = item.Id;
                                SMSListManager.Add(objSMSManager);
                            }

                            await _email.SendScheduleRequestNotificationSMSToManager(SMSListManager);
                        }
                    }
                }
                // code for notification //
            }

            if (aPIILTRequestResponse.TrainingRequesStatus.ToLower() == "availability")
            {
                objILTResponse.ID = 0;
                await this.Add(objILTResponse);
            }

            if (aPIILTRequestResponse.TrainingRequesStatus.ToLower() == "unavailability")
            {
                objILTResponse.ID = 0;
                await this.Add(objILTResponse);
                ILTRequestResponse objILTRequestResponse = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.TrainingRequesStatus == "Waiting")
                                                                                                 .FirstOrDefaultAsync();
                if (objILTRequestResponse != null)
                {
                    ILTRequestResponse firstILTRequestResponseForWaiting = await this._db.ILTRequestResponse.AsNoTracking().Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.TrainingRequesStatus == "Waiting").OrderBy(a => a.ID)
                                                                                                 .FirstOrDefaultAsync();

                    firstILTRequestResponseForWaiting.IsActive = false;
                    firstILTRequestResponseForWaiting.IsDeleted = true;
                    await this.Update(firstILTRequestResponseForWaiting);

                    ILTRequestResponse objILTResponseRequest = new ILTRequestResponse();
                    objILTResponseRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    objILTResponseRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    objILTResponseRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    objILTResponseRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                    objILTResponseRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    objILTResponseRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                    objILTResponseRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                    objILTResponseRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                    objILTResponseRequest.TrainingRequesStatus = "Requested";
                    objILTResponseRequest.IsActive = true;
                    objILTResponseRequest.IsDeleted = false;
                    objILTResponseRequest.ID = 0;
                    await this.Add(objILTResponseRequest);
                }

                TrainingNomination updateForUnavailability = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.UserID == UserId && a.TrainingRequestStatus == "Registered"
                                                                                                         && a.IsActiveNomination == true).OrderByDescending(a => a.ID).FirstOrDefaultAsync();
                if (updateForUnavailability != null)
                {
                    updateForUnavailability.TrainingRequestStatus = "Unregistered";
                    updateForUnavailability.ModifiedBy = UserId;
                    updateForUnavailability.ModifiedDate = DateTime.UtcNow;
                    updateForUnavailability.IsActive = true;
                    updateForUnavailability.IsDeleted = false;
                    updateForUnavailability.IsActiveNomination = false;

                    this._db.TrainingNomination.Update(updateForUnavailability);
                    this._db.SaveChanges();

                }
            }
            return "Success";
        }
        public async Task<string> PostBatchRequest(APIILTBatchRequestResponse aPIILTBatchRequestResponse, int UserId, string OrganizationCode)
        {
            ILTBatch iLTBatch = await _db.ILTBatch.Where(bat => bat.Id == aPIILTBatchRequestResponse.BatchID && bat.IsDeleted == false).FirstOrDefaultAsync();

            if (iLTBatch.EndDate.Date < DateTime.UtcNow.Date || (iLTBatch.EndDate.Date == DateTime.UtcNow.Date && iLTBatch.EndTime < DateTime.Now.TimeOfDay))
                return "Cannot request as batch " + iLTBatch.BatchCode + " end date & time is over.";
            else if (iLTBatch.StartDate.Date < DateTime.UtcNow.Date || (iLTBatch.StartDate.Date == DateTime.UtcNow.Date && iLTBatch.StartTime < DateTime.Now.TimeOfDay))
                return "Cannot request as batch " + iLTBatch.BatchCode + " is already started.";

            if (aPIILTBatchRequestResponse.TrainingRequestStatus == "Requested")
            {
                List<APIILTCheckUserBusy> aPIILTCheckUserBusyList = new List<APIILTCheckUserBusy>();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserId", UserId);
                        parameters.Add("@BatchId", aPIILTBatchRequestResponse.BatchID);
                        var result = await SqlMapper.QueryAsync<APIILTCheckUserBusy>((SqlConnection)connection, "[dbo].[CheckUserIsBusyInBatchSchedules]", parameters, null, null, CommandType.StoredProcedure);
                        aPIILTCheckUserBusyList = result.ToList();
                        connection.Close();
                    }
                }
                if (aPIILTCheckUserBusyList.Count > 0)
                {
                    if (aPIILTCheckUserBusyList.Where(x => x.IsUserBusy == true).Count() > 0)
                    {
                        string SCH_Code = string.Join(",", aPIILTCheckUserBusyList.Select(x => x.ScheduleCode).ToList());
                        return "Cannot request as you are busy in another schedule " + SCH_Code + " at this time.";
                    }
                    else if (!string.Equals(OrganizationCode, "canh", StringComparison.CurrentCultureIgnoreCase)
                             && !string.Equals(OrganizationCode, "tcns", StringComparison.CurrentCultureIgnoreCase)
                             && !string.Equals(OrganizationCode, "tablez", StringComparison.CurrentCultureIgnoreCase)
                             && !string.Equals(OrganizationCode, "ujjivan", StringComparison.CurrentCultureIgnoreCase)
                             && aPIILTCheckUserBusyList.Where(x => x.CanNominate == false).Count() > 0)
                        return aPIILTCheckUserBusyList.Where(x => x.CanNominate == false).Select(x => x.ErrorMessage.Replace("Cannot nominate", "Cannot request")).FirstOrDefault();
                }
            }

            List<ILTSchedule> iLTSchedules = await _db.ILTSchedule.Where(sch => sch.BatchId == iLTBatch.Id && sch.IsDeleted == false).ToListAsync();
            string CourseName = this._db.Course.Where(a => a.Id == iLTBatch.CourseId).Select(a => a.Title).FirstOrDefault();
            List<APIILTScheduleRequestStatus> aPIILTScheduleRequestStatusList = new List<APIILTScheduleRequestStatus>();

            foreach (ILTSchedule schedule in iLTSchedules)
            {
                APIILTScheduleRequestStatus aPIILTScheduleRequestStatus = new APIILTScheduleRequestStatus();
                aPIILTScheduleRequestStatus.ScheduleCode = schedule.ScheduleCode;
                aPIILTScheduleRequestStatus.TrainingName = this._db.Module.Where(a => a.Id == schedule.ModuleId).Select(a => a.Name).FirstOrDefault();

                var SCHEDULE_SEATCAPACITY = await GetMasterConfigurableParameterValue("SCHEDULE_SEATCAPACITY");
                string AccomodationCapacity = "0";
                int PlaceID = schedule.PlaceID;
                if (Convert.ToString(SCHEDULE_SEATCAPACITY).ToLower() == "yes")
                {
                    AccomodationCapacity = Convert.ToString(await this._db.ILTSchedule.Where(a => a.ID == schedule.ID).Select(a => a.ScheduleCapacity).FirstOrDefaultAsync());
                }
                else
                {
                    var TrainingPlaceDetails = await this._db.TrainingPlace.Where(a => a.IsActive == true && a.IsDeleted == false && a.Id == PlaceID)
                                                                       .Select(ilt => new
                                                                       {
                                                                           AccomodationCapacity = ilt.AccommodationCapacity,
                                                                           Venue = ilt.PostalAddress
                                                                       }).FirstOrDefaultAsync();

                    AccomodationCapacity = TrainingPlaceDetails.AccomodationCapacity;
                    aPIILTScheduleRequestStatus.Venue = TrainingPlaceDetails.Venue;
                }






                ILTRequestResponse iLTRequestResponseExists = await _db.ILTRequestResponse.Where(x => x.ScheduleID == schedule.ID && x.IsDeleted == false && x.UserID == UserId && x.BatchID == iLTBatch.Id).OrderByDescending(o => o.ID).FirstOrDefaultAsync();
                if (iLTRequestResponseExists == null || (iLTRequestResponseExists != null && iLTRequestResponseExists.TrainingRequesStatus.ToLower() == "rejected"))
                {
                    int CountRegistered = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                       && a.ScheduleID == schedule.ID
                                                                                       && a.ModuleID == schedule.ModuleId
                                                                                       && a.CourseID == schedule.CourseId
                                                                                       && a.TrainingRequestStatus == "Registered"
                                                                                       && a.IsActiveNomination == true)
                                                                                       .Select(n => n.ID).CountAsync();

                    var RequestList = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                       && a.ScheduleID == schedule.ID
                                                                                       && a.ModuleID == schedule.ModuleId
                                                                                       && a.CourseID == schedule.CourseId)
                                                                                       .Select(n => new { RequestId = n.ID, TrainingRequesStatus = n.TrainingRequesStatus }).ToListAsync();

                    int CountUnavailable = RequestList.Where(x => x.TrainingRequesStatus == "Unavailability").Count();
                    int CountReject = RequestList.Where(x => x.TrainingRequesStatus == "Rejected").Count();
                    int CountRequest = RequestList.Where(x => x.TrainingRequesStatus == "Requested").Count();

                    ILTRequestResponse objILTResponse = new ILTRequestResponse();
                    objILTResponse.CourseID = iLTBatch.CourseId;
                    objILTResponse.ModuleID = schedule.ModuleId;
                    objILTResponse.ScheduleID = schedule.ID;
                    objILTResponse.TrainingRequesStatus = aPIILTBatchRequestResponse.TrainingRequestStatus;
                    objILTResponse.UserID = UserId;
                    objILTResponse.CreatedBy = UserId;
                    objILTResponse.ModifiedBy = UserId;
                    objILTResponse.CreatedDate = DateTime.UtcNow;
                    objILTResponse.ModifiedDate = DateTime.UtcNow;
                    objILTResponse.IsDeleted = false;
                    objILTResponse.IsActive = true;
                    objILTResponse.BatchID = iLTBatch.Id;

                    if (aPIILTBatchRequestResponse.TrainingRequestStatus.ToLower() == "requested")
                    {
                        if (PlaceID != 0 || Convert.ToInt32(AccomodationCapacity) != 0) // check for vilt 
                        {

                            if (((Convert.ToInt32(AccomodationCapacity)) <= ((CountRegistered + CountRequest) - (CountUnavailable + CountReject)))
                            || (aPIILTScheduleRequestStatusList.Where(x => x.RequestStatus == "Waiting").Count() > 0))
                                objILTResponse.TrainingRequesStatus = "Waiting";
                        }

                        objILTResponse.ID = 0;
                        await this.Add(objILTResponse);
                        aPIILTScheduleRequestStatus.RequestStatus = objILTResponse.TrainingRequesStatus;
                        aPIILTScheduleRequestStatus.Response = "Success";
                    }
                }
                else
                {
                    return "Already Requested for this batch " + iLTBatch.BatchCode;
                }
                aPIILTScheduleRequestStatusList.Add(aPIILTScheduleRequestStatus);
            }

            if (aPIILTScheduleRequestStatusList.Where(x => x.Response == "Success" && x.RequestStatus == "Requested").Count() > 0)
            {
                string ScheduleCodes = string.Join(", ", aPIILTScheduleRequestStatusList.Where(x => x.Response == "Success" && x.RequestStatus == "Requested").Select(sch => sch.ScheduleCode).ToList());
                string Venues = string.Join(", ", aPIILTScheduleRequestStatusList.Where(x => x.Response == "Success" && x.RequestStatus == "Requested").Select(sch => sch.Venue).ToList());
                string title = "Send Batch Request";
                string token = _identitySv.GetToken();
                int UserIDToSend = UserId;
                string Type = Record.Enrollment1;
                string Message = "Your training request for batch '{BatchCode}'-'{BatchName}' of course '{CourseName}' having schedules {ScheduleCodes} has been successfully submitted for approval.";
                Message = Message.Replace("{BatchCode}", iLTBatch.BatchCode);
                Message = Message.Replace("{BatchName}", iLTBatch.BatchName);
                Message = Message.Replace("{CourseName}", CourseName);
                Message = Message.Replace("{ScheduleCodes}", ScheduleCodes);
                await ScheduleRequestNotificationTo_Common(iLTBatch.CourseId, iLTBatch.Id, title, token, UserIDToSend, Message, Type);

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllSupervisorsAndDirector";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.BigInt) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Flag", SqlDbType.Int) { Value = 2 });

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
                                string UserName = string.IsNullOrEmpty(row["UserName"].ToString()) ? null : Security.Decrypt(row["UserName"].ToString());
                                string EmailID = string.IsNullOrEmpty(row["EmailID"].ToString()) ? null : Security.Decrypt(row["EmailID"].ToString());
                                int UserIdtoSend = string.IsNullOrEmpty(row["UserId"].ToString()) ? 0 : int.Parse(row["UserId"].ToString());
                                string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());

                                string Message_LM = "You have a pending training request for batch '{BatchCode}'-'{BatchName}' of course {CourseName} having schedules '{ScheduleCodes}' awaiting your approval.";
                                Message_LM = Message_LM.Replace("{BatchCode}", iLTBatch.BatchCode);
                                Message_LM = Message_LM.Replace("{BatchName}", iLTBatch.BatchName);
                                Message_LM = Message_LM.Replace("{CourseName}", CourseName);
                                Message_LM = Message_LM.Replace("{ScheduleCodes}", ScheduleCodes);

                                ApiNotification Notification = new ApiNotification();
                                Notification.Title = title;
                                Notification.Message = Message_LM;
                                Notification.Url = TlsUrl.NotificationAPost + iLTBatch.CourseId;
                                Notification.Type = Record.ScheduleCancel;
                                Notification.UserId = UserIdtoSend;
                                lstApiNotification.Add(Notification);
                                count++;
                                if (count % Constants.BATCH_SIZE == 0)
                                {
                                    await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                    lstApiNotification.Clear();
                                }

                                await _email.ScheduleRequestedMail(OrganizationCode, EndUserEmailId, EmailID, null, UserName, CourseName, EndUserName, Convert.ToString(iLTBatch.StartTime), Convert.ToString(iLTBatch.EndTime), Convert.ToString(iLTBatch.StartDate), Convert.ToString(iLTBatch.EndDate), Venues);
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
            return "Success";
        }
        public async Task<string> PostBatchResponse(List<APIILTBatchRequestApprove> aPIILTBatchRequestApprove, int UserId, string OrgCode)
        {
            ILTBatch iLTBatch = _db.ILTBatch.Where(x => x.Id == aPIILTBatchRequestApprove.Select(x => x.BatchId).FirstOrDefault() && x.IsDeleted == false).FirstOrDefault();

            //if (iLTBatch.EndDate.Date < DateTime.UtcNow.Date || (iLTBatch.EndDate.Date == DateTime.UtcNow.Date && iLTBatch.EndTime < DateTime.Now.TimeOfDay))
            //    return "Cannot change request as batch " + iLTBatch.BatchCode + " end date & time is over.";
            //else if (iLTBatch.StartDate.Date < DateTime.UtcNow.Date || (iLTBatch.StartDate.Date == DateTime.UtcNow.Date && iLTBatch.StartTime < DateTime.Now.TimeOfDay))
            //    return "Cannot change request as batch " + iLTBatch.BatchCode + " is already started.";

            foreach (APIILTBatchRequestApprove item in aPIILTBatchRequestApprove)
            {
                List<ILTRequestResponse> iLTScheduleRequests = new List<ILTRequestResponse>();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserId", item.UserId);
                        parameters.Add("@BatchId", item.BatchId);
                        var result = await SqlMapper.QueryAsync<ILTRequestResponse>((SqlConnection)connection, "[dbo].[GetUserScheduleRequestsByBatchId]", parameters, null, null, CommandType.StoredProcedure);
                        iLTScheduleRequests = result.ToList();
                        connection.Close();
                    }
                }

                List<int> scheduleIds = iLTScheduleRequests.Select(x => x.ScheduleID).Distinct().ToList();
                List<int> moduleIds = iLTScheduleRequests.Select(x => x.ModuleID).Distinct().ToList();
                List<ILTSchedule> iLTSchedules = await _db.ILTSchedule.Where(x => scheduleIds.Contains(x.ID) && x.BatchId == iLTBatch.Id && x.IsDeleted == false).ToListAsync();
                List<int> placeIds = iLTSchedules.Select(x => x.PlaceID).ToList();
                var trainingPlaces = await _db.TrainingPlace.Where(x => placeIds.Contains(x.Id) && x.IsDeleted == false).Select(x => new { Id = x.Id, AccommodationCapacity = x.AccommodationCapacity, Venue = x.PostalAddress }).ToListAsync();
                string CourseName = await _db.Course.Where(x => x.Id == iLTBatch.CourseId && x.IsDeleted == false).Select(x => x.Title).FirstOrDefaultAsync();

                List<APIILTBatchRequestApproveStatus> aPIILTBatchRequestApproveStatusList = new List<APIILTBatchRequestApproveStatus>();

                foreach (ILTRequestResponse request in iLTScheduleRequests)
                {
                    ILTSchedule iLTSchedule = iLTSchedules.Where(x => x.ID == request.ScheduleID && x.BatchId == request.BatchID).FirstOrDefault();
                    var SCHEDULE_SEATCAPACITY = await GetMasterConfigurableParameterValue("SCHEDULE_SEATCAPACITY");
                    string AccomodationCapacity = "0";
                    if (Convert.ToString(SCHEDULE_SEATCAPACITY).ToLower() == "yes")
                    {
                        AccomodationCapacity = Convert.ToString(await this._db.ILTSchedule.Where(a => a.ID == iLTSchedule.ID).Select(a => a.ScheduleCapacity).FirstOrDefaultAsync());
                    }
                    else
                    {
                        var PlaceDetails = trainingPlaces.Where(a => a.Id == iLTSchedule.PlaceID).Select(ilt => new { AccommodationCapacity = ilt.AccommodationCapacity, Venue = ilt.Venue }).FirstOrDefault();
                        AccomodationCapacity = PlaceDetails.AccommodationCapacity;
                        string venue = PlaceDetails.Venue;
                    }





                    int CountRegistered = await _db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                       && a.ScheduleID == request.ScheduleID
                                                                                       && a.ModuleID == request.ModuleID
                                                                                       && a.CourseID == request.CourseID
                                                                                       && a.TrainingRequestStatus == "Registered"
                                                                                       && a.IsActiveNomination == true)
                                                                                       .Select(n => n.ID).CountAsync();

                    var requestResponseList = await _db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                       && a.ScheduleID == request.ScheduleID
                                                                                       && a.ModuleID == request.ModuleID
                                                                                       && a.CourseID == request.CourseID)
                                                                            .Select(d => new { Id = d.ID, TrainingRequesStatus = d.TrainingRequesStatus }).ToListAsync();

                    int CountUnavailable = requestResponseList.Where(x => x.TrainingRequesStatus == "Unavailability").Select(d => d.Id).Count();
                    int CountReject = requestResponseList.Where(x => x.TrainingRequesStatus == "Rejected").Select(d => d.Id).Count();
                    int CountRequest = requestResponseList.Where(x => x.TrainingRequesStatus == "Requested").Select(d => d.Id).Count();

                    ILTRequestResponse objILTResponse = new ILTRequestResponse();
                    objILTResponse.CourseID = request.CourseID;
                    objILTResponse.ModuleID = request.ModuleID;
                    objILTResponse.ScheduleID = request.ScheduleID;
                    objILTResponse.BatchID = request.BatchID;
                    objILTResponse.ReferenceRequestID = request.ID;
                    objILTResponse.UserID = request.UserID;
                    objILTResponse.CreatedBy = UserId;
                    objILTResponse.ModifiedBy = UserId;
                    objILTResponse.CreatedDate = DateTime.UtcNow;
                    objILTResponse.ModifiedDate = DateTime.UtcNow;
                    objILTResponse.IsDeleted = false;
                    objILTResponse.IsActive = true;
                    objILTResponse.TrainingRequesStatus = item.TrainingRequestStatus;

                    if (item.TrainingRequestStatus.ToLower() == "requested")
                    {
                        if (Convert.ToInt32(AccomodationCapacity) != 0)
                        {
                            if ((Convert.ToInt32(AccomodationCapacity)) <= ((CountRegistered + CountRequest) - (CountUnavailable + CountReject)))
                            {
                                objILTResponse.TrainingRequesStatus = "Waiting";
                            }
                        }
                        objILTResponse.ID = 0;
                        _db.ILTRequestResponse.Add(objILTResponse);
                        await _db.SaveChangesAsync();
                    }

                    if (item.TrainingRequestStatus.ToLower() == "approved")
                    {
                        objILTResponse.ID = 0;
                        objILTResponse.UserID = item.UserId;
                        _db.ILTRequestResponse.Add(objILTResponse);
                        await _db.SaveChangesAsync();

                        // ---- Generate OTP --- //
                        string Otp = null;
                        bool flag = false;

                        while (flag == false)
                        {
                            Otp = GenerateRandomPassword();
                            List<TrainingNomination> existingOTP = _db.TrainingNomination.Where(a => a.ScheduleID == request.ScheduleID
                                                                                                      && a.ModuleID == request.ModuleID && a.CourseID == request.CourseID
                                                                                                      && a.IsActive == true && a.IsDeleted == Record.NotDeleted && a.IsActiveNomination == true).ToList();
                            if (existingOTP != null)
                            {
                                flag = true;
                            }
                        }

                        TrainingNomination checkTrainingNomination = await _db.TrainingNomination.Where(a => a.ScheduleID == request.ScheduleID && a.UserID == request.UserID && a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true).FirstOrDefaultAsync();

                        if (checkTrainingNomination == null)
                        {

                            if (!string.Equals(OrgCode, "canh", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(OrgCode, "tcns", StringComparison.CurrentCultureIgnoreCase))
                            {
                                List<TrainingNomination> oldNominations = await _db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true && a.CourseID == request.CourseID && a.ModuleID == request.ModuleID && a.UserID == request.UserID).ToListAsync();

                                foreach (TrainingNomination item1 in oldNominations)
                                {
                                    item1.IsActiveNomination = false;
                                }
                                _db.TrainingNomination.UpdateRange(oldNominations);
                                await _db.SaveChangesAsync();
                            }

                            TrainingNomination obj = new TrainingNomination();
                            TrainingNomination lastRequestCode = await _db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true).OrderByDescending(a => a.ID).FirstOrDefaultAsync();
                            if (lastRequestCode == null)
                            {
                                obj.RequestCode = "RQ1";
                            }
                            else
                            {
                                obj.RequestCode = "RQ" + (lastRequestCode.ID + 1);
                            }
                            obj.ScheduleID = request.ScheduleID;
                            obj.UserID = request.UserID;
                            obj.TrainingRequestStatus = "Registered";
                            obj.ModuleID = request.ModuleID;
                            obj.CourseID = request.CourseID;
                            obj.OTP = Otp;
                            obj.CreatedBy = UserId;
                            obj.CreatedDate = DateTime.UtcNow;
                            obj.ModifiedBy = UserId;
                            obj.ModifiedDate = DateTime.UtcNow;
                            obj.IsActive = true;
                            obj.IsDeleted = false;
                            obj.IsActiveNomination = true;
                            _db.TrainingNomination.Add(obj);
                            await _db.SaveChangesAsync();

                            APIILTBatchRequestApproveStatus objStatus = new APIILTBatchRequestApproveStatus();
                            objStatus.RequestId = request.ID;
                            objStatus.Status = "approved";
                            objStatus.ScheduleCode = iLTSchedule.ScheduleCode;
                            aPIILTBatchRequestApproveStatusList.Add(objStatus);
                        }
                    }

                    if (item.TrainingRequestStatus.ToLower() == "rejected")
                    {
                        ILTTrainingAttendance iLTTrainingAttendance = await _db.ILTTrainingAttendance.Where(x => x.ScheduleID == request.ScheduleID && x.UserID == request.UserID && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();

                        if (iLTTrainingAttendance != null)
                            return "Cannot reject batch request as attendance found for schedule " + iLTSchedule.ScheduleCode + " of batch " + iLTBatch.BatchCode + ".";

                        TrainingNomination trainingNomination = await _db.TrainingNomination.Where(x => x.ScheduleID == request.ScheduleID && x.UserID == request.UserID && x.IsActiveNomination == true && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                        if (trainingNomination != null)
                        {
                            trainingNomination.IsActive = false;
                            trainingNomination.IsDeleted = true;
                            _db.TrainingNomination.Update(trainingNomination);
                            await _db.SaveChangesAsync();
                        }

                        objILTResponse.UserID = item.UserId;
                        objILTResponse.Reason = item.Reason;
                        objILTResponse.ID = 0;
                        _db.ILTRequestResponse.Add(objILTResponse);
                        await _db.SaveChangesAsync();

                        ILTRequestResponse objILTRequestResponse = await _db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                                 && a.ModuleID == request.ModuleID
                                                                                                                 && a.CourseID == request.CourseID
                                                                                                                 && a.ScheduleID == request.ScheduleID
                                                                                                                 && a.TrainingRequesStatus == "Waiting")
                                                                                                         .FirstOrDefaultAsync();
                        if (objILTRequestResponse != null)
                        {
                            ILTRequestResponse firstILTRequestResponseForWaiting = await _db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                                 && a.ModuleID == request.ModuleID
                                                                                                                 && a.CourseID == request.CourseID
                                                                                                                 && a.ScheduleID == request.ScheduleID
                                                                                                                 && a.TrainingRequesStatus == "Waiting").OrderBy(a => a.ID)
                                                                                                         .FirstOrDefaultAsync();

                            firstILTRequestResponseForWaiting.IsActive = false;
                            firstILTRequestResponseForWaiting.IsDeleted = true;
                            _db.ILTRequestResponse.Update(firstILTRequestResponseForWaiting);
                            await _db.SaveChangesAsync();

                            ILTRequestResponse objILTResponseRequest = new ILTRequestResponse();
                            objILTResponseRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                            objILTResponseRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                            objILTResponseRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                            objILTResponseRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                            objILTResponseRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                            objILTResponseRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                            objILTResponseRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                            objILTResponseRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                            objILTResponseRequest.TrainingRequesStatus = "Requested";
                            objILTResponseRequest.IsActive = true;
                            objILTResponseRequest.IsDeleted = false;
                            objILTResponseRequest.ID = 0;
                            _db.ILTRequestResponse.Add(objILTResponseRequest);
                            await _db.SaveChangesAsync();
                        }
                        APIILTBatchRequestApproveStatus objStatus = new APIILTBatchRequestApproveStatus();
                        objStatus.RequestId = request.ID;
                        objStatus.Status = "rejected";
                        objStatus.Reason = request.Reason;
                        objStatus.ScheduleCode = iLTSchedule.ScheduleCode;
                        aPIILTBatchRequestApproveStatusList.Add(objStatus);
                    }
                }

                if (aPIILTBatchRequestApproveStatusList.Count > 0)
                {
                    string EndUserName = null, EndUserEmailId = null, EndUserMobileNumber = string.Empty;
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "GetAllSupervisorsAndDirector";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.BigInt) { Value = item.UserId });
                                cmd.Parameters.Add(new SqlParameter("@Flag", SqlDbType.Int) { Value = 1 });

                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count <= 0)
                                {
                                    reader.Dispose();
                                    connection.Close();
                                }
                                foreach (DataRow row in dt.Rows)
                                {
                                    EndUserName = string.IsNullOrEmpty(Convert.ToString(row["EndUserName"])) ? null : Convert.ToString(row["EndUserName"]);
                                    EndUserEmailId = string.IsNullOrEmpty(Convert.ToString(row["EndUserEmailId"])) ? null : Security.Decrypt(Convert.ToString(row["EndUserEmailId"]));
                                    EndUserMobileNumber = string.IsNullOrEmpty(Convert.ToString(row["MobileNumber"])) ? null : Security.Decrypt(Convert.ToString(row["MobileNumber"]));
                                }
                                reader.Dispose();
                            }
                            connection.Close();
                        }
                    }
                    string status = aPIILTBatchRequestApproveStatusList.Select(x => x.Status).FirstOrDefault();
                    string reason = aPIILTBatchRequestApprove.Select(x => x.Reason).FirstOrDefault();
                    string scheduleCodes = string.Join(", ", aPIILTBatchRequestApproveStatusList.Select(x => x.ScheduleCode).ToList());
                    if (status == "approved")
                    {
                        await _email.TrainingBatchRequestApprovalMail(OrgCode, EndUserEmailId, iLTBatch.BatchCode, null, EndUserName, scheduleCodes, Convert.ToString(iLTBatch.StartTime), Convert.ToString(iLTBatch.EndTime), Convert.ToString(iLTBatch.StartDate), Convert.ToString(iLTBatch.EndDate), CourseName);

                        string title = "Batch Request Approval";
                        string token = _identitySv.GetToken();
                        int UserIDToSend = UserId;
                        string Type = Record.Enrollment1;
                        string Message = "Your training request for batch '{BatchCode}' of course '{CourseName}' having schedules '{ScheduleCodes}' has been successfully approved.";
                        Message = Message.Replace("{BatchCode}", iLTBatch.BatchCode);
                        Message = Message.Replace("{CourseName}", CourseName);
                        Message = Message.Replace("{ScheduleCodes}", scheduleCodes);
                        await ScheduleRequestNotificationTo_Common(iLTBatch.CourseId, iLTBatch.Id, title, token, item.UserId, Message, Type);

                    }
                    else if (status == "rejected")
                    {
                        await _email.TrainingBatchRequestRejectedMail(OrgCode, EndUserEmailId, iLTBatch.BatchCode, null, EndUserName, scheduleCodes, Convert.ToString(iLTBatch.StartTime), Convert.ToString(iLTBatch.EndTime), Convert.ToString(iLTBatch.StartDate), Convert.ToString(iLTBatch.EndDate), CourseName, reason);

                        string title = "Batch Request Rejection";
                        string token = _identitySv.GetToken();
                        int UserIDToSend = UserId;
                        string Type = Record.Enrollment1;
                        string Message = "Your training request for batch'{BatchCode}' of course '{CourseName}' having schedules '{ScheduleCodes}' has been rejected for the reason {Reason}.";
                        Message = Message.Replace("{BatchCode}", iLTBatch.BatchCode);
                        Message = Message.Replace("{CourseName}", CourseName);
                        Message = Message.Replace("{ScheduleCodes}", scheduleCodes);
                        Message = Message.Replace("{Reason}", reason);
                        await ScheduleRequestNotificationTo_Common(iLTBatch.CourseId, iLTBatch.Id, title, token, item.UserId, Message, Type);
                    }
                }
            }
            return "Success";
        }
        public async Task<int> ScheduleRequestNotificationTo_Common(int courseId, int ScheduleId, string title, string token, int ReportsToID, string Message, string type, int? CourseId = null)
        {
            List<ApiNotification> apiNotifications = new List<ApiNotification>();
            ApiNotification Notification = new ApiNotification();
            Notification.Title = title;
            Notification.Message = Message;
            Notification.Url = TlsUrl.NotificationAPost + courseId;
            Notification.Type = type;
            Notification.UserId = ReportsToID;
            apiNotifications.Add(Notification);
            await this._notification.ScheduleRequestNotificationTo_Common(apiNotifications, token);
            return 1;

        }
        public async Task<int> ScheduleRequestNotificationTo_CommonBulk(List<ApiNotification> Notification, string token)
        {
            await this._notification.ScheduleRequestNotificationTo_Common(Notification, token);
            return 1;
        }


        public async Task<string> PostResponse(APIILTRequestResponse aPIILTRequestResponse, int UserId, string OrgCode)
        {
            int PlaceID = await this._db.ILTSchedule.Where(a => a.IsActive == true && a.IsDeleted == false && a.ID == aPIILTRequestResponse.ScheduleID)
                                                                    .Select(ilt => ilt.PlaceID).FirstOrDefaultAsync();

            var SCHEDULE_SEATCAPACITY = await GetMasterConfigurableParameterValue("SCHEDULE_SEATCAPACITY");
            string AccomodationCapacity = "0";

            if (Convert.ToString(SCHEDULE_SEATCAPACITY).ToLower() == "yes")
            {
                AccomodationCapacity = Convert.ToString(await this._db.ILTSchedule.Where(a => a.ID == aPIILTRequestResponse.ScheduleID).Select(a => a.ScheduleCapacity).FirstOrDefaultAsync());
            }
            else
            {
                AccomodationCapacity = await this._db.TrainingPlace.Where(a => a.IsActive == true && a.IsDeleted == false && a.Id == PlaceID)
                                                                   .Select(ilt => ilt.AccommodationCapacity).FirstOrDefaultAsync();

            }


            int CountRegistered = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                               && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                               && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                               && a.CourseID == aPIILTRequestResponse.CourseID
                                                                               && a.TrainingRequestStatus == "Registered"
                                                                               && a.IsActiveNomination == true)
                                                                               .Select(n => n.ID).CountAsync();

            var requestResponseList = await _db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                       && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                       && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                       && a.CourseID == aPIILTRequestResponse.CourseID)
                                                                            .Select(d => new { Id = d.ID, TrainingRequesStatus = d.TrainingRequesStatus }).ToListAsync();

            int CountUnavailable = requestResponseList.Where(x => x.TrainingRequesStatus == "Unavailability").Select(d => d.Id).Count();
            int CountReject = requestResponseList.Where(x => x.TrainingRequesStatus == "Rejected").Select(d => d.Id).Count();
            int CountRequest = requestResponseList.Where(x => x.TrainingRequesStatus == "Requested").Select(d => d.Id).Count();

            ILTSchedule objILTSchedule = new ILTSchedule();

            objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == aPIILTRequestResponse.ScheduleID).FirstOrDefaultAsync();

            //if (objILTSchedule.EndDate.Date < DateTime.UtcNow.Date || (objILTSchedule.EndDate.Date == DateTime.UtcNow.Date && objILTSchedule.EndTime < DateTime.Now.TimeOfDay))
            //    return "Cannot change request as schedule " + objILTSchedule.ScheduleCode + " end date & time is over.";
            //else if (objILTSchedule.StartDate.Date < DateTime.UtcNow.Date || (objILTSchedule.StartDate.Date == DateTime.UtcNow.Date && objILTSchedule.StartTime < DateTime.Now.TimeOfDay))
            //    return "Cannot change request as schedule " + objILTSchedule.ScheduleCode + " is already started.";

            string venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
            string trainingName = this._db.Module.Where(a => a.Id == aPIILTRequestResponse.ModuleID).Select(a => a.Name).FirstOrDefault();
            string courseName = this._db.Course.Where(a => a.Id == aPIILTRequestResponse.CourseID).Select(a => a.Title).FirstOrDefault();
            string EmpoweredHost = null;
            try
            {
                EmpoweredHost = _db.EdCastConfiguration.Select(a => a.EmpoweredHost).FirstOrDefault();
                EmpoweredHost = EmpoweredHost + "/myCourseModule/" + objILTSchedule.CourseId;
            }
            catch
            {
            }

            ILTRequestResponse objILTResponse = Mapper.Map<ILTRequestResponse>(aPIILTRequestResponse);
            objILTResponse.UserID = UserId;
            objILTResponse.CreatedBy = UserId;
            objILTResponse.ModifiedBy = UserId;
            objILTResponse.CreatedDate = DateTime.UtcNow;
            objILTResponse.ModifiedDate = DateTime.UtcNow;
            objILTResponse.IsDeleted = false;
            objILTResponse.IsActive = true;
            if (aPIILTRequestResponse.TrainingRequesStatus.ToLower() == "requested")
            {
                if (PlaceID != 0 || Convert.ToInt32(AccomodationCapacity) != 0) // check for vilt 
                {
                    if (OrgCode.ToLower().Contains("wns"))
                    {
                        if (Convert.ToInt32(AccomodationCapacity) <= CountRegistered)
                        {
                            objILTResponse.TrainingRequesStatus = "Waiting";
                        }
                    }
                    else
                    {
                        if ((Convert.ToInt32(AccomodationCapacity)) <= ((CountRegistered + CountRequest) - (CountUnavailable + CountReject)))
                        {
                            objILTResponse.TrainingRequesStatus = "Waiting";
                        }
                    }
                }

                objILTResponse.ID = 0;
                await this.Add(objILTResponse);
            }

            string EndUserName = null, EndUserEmailId = null, EndUserMobileNumber = string.Empty;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetAllSupervisorsAndDirector";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.BigInt) { Value = aPIILTRequestResponse.UserID });
                        cmd.Parameters.Add(new SqlParameter("@Flag", SqlDbType.Int) { Value = 1 });


                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            connection.Close();
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            EndUserName = string.IsNullOrEmpty(Convert.ToString(row["EndUserName"])) ? null : Convert.ToString(row["EndUserName"]);
                            EndUserEmailId = string.IsNullOrEmpty(Convert.ToString(row["EndUserEmailId"])) ? null : Security.Decrypt(Convert.ToString(row["EndUserEmailId"]));
                            EndUserMobileNumber = string.IsNullOrEmpty(Convert.ToString(row["MobileNumber"])) ? null : Security.Decrypt(Convert.ToString(row["MobileNumber"]));
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }

            List<APINominateUserSMS> SMSList = new List<APINominateUserSMS>();
            var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_ILT");
            string urlSMS = string.Empty;

            if (aPIILTRequestResponse.TrainingRequesStatus.ToLower() == "approved")
            {
                if (PlaceID != 0 || Convert.ToInt32(AccomodationCapacity) != 0) // check for vilt 
                {
                    if (OrgCode.ToLower().Contains("wns"))
                    {
                        if (Convert.ToInt32(AccomodationCapacity) <= CountRegistered)
                        {
                            objILTResponse.TrainingRequesStatus = "Waiting";
                        }
                    }
                    else
                    {
                        if ((Convert.ToInt32(AccomodationCapacity)) <= ((CountRegistered + CountRequest) - (CountUnavailable + CountReject)))
                        {
                            objILTResponse.TrainingRequesStatus = "Waiting";
                        }
                    }
                }

                objILTResponse.ID = 0;
                objILTResponse.UserID = aPIILTRequestResponse.UserID;
                await this.Add(objILTResponse);

                // ---- Generate OTP --- //
                if (objILTResponse.TrainingRequesStatus.ToLower() == "approved")  // check added for wns schedule capacity exceeds at time of approval
                                                                                  //schedule capacity =1 -> selfe enroll by user1 , requested by user2 results in waiting for user2 
                                                                                  //nomination deleted by user1 from nomination tab User2 moved to requested status from waiting
                                                                                  //allows approve from schedule request even if capacity set to 1
                {
                    string Otp = null;
                    bool flag = false;

                    while (flag == false)
                    {
                        Otp = GenerateRandomPassword();
                        List<TrainingNomination> existingOTP = this._db.TrainingNomination.Where(a => a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                  && a.ModuleID == aPIILTRequestResponse.ModuleID && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                  && a.IsActive == true && a.IsDeleted == Record.NotDeleted && a.IsActiveNomination == true).ToList();
                        if (existingOTP != null)
                        {
                            flag = true;
                        }
                    }
                    // ---- Generate OTP --- //

                    if (!string.Equals(OrgCode, "canh", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(OrgCode, "tcns", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(OrgCode, "wnsuat", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(OrgCode, "wns", StringComparison.CurrentCultureIgnoreCase))
                    {
                        List<TrainingNomination> oldNominations = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true && a.CourseID == aPIILTRequestResponse.CourseID && a.ModuleID == aPIILTRequestResponse.ModuleID && a.UserID == aPIILTRequestResponse.UserID).ToListAsync();

                        foreach (TrainingNomination item in oldNominations)
                        {
                            item.IsActiveNomination = false;
                        }
                        _db.TrainingNomination.UpdateRange(oldNominations);
                        await _db.SaveChangesAsync();
                    }

                    TrainingNomination checkTrainingNomination = await _db.TrainingNomination.Where(a => a.ScheduleID == aPIILTRequestResponse.ScheduleID && a.UserID == aPIILTRequestResponse.UserID && a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true).FirstOrDefaultAsync();

                    if (checkTrainingNomination == null)
                    {
                        TrainingNomination obj = new TrainingNomination();
                        TrainingNomination lastRequestCode = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true).OrderByDescending(a => a.ID).FirstOrDefaultAsync();
                        if (lastRequestCode == null)
                        {
                            obj.RequestCode = "RQ1";
                        }
                        else
                        {
                            obj.RequestCode = "RQ" + (lastRequestCode.ID + 1);
                        }
                        obj.ScheduleID = aPIILTRequestResponse.ScheduleID;
                        obj.UserID = aPIILTRequestResponse.UserID;
                        obj.TrainingRequestStatus = "Registered";
                        obj.ModuleID = aPIILTRequestResponse.ModuleID;
                        obj.CourseID = aPIILTRequestResponse.CourseID;
                        obj.OTP = Otp;
                        obj.CreatedBy = UserId;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.ModifiedBy = UserId;
                        obj.ModifiedDate = DateTime.UtcNow;
                        obj.IsActive = true;
                        obj.IsDeleted = false;
                        obj.IsActiveNomination = true;

                        this._db.TrainingNomination.Add(obj);
                        this._db.SaveChanges();

                        if (objILTSchedule.WebinarType.ToLower() == "teams")
                        {
                            int id = await NominateTeamsLink(objILTSchedule, EndUserEmailId, EndUserName);
                        }
                        else if (objILTSchedule.WebinarType.ToLower() == "googlemeet")
                        {
                            int id = await NominateGsuitLink(objILTSchedule, EndUserEmailId, EndUserName);
                        }

                    }

                    // await _email.TrainingRequestFullyApprovedToUser(OrgCode, EndUserEmailId, null, null, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);
                    await _email.TrainingRequestApprovalMail(OrgCode, EndUserEmailId, null, null, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue, courseName, objILTSchedule.CourseId, EmpoweredHost);

                    string title = "Schedule Request";
                    string token = _identitySv.GetToken();
                    int UserIDToSend = UserId;
                    string Type = Record.Enrollment1;
                    string Message = "Your training request for '{ScheduleCode}' of '{ModuleName}' has been successfully approved.";
                    Message = Message.Replace("{ScheduleCode}", objILTSchedule.ScheduleCode);
                    Message = Message.Replace("{ModuleName}", trainingName);
                    await ScheduleRequestNotificationTo_Common(objILTResponse.CourseID, objILTResponse.ScheduleID, title, token, aPIILTRequestResponse.UserID, Message, Type);

                    if (string.Equals(OrgCode, "bandhan", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                        {
                            string CourseTitle = _db.Course.Where(x => x.Id == aPIILTRequestResponse.CourseID).Select(d => d.Title).FirstOrDefault();

                            APINominateUserSMS objSMS = new APINominateUserSMS();
                            objSMS.CourseTitle = CourseTitle;
                            objSMS.UserName = EndUserName;
                            objSMS.StartDate = objILTSchedule.StartDate;
                            objSMS.MobileNumber = !string.IsNullOrEmpty(EndUserMobileNumber) ? Security.Decrypt(EndUserMobileNumber) : null;
                            objSMS.organizationCode = OrgCode;
                            objSMS.UserID = aPIILTRequestResponse.UserID;
                            SMSList.Add(objSMS);

                            await this._email.SendScheduleRequestApproveNotificationSMS(SMSList);
                        }
                    }
                }
            }

            if (aPIILTRequestResponse.TrainingRequesStatus.ToLower() == "rejected")
            {
                ILTTrainingAttendance iLTTrainingAttendance = await _db.ILTTrainingAttendance.Where(x => x.ScheduleID == aPIILTRequestResponse.ScheduleID && x.UserID == aPIILTRequestResponse.UserID && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();

                if (iLTTrainingAttendance != null)
                    return "Cannot reject schedule request as attendance found for schedule " + objILTSchedule.ScheduleCode + ".";

                TrainingNomination trainingNomination = await _db.TrainingNomination.Where(x => x.ScheduleID == aPIILTRequestResponse.ScheduleID && x.UserID == aPIILTRequestResponse.UserID && x.IsActiveNomination == true && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                if (trainingNomination != null)
                {
                    trainingNomination.IsActive = false;
                    trainingNomination.IsDeleted = true;
                    _db.TrainingNomination.Update(trainingNomination);
                    await _db.SaveChangesAsync();
                }

                objILTResponse.UserID = aPIILTRequestResponse.UserID;
                objILTResponse.ID = 0;
                await this.Add(objILTResponse);

                ILTRequestResponse objILTRequestResponse = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.TrainingRequesStatus == "Waiting")
                                                                                                 .FirstOrDefaultAsync();
                if (objILTRequestResponse != null)
                {
                    ILTRequestResponse firstILTRequestResponseForWaiting = await this._db.ILTRequestResponse.AsNoTracking().Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                         && a.ModuleID == aPIILTRequestResponse.ModuleID
                                                                                                         && a.CourseID == aPIILTRequestResponse.CourseID
                                                                                                         && a.ScheduleID == aPIILTRequestResponse.ScheduleID
                                                                                                         && a.TrainingRequesStatus == "Waiting").OrderBy(a => a.ID)
                                                                                                 .FirstOrDefaultAsync();

                    firstILTRequestResponseForWaiting.IsActive = false;
                    firstILTRequestResponseForWaiting.IsDeleted = true;
                    await this.Update(firstILTRequestResponseForWaiting);

                    ILTRequestResponse objILTResponseRequest = new ILTRequestResponse();
                    objILTResponseRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    objILTResponseRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    objILTResponseRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    objILTResponseRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                    objILTResponseRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    objILTResponseRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                    objILTResponseRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                    objILTResponseRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                    objILTResponseRequest.TrainingRequesStatus = "Requested";
                    objILTResponseRequest.IsActive = true;
                    objILTResponseRequest.IsDeleted = false;
                    objILTResponseRequest.ID = 0;
                    await this.Add(objILTResponseRequest);
                }
                await _email.TrainingRequestRejectedMail(OrgCode, EndUserEmailId, null, null, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue, aPIILTRequestResponse.Reason);
                string title = "Schedule Request";
                string token = _identitySv.GetToken();
                int UserIDToSend = UserId;
                string Type = Record.Enrollment1;
                string Message = "Your training request for '{ScheduleCode}' of '{ModuleName}' has been rejected for the reason {Reason}.";
                Message = Message.Replace("{ScheduleCode}", objILTSchedule.ScheduleCode);
                Message = Message.Replace("{ModuleName}", trainingName);
                Message = Message.Replace("{Reason}", aPIILTRequestResponse.Reason);
                await ScheduleRequestNotificationTo_Common(objILTResponse.CourseID, objILTResponse.ScheduleID, title, token, aPIILTRequestResponse.UserID, Message, Type);

                if (string.Equals(OrgCode, "bandhan", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                    {
                        string CourseTitle = _db.Course.Where(x => x.Id == aPIILTRequestResponse.CourseID).Select(d => d.Title).FirstOrDefault();

                        APINominateUserSMS objSMS = new APINominateUserSMS();
                        objSMS.CourseTitle = CourseTitle;
                        objSMS.UserName = EndUserName;
                        objSMS.StartDate = objILTSchedule.StartDate;
                        objSMS.MobileNumber = !string.IsNullOrEmpty(EndUserMobileNumber) ? Security.Decrypt(EndUserMobileNumber) : null;
                        objSMS.organizationCode = OrgCode;
                        objSMS.UserID = aPIILTRequestResponse.UserID;
                        SMSList.Add(objSMS);

                        await this._email.SendScheduleRequestRejectNotificationSMS(SMSList);
                    }
                }
            }
            return "Success";
        }
        public async Task<int> NominateTeamsLink(ILTSchedule iLTSchedule, string emailId, string userName)
        {
            if (iLTSchedule != null)
            {
                Model.Course course = _db.Course.Where(a => a.Id == iLTSchedule.CourseId).FirstOrDefault();
                if (course != null)
                {

                    List<TeamsScheduleDetails> teamsScheduleDetailss = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID).ToList();
                    if (teamsScheduleDetailss != null)
                    {
                        foreach (TeamsScheduleDetails teamsScheduleDetails in teamsScheduleDetailss)
                        {
                            UserWebinarMaster userWebinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();
                            if (userWebinarMaster != null)
                            {

                                EventMeeting eventMeeting = new EventMeeting();
                                Start start = new Start();
                                End end = new End();

                                try
                                {
                                    eventMeeting.start = new Start();
                                    eventMeeting.end = new End();
                                    DateTime startDate = new DateTime(Convert.ToDateTime(teamsScheduleDetails.StartTime).Ticks);
                                    DateTime endDate = new DateTime(Convert.ToDateTime(teamsScheduleDetails.EndTime).Ticks);

                                    string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", startDate);
                                    string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", endDate);

                                    var sdate = startdate1.Substring(0, 11);
                                    var sdate1 = iLTSchedule.StartTime;
                                    var sdate2 = startdate1.Substring(17, 2);

                                    var edate = enddate1.Substring(0, 11);
                                    var edate1 = iLTSchedule.EndTime;
                                    var edate2 = enddate1.Substring(17, 2);

                                    eventMeeting.subject = course.Title;
                                    eventMeeting.start.dateTime = sdate + sdate1 + ":" + sdate2;
                                    eventMeeting.start.timeZone = "Asia/Kolkata";
                                    eventMeeting.end.dateTime = edate + edate1 + ":" + edate2;
                                    eventMeeting.end.timeZone = "Asia/Kolkata";
                                    eventMeeting.IsOnlineMeeting = true;
                                    eventMeeting.OnlineMeetingProvider = "teamsForBusiness";
                                    List<APIUserData> mailSendUser = new List<APIUserData>();
                                    List<APITrainingNomination> aPITrainingNomination = await GetNominateUserDetails(iLTSchedule.ID, course.Id, 1, 10000, "userName", null);
                                    if (aPITrainingNomination != null)
                                    {
                                        for (int i = 0; i < aPITrainingNomination.Count; i++)
                                        {
                                            APIUserData aPIUserData = new APIUserData();
                                            aPIUserData.emailId = aPITrainingNomination[i].EmailId;
                                            aPIUserData.userName = aPITrainingNomination[i].UserName;
                                            mailSendUser.Add(aPIUserData);
                                        }
                                    }

                                    List<ILTScheduleTrainerBindings> iLTScheduleTrainerBindings = this._db.ILTScheduleTrainerBindings
                                        .Where(a => a.ScheduleID == iLTSchedule.ID).ToList();

                                    foreach(ILTScheduleTrainerBindings iLTScheduleTrainerBindings1 in iLTScheduleTrainerBindings)
                                    {
                                        UserMaster userMaster = this._db.UserMaster.Where(a => a.Id == iLTScheduleTrainerBindings1.TrainerID).FirstOrDefault();
                                        APIUserData aPIUserData = new APIUserData();
                                        aPIUserData.emailId = Security.Decrypt(userMaster.EmailId);
                                        aPIUserData.userName = userMaster.UserName;
                                        mailSendUser.Add(aPIUserData);
                                    }

                                    eventMeeting.attendees = new Attendance[mailSendUser.Count];
                                    for (int i = 0; i < mailSendUser.Count; i++)
                                    {
                                        eventMeeting.attendees[i] = new Attendance();
                                        eventMeeting.attendees[i].emailAddress = new EmailAddress();
                                        eventMeeting.attendees[i].emailAddress.address = mailSendUser[i].emailId;
                                        eventMeeting.attendees[i].emailAddress.name = mailSendUser[i].userName;
                                        eventMeeting.attendees[i].status = new Status1();
                                        eventMeeting.attendees[i].type = "required";
                                    }

                                    JObject oJsonObject1 = new JObject();
                                    oJsonObject1 = JObject.Parse(JsonConvert.SerializeObject(eventMeeting));
                                    _logger.Error(teamsScheduleDetails.MeetingId);
                                    string baseUrl = "https://graph.microsoft.com/v1.0/users/" + userWebinarMaster.TeamsEmail + "/calendar/events/" + teamsScheduleDetails.MeetingId;
                                    AuthenticationResult results = await GetTeamsToken();
                                    if (results != null)
                                    {
                                        HttpResponseMessage Response = await ApiHelper.CallPatchAPIForTeams(baseUrl, oJsonObject1, results.AccessToken);
                                        TeamsEventResponse TeamsResponce = null;
                                        if (Response.IsSuccessStatusCode)
                                        {
                                            var result = Response.Content.ReadAsStringAsync().Result;

                                            TeamsResponce = JsonConvert.DeserializeObject<TeamsEventResponse>(result);

                                        }
                                    }
                                }

                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }


                    }

                }

            }
            return -3;
        }

        public async Task<int> NominateGsuitLink(ILTSchedule iLTSchedule, string emailId, string userName)
        {
            if (iLTSchedule != null)
            {
                List<EventAttendee> gsuitattendees = new List<EventAttendee>();
                if (!string.IsNullOrEmpty(emailId))
                {
                    EventAttendee att = new EventAttendee();
                    att.Email=emailId;
                    att.DisplayName = userName;
                    gsuitattendees.Add(att);
                }

                if (gsuitattendees.Count > 0)
                {
                    GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        googleMeetDetails = dbContext.GoogleMeetDetails.Where(a => a.ScheduleID == iLTSchedule.ID).FirstOrDefault();
                    }
                    UpdateGsuit updateGsuit = new UpdateGsuit();
                    updateGsuit.eventId = googleMeetDetails.MeetingId;
                    updateGsuit.Username = googleMeetDetails.OrganizerEmail;
                    EventAttendee[] allattendees = await _onlineWebinarRepository.CallGSuitUpdateEventCalendars(updateGsuit, gsuitattendees);
                }

            }
            return -3;
        }
        private IConfidentialClientApplication GetTeamsConfidentialClientApplication()
        {
            IConfidentialClientApplication app;
            try
            {
                ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "TEAMS").FirstOrDefault();

                string authority = iltonlineSettingteams.TeamsAuthority;
                string ClientId = iltonlineSettingteams.ClientID;
                string ClientSecret = iltonlineSettingteams.ClientSecret;

                app = ConfidentialClientApplicationBuilder.Create(ClientId)
                        .WithClientSecret(ClientSecret)
                        .WithAuthority(new Uri(authority))
                        .Build();
                return app;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<AuthenticationResult> GetTeamsToken()
        {
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            IConfidentialClientApplication app = GetTeamsConfidentialClientApplication();
            AuthenticationResult results = null;
            try
            {
                results = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return results;
        }
        public async Task<ILTSchedule> GetSchedulePurpose(int id)
        {
            ILTSchedule objILTSchedule = await _db.ILTSchedule.Where(x => x.ID == id).FirstOrDefaultAsync();
            return objILTSchedule;
        }
        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configurationCode });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = dt.Rows[0]["Value"].ToString();
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return value;
        }
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 6,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = false,
                RequireNonAlphanumeric = false,
                RequireUppercase = false
            };

            string[] randomChars = new[] {
        "0123456789",                   // digits
    };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
        public async Task<List<APIILTBatchRequests>> GetBatchRequests(int UserId, string RoleCode, int batchId, int page, int pageSize, string searchParameter = null, string searchText = null)
        {
            List<APIILTBatchRequests> aPIILTBatchRequestsList = new List<APIILTBatchRequests>();
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
                            cmd.CommandText = "[dbo].[GetAllBatchRequest]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@BatchId", SqlDbType.Int) { Value = batchId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = RoleCode });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@SearchParameter", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APIILTBatchRequests aPIILTBatchRequests = new APIILTBatchRequests();
                                aPIILTBatchRequests.RequestId = int.Parse(row["RequestId"].ToString());
                                aPIILTBatchRequests.BatchId = int.Parse(row["BatchId"].ToString());
                                aPIILTBatchRequests.BatchCode = row["BatchCode"].ToString();
                                aPIILTBatchRequests.BatchName = row["BatchName"].ToString();
                                aPIILTBatchRequests.CourseId = int.Parse(row["CourseID"].ToString());
                                aPIILTBatchRequests.CourseCode = row["CourseCode"].ToString();
                                aPIILTBatchRequests.CourseName = row["CourseName"].ToString();
                                aPIILTBatchRequests.StartDate = Convert.ToDateTime(row["StartDate"].ToString());
                                aPIILTBatchRequests.EndDate = Convert.ToDateTime(row["EndDate"].ToString());
                                aPIILTBatchRequests.StartTime = TimeSpan.Parse(row["StartTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTBatchRequests.EndTime = TimeSpan.Parse(row["EndTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTBatchRequests.UserMasterId = int.Parse(row["UserMasterId"].ToString());
                                aPIILTBatchRequests.UserId = Security.Decrypt(row["UserId"].ToString());
                                aPIILTBatchRequests.UserName = row["UserName"].ToString();
                                aPIILTBatchRequests.TrainingRequestStatus = row["TrainingRequestStatus"].ToString();
                                aPIILTBatchRequests.RequestedDate = Convert.ToDateTime(row["RequestedDate"].ToString());
                                if (!DBNull.Value.Equals(row["IsExpired"]))
                                    aPIILTBatchRequests.IsExpired = Convert.ToBoolean(row["IsExpired"]);
                                if (!DBNull.Value.Equals(row["Reason"]))
                                    aPIILTBatchRequests.Reason = row["Reason"].ToString();
                                aPIILTBatchRequestsList.Add(aPIILTBatchRequests);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return aPIILTBatchRequestsList;
        }
        public async Task<int> GetBatchRequestsCount(int UserId, string RoleCode, int batchId, string searchParameter = null, string searchText = null)
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
                            cmd.CommandText = "[dbo].[GetAllBatchRequest]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@BatchId", SqlDbType.Int) { Value = batchId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = RoleCode });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = 1 });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = 0 });
                            cmd.Parameters.Add(new SqlParameter("@SearchParameter", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

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
                                Count = string.IsNullOrEmpty(row["TotalRecords"].ToString()) ? 0 : int.Parse(row["TotalRecords"].ToString());
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
            return Count;
        }
        public async Task<List<APIILTRequestedBatches>> GetAllRequestedBatches(int UserId, string RoleCode, int page, int pageSize, string searchParameter = null, string searchText = null)
        {
            List<APIILTRequestedBatches> aPIILTRequestedBatchesList = new List<APIILTRequestedBatches>();
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
                            cmd.CommandText = "[dbo].[GetAllRequestedBatches]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = RoleCode });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@SearchParameter", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APIILTRequestedBatches aPIILTRequestedBatches = new APIILTRequestedBatches();
                                aPIILTRequestedBatches.BatchId = int.Parse(row["BatchId"].ToString());
                                aPIILTRequestedBatches.BatchCode = row["BatchCode"].ToString();
                                aPIILTRequestedBatches.BatchName = row["BatchName"].ToString();
                                aPIILTRequestedBatches.CourseCode = row["CourseCode"].ToString();
                                aPIILTRequestedBatches.CourseName = row["CourseName"].ToString();
                                aPIILTRequestedBatches.StartDate = Convert.ToDateTime(row["StartDate"].ToString());
                                aPIILTRequestedBatches.EndDate = Convert.ToDateTime(row["EndDate"].ToString());
                                aPIILTRequestedBatches.StartTime = TimeSpan.Parse(row["StartTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTRequestedBatches.EndTime = TimeSpan.Parse(row["EndTime"].ToString()).ToString(@"hh\:mm");
                                aPIILTRequestedBatches.Description = row["Description"].ToString();
                                aPIILTRequestedBatchesList.Add(aPIILTRequestedBatches);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return aPIILTRequestedBatchesList;
        }
        public async Task<int> GetAllRequestedBatchesCount(int UserId, string RoleCode, string searchParameter = null, string searchText = null)
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
                            cmd.CommandText = "[dbo].[GetAllRequestedBatches]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = RoleCode });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = 1 });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = 0 });
                            cmd.Parameters.Add(new SqlParameter("@SearchParameter", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

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
                                Count = string.IsNullOrEmpty(row["TotalRecords"].ToString()) ? 0 : int.Parse(row["TotalRecords"].ToString());
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
            return Count;
        }
        public async Task<List<APITrainingNomination>> GetNominateUserDetails(int id, int courseId, int page, int pageSize, string search = null, string searchText = null)
        {
            List<APITrainingNomination> TrainingNominationList = new List<APITrainingNomination>();
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
                            cmd.CommandText = "GetAllNominateUserDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
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
                                APITrainingNomination TrainingNomination = new APITrainingNomination();
                                TrainingNomination.ID = string.IsNullOrEmpty(row["AutoGenerateUserID"].ToString()) ? 0 : int.Parse(row["AutoGenerateUserID"].ToString());
                                TrainingNomination.ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString());
                                TrainingNomination.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? null : Security.Decrypt(row["UserId"].ToString());
                                TrainingNomination.UserName = row["UserName"].ToString();
                                TrainingNomination.EmailId = string.IsNullOrEmpty(row["EmailId"].ToString()) ? null : Security.Decrypt(row["EmailId"].ToString());
                                TrainingNomination.MobileNumber = string.IsNullOrEmpty(row["MobileNumber"].ToString()) ? null : Security.Decrypt(row["MobileNumber"].ToString());
                                TrainingNomination.Status = row["Status"].ToString();
                                TrainingNomination.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                TrainingNomination.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());

                                TrainingNominationList.Add(TrainingNomination);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return TrainingNominationList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

    }
}
