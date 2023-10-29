using Gadget.API.Data;
using Gadget.API.Helper;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories
{
    public class BannerRepository : Repository<Banner>, IBannerRepository
    {
        private GadgetDbContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BannerRepository));
        public BannerRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            this._db = context;
            this._customerConnection = customerConnection;
        }

        public async Task<List<object>> GetAll(int page, int pageSize, bool? status = null, string search = null)
        {
            var Query = (from banner in _db.Banner
                         where banner.IsDeleted == false
                         select banner);

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Name.Contains(search));
            }

            if (status != null)
                Query = Query.Where(r => r.IsActive == status);

            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);

            List<Banner> BannerList = await Query.OrderBy(r => r.BannerNumber).ToListAsync();

            int NewBannerNumber = 1;
            List<object> Result = new List<object>();
            foreach (Banner item in BannerList)
            {
                item.BannerNumber = NewBannerNumber;
                object bannerobj = new
                {
                    item.Id,
                    item.Name,
                    item.BannerType,
                    item.ThumbnailImage,
                    item.Path,
                    item.IsActive,
                    item.BannerNumber
                };
                Result.Add(bannerobj);

                NewBannerNumber += 1;
            }
            return Result;
        }

        public async Task<List<object>> GetActiveBanners()
        {
            var Query = (from banner in _db.Banner
                         where banner.IsDeleted == false && banner.IsActive == true
                         select banner);

            List<Banner> BannerList = await Query.OrderBy(r => r.BannerNumber).ToListAsync();

            int NewBannerNumber = 1;
            List<object> Result = new List<object>();
            foreach (Banner item in BannerList)
            {
                item.BannerNumber = NewBannerNumber;
                object bannerobj = new
                {
                    item.Id,
                    item.Name,
                    item.BannerType,
                    item.ThumbnailImage,
                    item.Path,
                    item.BannerNumber
                };
                Result.Add(bannerobj);

                NewBannerNumber += 1;
            }
            return Result;
        }

        public async Task<int> Count(bool? status = null, string search = null)
        {
            var Query = _db.Banner.Where(r => r.IsDeleted == Record.NotDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Name.Contains(search));
            }

            if (status != null)
                Query = Query.Where(r => r.IsActive == status);

            return await Query.CountAsync();
        }

        public async Task<object> GetBannerById(int BannerId)
        {
            var Query = _db.Banner.Where(r => r.IsDeleted == Record.NotDeleted && r.Id == BannerId);
            Banner banner = await Query.FirstOrDefaultAsync();
            return banner;
        }

        public async Task<bool> NameExists(string Name, int? BannerId = null)
        {
            Name = Name.ToLower().Trim();
            int Count = 0;

            if (BannerId != null)
            {
                Count = await (from c in this._db.Banner
                               where c.Id != BannerId && c.IsDeleted == false && (c.Name.ToLower().Equals(Name))
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this._db.Banner
                               where c.IsDeleted == false && (c.Name.ToLower().Equals(Name))
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;
        }
        
        public async Task<bool> ShowBannerOnLandingPage()
        {
            int Count = 0;
            try
            {
                Count = await (from c in this._db.ConfigurableParameter
                               where c.IsDeleted == false && c.Code == "BANNER" && c.Value == "Yes"
                               select c).CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            if (Count > 0)
                return true;
            return false;
        }

        public async Task<int> TotalCount()
        {
            var Query = _db.Banner;
            return await Query.CountAsync();
        }

        public async Task<List<Banner>> UpdateBannerSequence(List<BannerPayload> bannersList, int userId)
        {
            try
            {
                List<Banner> banner = await this._db.Banner.Where(r => r.IsDeleted == Record.NotDeleted).ToListAsync();

                foreach (var page in bannersList)
                {
                    Banner updatePage = new Banner();
                    updatePage = banner.Find(x => x.Name == page.Name);
                    updatePage.BannerNumber = page.BannerNumber;
                    updatePage.ModifiedBy = userId;
                    updatePage.ModifiedDate = DateTime.UtcNow;
                    _db.Banner.Update(updatePage);
                    await this._db.SaveChangesAsync();
                }
                return banner;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
    }
}
