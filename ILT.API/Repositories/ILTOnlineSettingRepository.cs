using ILT.API.APIModel;
using ILT.API.Model.ILT;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ILT.API.Helper;
using log4net;


namespace ILT.API.Repositories
{
    public class ILTOnlineSettingRepository : Repository<ILTOnlineSetting>, IILTOnlineSetting
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTOnlineSettingRepository));
        private CourseContext _db;
        public ILTOnlineSettingRepository(CourseContext context) : base(context)
        {
            _db = context;
        }

        public async Task<List<APIILTOnlineSetting>> GetAllOnlineSetting(int page, int pageSize, string searchText)
        {
            try
            {
                var Query = (from iltonlinesetting in this._db.ILTOnlineSetting
                             select new APIILTOnlineSetting
                             {
                                 ID = iltonlinesetting.ID,
                                 UserID = iltonlinesetting.UserID,
                                 Password=iltonlinesetting.Password,
                                 ClientID= iltonlinesetting.ClientID,
                                 ClientSecret=iltonlinesetting.ClientSecret,
                                 RedirectUri=iltonlinesetting.RedirectUri,
                                 Type=iltonlinesetting.Type,
                                 TeamsAuthority = iltonlinesetting.TeamsAuthority
                             });

                Query = Query.Distinct().OrderByDescending(a => a.ID);

                if (!string.IsNullOrEmpty(searchText))
                    Query = Query.Where(a => a.UserID.Contains(searchText) || a.Type.Contains(searchText));

                if (page != -1)
                    Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
                if (pageSize != -1)
                    Query = Query.Take(Convert.ToInt32(pageSize));

                return Query.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetILTOnlineSettingCount(string searchText = null)
        {
            try
            {
                var Query = (from iltonlinesetting in this._db.ILTOnlineSetting
                             select new APIILTOnlineSetting
                             {
                                 ID = iltonlinesetting.ID,
                                 UserID = iltonlinesetting.UserID,
                                 Password = iltonlinesetting.Password,
                                 ClientID = iltonlinesetting.ClientID,
                                 ClientSecret = iltonlinesetting.ClientSecret,
                                 RedirectUri = iltonlinesetting.RedirectUri,
                                 Type = iltonlinesetting.Type
                             });
                if (!string.IsNullOrEmpty(searchText))
                    Query = Query.Where(a => a.UserID.Contains(searchText));

                return Query.Count();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> Exists(string name)
        {
            int Id = await (from c in _db.ILTOnlineSetting
                                    where  c.Type == name
                                    select c.ID).SingleOrDefaultAsync();
            return Id;
        }

       
    }
}
