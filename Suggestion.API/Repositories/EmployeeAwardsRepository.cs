using Suggestion.API.Data;
using Suggestion.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Suggestion.API.Helper;
using Suggestion.API.APIModel;
using Suggestion.API.Models;
using System.Globalization;

namespace Suggestion.API.Repositories
{
    public class EmployeeAwardsRepository : Repository<EmployeeAwards> , IEmployeeAwards
    {
        private GadgetDbContext _db;
        public EmployeeAwardsRepository(GadgetDbContext context) : base(context)
        {
            _db = context; 
        }

        public async Task<APIEmployeeAwardsListandCount> GetEmployeeAwards(int page, int pageSize, string search)
        {
            var Query = (from x in _db.EmployeeAwards
                         join um in _db.UserMaster on x.EmployeeId equals um.Id
                         join um1 in _db.AwardList on x.AwardId equals um1.Id
                         join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                         join loc in _db.Location on umd.LocationId equals loc.Id
                         into locjoin
                         from loc in locjoin.DefaultIfEmpty()
                         join area in _db.Area on umd.AreaId equals area.Id
                         into areajoin
                         from area in areajoin.DefaultIfEmpty()
                         select new EmployeeAwardsGet
                         {
                             Id = x.Id,
                             AwardId = x.AwardId,
                             CreatedDate = x.CreatedDate,
                             EmployeeName = um.UserName,
                             AwardName = um1.Title,
                             Month = x.Month,
                             Year = x.Year,
                             Remarks = x.Remarks,
                             EmployeeId = x.EmployeeId,
                             Code = um.UserId,
                             Location = loc.Name,
                             Area = area.Name
                         }).AsNoTracking();



            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.AwardName.Contains(search) || r.EmployeeName.Contains(search) || r.Month.Contains(search) || r.Year.ToString().Contains(search) || r.Remarks.Contains(search)));
            }

            APIEmployeeAwardsListandCount ListandCount = new APIEmployeeAwardsListandCount();
            Query = Query.OrderByDescending(a => a.CreatedDate);

            ListandCount.Count = Query.Distinct().Count();
            ListandCount.EmployeeAwardListandCount = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
            foreach (EmployeeAwardsGet item in ListandCount.EmployeeAwardListandCount)
            {
                item.Code = Security.Decrypt(item.Code);
            }

            return ListandCount;
        }

        public async Task<List<EmployeeAwardsGet>> GetEmployeeAwardsByUserId(int userId)
        {
            var Query = (from x in _db.EmployeeAwards
                         join um in _db.UserMaster on x.EmployeeId equals um.Id
                         join um1 in _db.AwardList on x.AwardId equals um1.Id
                         where um.Id == userId
                         orderby x.CreatedDate descending
                         select new EmployeeAwardsGet
                         {
                             AwardId = x.AwardId,
                             CreatedDate = x.CreatedDate,
                             EmployeeName = um.UserName,
                             AwardName = um1.Title,
                             Month = x.Month,
                             Year = x.Year,
                             EmployeeId = x.EmployeeId,
                             FilePath = um1.FilePath
                         }).AsNoTracking();


           

           UserMaster user = _db.UserMaster.Where(a => a.Id == userId).FirstOrDefault();
           Area project = (from x in _db.UserMasterDetails
                        join area in _db.Area on x.AreaId equals area.Id
                        into ara
                        from ar in ara.DefaultIfEmpty()
                        where x.UserMasterId == userId
                        select new Area
                        {
                            Name = ar.Name
                        }
                      ).AsNoTracking().FirstOrDefault();

           List<EmployeeAwardsGet> EmployeeAwards = Query.ToList();
            
            foreach(EmployeeAwardsGet item in EmployeeAwards)
            {
                item.UserId = Security.Decrypt(user.UserId);
                item.ProjectName = project.Name;
            }
            return  EmployeeAwards;
        }

        public async Task<List<EmployeeAwardsGet>> BestAwardDashboard(APIFilter filterData)
        {
            var Query = (from x in _db.EmployeeAwards
                         join um in _db.UserMaster on x.EmployeeId equals um.Id
                         join um1 in _db.AwardList on x.AwardId equals um1.Id
                         join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                         join location in _db.Location on umd.LocationId equals location.Id
                         into locjoin
                         from loc in locjoin.DefaultIfEmpty()
                         join busi in _db.Business on umd.BusinessId equals busi.Id
                         into busijoin
                         from bus in busijoin.DefaultIfEmpty()
                         join area in _db.Area on umd.AreaId equals area.Id
                         into areajoin
                         from area in areajoin.DefaultIfEmpty()
                         select new EmployeeAwardsGet
                         {
                             Id = x.Id,
                             UserId = Security.Decrypt(um.UserId),
                             AwardId = x.AwardId,
                             CreatedDate = x.CreatedDate,
                             EmployeeName = um.UserName,
                             AwardName = um1.Title,
                             Month = x.Month,
                             Year = x.Year,
                             Remarks = x.Remarks,
                             EmployeeId = x.EmployeeId,
                             Code = um.UserId,
                             Cluster = bus.Name,
                             ClusterId = umd.BusinessId,
                             ProjectId = umd.AreaId,
                             Area = area.Name,
                             Location = loc.Name,
                             Gender = umd.Gender,
                             ProfilePicture = umd.ProfilePicture
                         }).AsNoTracking();

            if (filterData.Cluster != null)
            {
                Query = Query.Where(a => a.ClusterId == filterData.Cluster);
            }
            if (filterData.Project != null)
            {
                Query = Query.Where(a => a.ProjectId == filterData.Project);
            }
            if (filterData.Month != null)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName((int)filterData.Month);
                Query = Query.Where(a => a.Month.ToLower() == monthName);
            }
            if (filterData.Year != null)
            {
                Query = Query.Where(a => a.Year == filterData.Year);
            }

            List<EmployeeAwardsGet> employeeAwards = await Query.ToListAsync();
            return employeeAwards;
        }

        public string CheckAwardExist(APIEmployeeAwards data)
        {
            List<EmployeeAwards> existingData = _db.EmployeeAwards.Where(x => (x.AwardId == data.AwardId && x.Month == data.Month && x.Year == data.Year)).ToList();
            if (existingData.Count > 0)
            {
                foreach (EmployeeAwards item in existingData)
                {
                    int? areaId = _db.UserMasterDetails.Where(a => a.UserMasterId == item.EmployeeId).Select(a => a.AreaId).FirstOrDefault();
                    int? dataAreaId = _db.UserMasterDetails.Where(a => a.UserMasterId == data.EmployeeId).Select(a => a.AreaId).FirstOrDefault();
                    if (areaId == dataAreaId)
                    {
                        return "Yes";
                    }
                }
            }
            return "No";
        }
    }
}

