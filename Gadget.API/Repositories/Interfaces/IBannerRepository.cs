using Gadget.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IBannerRepository : IRepository<Banner>
    {
        Task<List<object>> GetAll(int page, int pageSize, bool? status = null, string search = null);
        Task<List<object>> GetActiveBanners();
        Task<int> Count(bool? status = null, string search = null);
        Task<object> GetBannerById(int BannerId);
        Task<bool> NameExists(string name, int? bannerId = null);
        Task<bool> ShowBannerOnLandingPage();
        Task<int> TotalCount();
        Task<List<Banner>> UpdateBannerSequence(List<BannerPayload> banners, int userId);
    }
}
