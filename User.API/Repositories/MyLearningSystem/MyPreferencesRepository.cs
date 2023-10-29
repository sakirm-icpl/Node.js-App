//======================================
// <copyright file="MyPreferencesRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using log4net;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel.MyLearningSystem;
using User.API.Data;
using User.API.Helper;
using User.API.Models.MyLearningSystem;
using User.API.Repositories.Interfaces.MyLearningSystem;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories.MyLearningSystem
{
    public class MyPreferencesRepository : Repository<MyPreferences>, IMyPreferencesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MyPreferencesRepository));
        private UserDbContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        public MyPreferencesRepository(UserDbContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            this._db = context;
            this._customerConnection = customerConnection;
        }
        public async Task<bool> Exist(int userId)
        {

            var count = await this._db.MyPreferences.Where(r => r.CreatedBy == userId).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<APIMyPreferences> GetMyPreferenceByToken(int userid)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from myPreferences in context.MyPreferences

                                  where (myPreferences.IsDeleted == Record.NotDeleted && myPreferences.CreatedBy == userid)
                                  select new APIMyPreferences
                                  {
                                      Id = myPreferences.Id,
                                      Profile = myPreferences.Profile,
                                      Status = myPreferences.Status,
                                      Language = myPreferences.Language,
                                      LandingPage = myPreferences.LandingPage,
                                      Code = myPreferences.Code,
                                  });


                    return await result.AsNoTracking().SingleOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;


        }
        public async Task<bool> IsLanguageExists(string Language)
        {
            bool value = false;
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "IsLanguageExists";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Language", SqlDbType.NVarChar) { Value = Language });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = string.IsNullOrEmpty(dt.Rows[0]["Value"].ToString()) ? false : Convert.ToBoolean(dt.Rows[0]["Value"].ToString());
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return value;
        }


        public async Task<List<MyPreferenceConfiguration>> GetLandingPages()
        {
            List<MyPreferenceConfiguration> myPreferenceConfiguration = await this._db.MyPreferenceConfigurations.ToListAsync();
            return myPreferenceConfiguration;
        }

        public async Task<List<MyPreferenceConfiguration>> UpdateLandingPageConfiguration(List<MyPreferenceConfiguration> myPreferenceConfigurations)
        {
            foreach (var preference in myPreferenceConfigurations)
            {
                MyPreferenceConfiguration myPreference = await this._db.MyPreferenceConfigurations.Where(s => s.Code == preference.Code).FirstOrDefaultAsync();

                if (myPreference != null)
                {
                    myPreference.IsActive = preference.IsActive;
                    this._db.MyPreferenceConfigurations.Update(myPreference);
                    this._db.SaveChanges();
                }
            }
            return myPreferenceConfigurations;
        }

    }
}
