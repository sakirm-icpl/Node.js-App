using ILT.API.APIModel;
//using ILT.API.APIModel;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Model.ILT;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ILT.API.Repositories
{
    public class TrainingPlaceRepository : Repository<TrainingPlace>, ITrainingPlaceRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TrainingPlaceRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        public TrainingPlaceRepository(ICustomerConnectionStringRepository customerConnection, CourseContext context) : base(context)
        {
            _db = context;
            _customerConnection = customerConnection;
        }
        public async Task<bool> Exists(string name)
        {
            if (await _db.TrainingPlace.CountAsync(y => (y.PlaceCode == name && y.IsDeleted == false)) > 0)
                return true;
            return false;
        }

        public async Task<bool> NameCodeExists(string name)
        {
            if (await _db.TrainingPlace.CountAsync(y => (y.PlaceName == name && y.IsDeleted == false)) > 0)
                return true;

            return false;
        }

        public async Task<List<TrainingPlaceTypeAhead>> GetTrainingPlaceTypeAhead(string search)
        {
            IQueryable<TrainingPlace> Query = _db.TrainingPlace.Where(r => r.IsDeleted == Record.NotDeleted && r.IsActive == true);
           if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.PlaceCode.Contains(search) || r.PlaceName.Contains(search));

            }
            return await Query.Select(r => new TrainingPlaceTypeAhead { Id = r.Id, PlaceName = r.PlaceName, PlaceCode = r.PlaceCode, Cityname = r.Cityname }).ToListAsync();
        }
        public async Task<List<TrainingPlace>> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            ValidUserInfo objValidUserInfo = new ValidUserInfo();
            IQueryable<TrainingPlace> Query = _db.TrainingPlace.Where(x=>x.IsDeleted == Record.NotDeleted);
            List<TrainingPlace> PlaceList = new List<TrainingPlace>();

            //foreach (TrainingPlace obj in Query)
            //{
            //    objValidUserInfo = await ValidUserInfo(obj.UserID);
            //    obj.ContactNumber = objValidUserInfo.MobileNumber;
            //    obj.ContactPerson = objValidUserInfo.UserName;
                PlaceList.AddRange(Query.ToList());
            //}

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(columnName))
            {
                if (columnName.ToLower().Equals("cityname"))
                    PlaceList = PlaceList.Where(r => r.Cityname.ToLower().Contains(search.ToLower())).ToList();
                if (columnName.ToLower().Equals("placename"))
                    PlaceList = PlaceList.Where(r => r.PlaceName.ToLower().Contains(search.ToLower())).ToList();
            }
            PlaceList = PlaceList.OrderByDescending(r => r.Id).ToList();

            if (page != -1)
                PlaceList = PlaceList.Skip((page - 1) * pageSize).ToList();

            if (pageSize != -1)
                PlaceList = PlaceList.Take(pageSize).ToList();

            return PlaceList;
        }
        public async Task<int> count(string search = null, string columnName = null)
        {
            IQueryable<TrainingPlace> Query = _db.TrainingPlace.Where(x=>x.IsDeleted==Record.NotDeleted);

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(columnName))
            {
                if (columnName.ToLower().Equals("cityname"))
                    Query = Query.Where(r => r.Cityname.ToLower().Contains(search.ToLower()));
                if (columnName.ToLower().Equals("placename"))
                    Query = Query.Where(r => r.PlaceName.ToLower().Contains(search.ToLower()));
            }

            return await Query.CountAsync();
        }

        public async Task<int> GetTrainingPlaceCount()
        {
            return await this._db.TrainingPlace.CountAsync();
        }

        public async Task<int> UpdateTrainingPlace(int id, TrainingPlace trainingPlace, int UserId)
        {
            TrainingPlace trainingplaceRecord = new TrainingPlace();
            trainingplaceRecord.PlaceCode = trainingPlace.PlaceCode;
            trainingplaceRecord.Cityname = trainingPlace.Cityname;
            trainingplaceRecord.PlaceName = trainingPlace.PlaceName;
            trainingplaceRecord.PostalAddress = trainingPlace.PostalAddress;
            trainingplaceRecord.TimeZone = trainingPlace.TimeZone;
            trainingplaceRecord.AccommodationCapacity = trainingPlace.AccommodationCapacity;
            trainingplaceRecord.Facilities = trainingPlace.Facilities;
            trainingplaceRecord.PlaceType = trainingPlace.PlaceType;
            trainingplaceRecord.UserID = trainingPlace.UserID;
            trainingplaceRecord.ContactNumber = trainingPlace.ContactNumber;
            trainingplaceRecord.AlternateContact = trainingPlace.AlternateContact;
            trainingplaceRecord.ContactPerson = trainingPlace.ContactPerson;
            trainingplaceRecord.Status = trainingPlace.Status;
            trainingplaceRecord.IsActive = trainingPlace.IsActive;
            trainingplaceRecord.ModifiedDate = DateTime.UtcNow;
            trainingplaceRecord.ModifiedBy = UserId;
            await this.Update(trainingPlace);
            return 1;
        }


        public async Task<int> DeleteTrainingPlace(int id, int UserId)

        {
            List<ILTSchedule> ifExistsTrainingPlace = this._db.ILTSchedule.Where(a => a.PlaceID == id && a.IsActive == Record.Active && a.IsDeleted == false).ToList();
            if (ifExistsTrainingPlace.Count != 0)
            {
                return 0;
            }
            return 1;
        }

        public async Task<ApiResponse> CheckForValidInfo(int UserID, string MobileNumber, string ContactPerson)
        {
            ApiResponse Response = new ApiResponse();

            ValidUserInfo objValidUserInfo = new ValidUserInfo();
            objValidUserInfo = await ValidUserInfo(UserID);

            if ((objValidUserInfo.MobileNumber != MobileNumber) || (objValidUserInfo.UserName != ContactPerson))
            {
                Response.Description = "Invalid Contact Information";
                Response.StatusCode = 409;
                return Response;
            }
            return Response;
        }

        public async Task<ValidUserInfo> ValidUserInfo(int UserID)
        {
            ValidUserInfo objValidUserInfo = new ValidUserInfo();
            //--------- Check for Valid UserInfo --------------//
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
                            cmd.CommandText = "GetForValidUserInfo";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.NVarChar) { Value = UserID });
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
                                objValidUserInfo.MobileNumber = string.IsNullOrEmpty(row["MobileNumber"].ToString()) ? null : Security.Decrypt(row["MobileNumber"].ToString());
                                objValidUserInfo.UserName = (row["UserName"].ToString());
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
            //--------- Check for Valid UserInfo --------------//
            return objValidUserInfo;
        }

    }
}
