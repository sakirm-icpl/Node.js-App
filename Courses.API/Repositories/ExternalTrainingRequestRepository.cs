using Course.API.Model;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using Courses.API.Helper;
using Newtonsoft.Json;

namespace Courses.API.Repositories
{

    public class ExternalTrainingRequestRepository : Repository<ExternalTrainingRequest>, IExternalTrainingRequest
    {
        private CourseContext _db;
        private IConfiguration _configuration;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ExternalTrainingRequestRepository));
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
        public ExternalTrainingRequestRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _db = context;
            _configuration = configuration;
            _customerConnectionRepository = customerConnectionRepository;

        }

        public async Task<ExternalTrainingRequest> PostExternalTrainingRequest(ExternalTrainingRequest data, int UserId)
        {
            ExternalTrainingRequest lastEntry = (from x in _db.ExternalTrainingRequest
                                                 orderby x.CreatedDate descending
                                                 select x).First();
            string reqCodeId = "";
            if (lastEntry != null)
            {
                lastEntry.Id = lastEntry.Id++;
                reqCodeId = lastEntry.Id.ToString();
            }
            else
            {
                reqCodeId = "1";
            }
            data.ModifiedBy = UserId;
            data.CreatedBy = UserId;
            data.CreatedDate = DateTime.Now;
            int year = data.CreatedDate.Year;
            string yr = year.ToString().Substring(2);
            data.RequestCode = "ETR-"+yr+"-"+reqCodeId;
            data.ModifiedDate = DateTime.Now;
            data.IsActive = true;
            data.IsDeleted = false;
            data.Status = "Pending";

            await this._db.ExternalTrainingRequest.AddAsync(data);
            await this._db.SaveChangesAsync();

            return data;
        }

        public async Task<ExternalTrainingRequestListandCount> GetExternalTrainingRequest(int page, int pageSize, string search,int UserId)
        {
            var Query = (from x in _db.ExternalTrainingRequest
                         where x.CreatedBy == UserId
                         orderby x.CreatedDate descending
                         select new ExternalTrainingRequestAllUser
                         {
                             Id = x.Id,
                             RequestCode = x.RequestCode,
                             CreatedDate = x.CreatedDate,
                             Title = x.Title,
                             Status = x.Status,
                             StartDate = x.StartDate,
                             Traveling = x.Traveling,
                             EndDate = x.EndDate,
                             Trainer = x.Trainer,
                             ContentUrl =x.ContentUrl,
                             Fee = x.Fee,
                             Currency =x.Currency,
                             Reason = x.Reason
                         }).AsNoTracking();



            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.Title.Contains(search)));
            }

            

            ExternalTrainingRequestListandCount ListandCount = new ExternalTrainingRequestListandCount();
            ListandCount.Count = Query.Distinct().Count();

            ListandCount.RequestList = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
          
            foreach(ExternalTrainingRequestAllUser user in ListandCount.RequestList)
            {
                user.Days = (user.EndDate - user.StartDate).Days +1;
            }


            string GST_Rate = await GetMasterConfigurableParameterValue("GST_Rate");
            double gstRate = Convert.ToDouble(GST_Rate);
            foreach(ExternalTrainingRequestAllUser reqest in ListandCount.RequestList)
            {
                reqest.GST = gstRate;
                reqest.NetAmount = gstRate + reqest.Fee;
            }


            return ListandCount;
        }

        public async Task<ExternalTrainingRequestListandCountAllUser> GetExternalTrainingRequestAllUser(int page, int pageSize, string searchBy, string search,int userId)
        {
            string role = GetUserRole(userId);

            if (role.ToLower() == "ca" || role.ToLower() == "ga" || role.ToLower() == "sp" || role.ToLower() == "la" || role.ToLower() == "ba")
            {

                var Query = (from x in _db.ExternalTrainingRequest
                             join um in _db.UserMaster on x.CreatedBy equals um.Id
                             orderby x.CreatedDate descending
                             select new ExternalTrainingRequestAllUser
                             {
                                 Id = x.Id,
                                 RequestCode = x.RequestCode,
                                 UserName = um.UserName,
                                 CreatedDate = x.CreatedDate,
                                 Title = x.Title,
                                 Status = x.Status,
                                 StartDate = x.StartDate,
                                 EndDate = x.EndDate,
                                 Traveling = x.Traveling,
                                 UserId = um.Id,
                                 Currency = x.Currency,
                                 Trainer = x.Trainer,
                                 Reason = x.Reason,
                                 ContentUrl = x.ContentUrl,
                                 Fee = x.Fee
                             }).AsNoTracking();

                if (role.ToLower() == "ga")
                {
                    string roleLimit = await GetMasterConfigurableParameterValue("LIMIT_GA");
                    double roleLmt = Convert.ToDouble(roleLimit);
                    Query = Query.Where(r => (r.Fee <= roleLmt));
                }

                if (role.ToLower() == "la")
                {
                    string roleLimit = await GetMasterConfigurableParameterValue("LIMIT_LA");
                    double roleLmt = Convert.ToDouble(roleLimit);

                    Query = Query.Where(r => (r.Fee <= (roleLmt)));
                }

                if (role.ToLower() == "sp")
                {
                    string roleLimit = await GetMasterConfigurableParameterValue("LIMIT_SUPERVISOR");
                    double roleLmt = Convert.ToDouble(roleLimit);
                    Query = Query.Where(r => r.Fee <= roleLmt);
                }

                if (role.ToLower() == "ba")
                {
                    string roleLimit = await GetMasterConfigurableParameterValue("LIMIT_DeptAdmin");
                    double roleLmt = Convert.ToDouble(roleLimit);
                    Query = Query.Where(r => r.Fee <= roleLmt);
                }

                



                if (!string.IsNullOrEmpty(search))
                {
                    if (searchBy.ToLower() == "title")
                    {
                        Query = Query.Where(r => (r.Title.Contains(search)));
                    }

                    if (searchBy.ToLower() == "username")
                    {
                        Query = Query.Where(r => (r.UserName.Contains(search)));
                    }

                    if (searchBy.ToLower() == "status")
                    {
                        Query = Query.Where(r => (r.Status.Contains(search)));
                    }

                }


                ExternalTrainingRequestListandCountAllUser ListandCount = new ExternalTrainingRequestListandCountAllUser();
                ListandCount.Count = Query.Distinct().Count();

                ListandCount.RequestList = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
                foreach (ExternalTrainingRequestAllUser user in ListandCount.RequestList)
                {
                    user.Days = (user.EndDate - user.StartDate).Days + 1;
                }

                return ListandCount;
            }
            else
            {
                return null;
            }
        }


        public async Task<ExternalTrainingRequestEdit> GetExternalTrainingRequestEdit(int reqId,int UserId)
        {
            var Query = (from x in _db.ExternalTrainingRequest
                         join um in _db.UserMaster on x.CreatedBy equals um.Id
                         where x.CreatedBy == UserId && x.Id == reqId
                         orderby x.CreatedDate descending
                         select new ExternalTrainingRequestEdit
                         {
                             Id = x.Id,
                             Fee = x.Fee,
                             RequestCode = x.RequestCode,
                             UserName = um.UserName,
                             CreatedDate = x.CreatedDate,
                             Title = x.Title,
                             Status = x.Status,
                             StartDate = x.StartDate,
                             Traveling = x.Traveling,
                             Currency  = x.Currency,
                             Trainer = x.Trainer,
                             Reason = x.Reason
                         }).AsNoTracking();


           
            ExternalTrainingRequestEdit data =  await Query.FirstOrDefaultAsync();

           
            string GST_Rate = await GetMasterConfigurableParameterValue("GST_Rate");
            string SUBSIDY_Rate = await GetMasterConfigurableParameterValue("SUBSIDY_Rate");
            string BASE_CURRENCY = await GetMasterConfigurableParameterValue("BASE_CURRENCY");
            double conversionRate = 0;

            string url = "https://api.exchangerate.host/convert?from="+ BASE_CURRENCY + "&to="+data.Currency;
            //string url = "https://api.exchangerate.host/convert?from=USD&to=INR";

            HttpResponseMessage response = await ApiHelper.CallGetAPI(url);
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                CurrencyConversion ConfigurableParameters = JsonConvert.DeserializeObject<CurrencyConversion>(result);
                conversionRate = ConfigurableParameters.Result;
                conversionRate = Math.Round(conversionRate, 2);
            }

         
            double gstRate = Convert.ToDouble(GST_Rate);
            double subsidyRate = Convert.ToDouble(SUBSIDY_Rate);

            data.Subsidy = (data.Fee * subsidyRate*conversionRate) / 100;
            data.Subsidy = Math.Round(data.Subsidy, 2);
            data.GST = (data.Fee * gstRate*conversionRate) / 100;
            data.GST = Math.Round(data.GST, 2);
            data.Converted = (data.Fee * conversionRate);
            data.NetOutFlow = data.Converted - data.Subsidy;

            return data;
        }


        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnectionRepository.GetDbContext())
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


        public string GetUserRole(int userId)
        {
            UserMaster role = _db.UserMaster.Where(x => x.Id == userId).FirstOrDefault();

            string userRole = role.UserRole;
            return userRole;
        }





    }


}
