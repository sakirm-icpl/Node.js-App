using Suggestion.API.Data;
using Suggestion.API.Repositories.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Suggestion.API.Models;
using Suggestion.API.APIModel;
using Microsoft.AspNetCore.Mvc;
using Suggestion.API.Helper;
using System.Data;

namespace Suggestion.API.Repositories
{
    public class EmployeeSuggestionsRepository : Repository<EmployeeSuggestions>, IEmployeeSuggestions
    {
        private GadgetDbContext _db;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DigitalAdoptionReviewListRepository));
        public EmployeeSuggestionsRepository(GadgetDbContext context) : base(context)
        {
            _db = context;
        }


        public async Task<APIEmployeeSuggestionsListandCount> GetEmployeeSuggestions(int page, int pageSize, int userId, string searchBy, string search)
        {
            try
            {
                List<APIEmployeeSuggestionsGet> employeeSuggestions = new List<APIEmployeeSuggestionsGet>();
                var Query = (from x in _db.EmployeeSuggestions
                             join cat in _db.SuggestionCategory on x.Category equals cat.Id
                             join user in _db.UserMaster on x.EmployeeId equals user.Id
                             join umd in _db.UserMasterDetails on user.Id equals umd.UserMasterId
                             join area in _db.Area on umd.AreaId equals area.Id
                             into arealeftjoin
                             from alj in arealeftjoin.DefaultIfEmpty()
                             join business in _db.Business on umd.BusinessId equals business.Id
                             into businessleftjoin
                             from bus in businessleftjoin.DefaultIfEmpty()
                             where x.IsDeleted == false 
                             select new APIEmployeeSuggestionsGet
                             {
                                 AdditionalDescription = x.AdditionalDescription,
                                 AreaName = alj.Name,
                                 Business = bus.Name,
                                 Category = x.Category,
                                 CreatedDate = x.CreatedDate,
                                 EmployeeName = user.UserName,
                                 Id = x.Id,
                                 IsActive = x.IsActive,
                                 Suggestion = x.Suggestion,
                                 SuggestionsCategory = cat.SuggestionsCategory,
                                 Status = "Pending"
                             }).AsNoTracking();

                employeeSuggestions = Query.ToList();

                // List<FilesAndLikesCount> filesCount = _db.EmployeeSuggestionFile.Where(a => a.IsDeleted == false).GroupBy(a => a.SuggestionId).Select(c => new FilesAndLikesCount { Count = c.Count(), SuggestionId = c.Key }).ToList();
                List<FilesAndLikesCount> filesCount = (from esf in _db.EmployeeSuggestionFile
                                                       join um in _db.UserMaster on esf.CreatedBy equals um.Id
                                                       where esf.IsDeleted == false
                                                       group esf by esf.SuggestionId into suggFileGroup
                                                       select new FilesAndLikesCount
                                                       {
                                                           Count = suggFileGroup.Count(),
                                                           SuggestionId = suggFileGroup.Key
                                                       }).ToList();
                foreach (FilesAndLikesCount item in filesCount)
                {
                    APIEmployeeSuggestionsGet data1 = employeeSuggestions.Where(a => a.Id == item.SuggestionId).FirstOrDefault();
                    data1.Files = item.Count;
                }

               // List<FilesAndLikesCount> likesCount = _db.EmployeeSuggestionLike.Where(a => a.IsDeleted == false).GroupBy(a => a.SuggestionId).Select(c => new FilesAndLikesCount { Count = c.Count(), SuggestionId = c.Key }).ToList();
                List<FilesAndLikesCount> likesCount = (from esl in _db.EmployeeSuggestionLike
                                                       join um in _db.UserMaster on esl.ReviewerId equals um.Id
                                                       where esl.IsDeleted == false
                                                       group esl by esl.SuggestionId into suggGroup
                                                       select new FilesAndLikesCount
                                                       {
                                                           Count = suggGroup.Count(),
                                                           SuggestionId = suggGroup.Key
                                                       }).ToList();
                foreach (FilesAndLikesCount item in likesCount)
                { 
                    APIEmployeeSuggestionsGet data1 = employeeSuggestions.Where(a => a.Id == item.SuggestionId).FirstOrDefault();
                    int like = 0, dislike = 0;
                    List<EmployeeSuggestionLike> likedata = _db.EmployeeSuggestionLike.Where(a => a.SuggestionId == item.SuggestionId).ToList();
                    foreach (EmployeeSuggestionLike item1 in likedata)
                    {
                        if (item1.Status == true)
                        {
                          like++;
                        }
                        else
                        {
                           dislike++;
                        }
                      
                        data1.Likes = like;
                        data1.Dislikes = dislike;
                    }
                }

                List<EmployeeSuggestionLike> likeList = _db.EmployeeSuggestionLike.Where(a => a.ReviewerId == userId && a.IsDeleted == false).ToList();
                foreach (EmployeeSuggestionLike item in likeList)
                {
                    APIEmployeeSuggestionsGet data1 = employeeSuggestions.Where(a => a.Id == item.SuggestionId).FirstOrDefault();
                    if (item.Status == true)
                    {
                        data1.Status = "Liked";
                    }
                    else
                    {
                        data1.Status = "Disliked";
                    }
                    data1.Remarks = item.Remarks;
                }

                APIEmployeeSuggestionsListandCount data = new APIEmployeeSuggestionsListandCount();

                data.EmployeeSuggestionListandCount = employeeSuggestions.ToList();
                if (searchBy != null)
                {
                    if (searchBy.ToLower() == "project")
                    {
                        data.EmployeeSuggestionListandCount = employeeSuggestions.ToList();
                        employeeSuggestions = employeeSuggestions.Where(a => a.AreaName.ToLower().Contains(search)).ToList();
                    }
                    else if (searchBy.ToLower() == "cluster")
                    {
                        employeeSuggestions = employeeSuggestions.Where(a => a.Business.ToLower().Contains(search)).ToList();
                    }
                }
                data.Count = employeeSuggestions.Count;
                employeeSuggestions = employeeSuggestions.OrderByDescending(a => a.CreatedDate).ToList();
                data.EmployeeSuggestionListandCount = employeeSuggestions.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToList();

                return data;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<List<LikesList>> GetLikeList(int suggestionId, string like)
        {
            try
            {
                bool likeStatus = true;
                if (like == "Like")
                {
                    likeStatus = true;
                }
                else if (like == "Dislike")
                {
                    likeStatus = false;
                }

                List<LikesList> likeList = (from x in _db.EmployeeSuggestionLike
                                            join user in _db.UserMaster on x.ReviewerId equals user.Id
                                            where x.SuggestionId == suggestionId && x.IsDeleted == false && x.Status == likeStatus
                                            select new LikesList
                                            {
                                                Id = x.ReviewerId,
                                                UserName = user.UserName,
                                                remark = x.Remarks
                                            }).AsNoTracking().ToList();

                return likeList;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<APIEmployeeSuggestionAndDigitalAdoptionReview> GetEmployeeSuggestionsTop5(APIFilter filterData)
        {
            try
            {
                if (filterData.Top5.ToLower() == "suggestions" || filterData.Top5.ToLower() == null)
                {
                    var query = (from es in _db.EmployeeSuggestions
                                 join scat in _db.SuggestionCategory on es.Category equals scat.Id
                                 join um in _db.UserMaster on es.EmployeeId equals um.Id
                                 join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                                 join area in _db.Area on umd.AreaId equals area.Id
                                 into top5area
                                 from area in top5area.DefaultIfEmpty()
                                 join busi in _db.Business on umd.BusinessId equals busi.Id
                                 into top5business
                                 from business in top5business.DefaultIfEmpty()
                                 where es.IsDeleted == false
                                 select new APIEmployeeSuggestionsGet
                                 {
                                     AdditionalDescription = es.AdditionalDescription,
                                     Category = es.Category,
                                     CreatedDate = es.CreatedDate,
                                     Id = es.Id,
                                     IsActive = es.IsActive,
                                     Suggestion = es.Suggestion,
                                     SuggestionsCategory = scat.SuggestionsCategory,
                                     EmployeeName = um.UserName,
                                     AreaName = area.Name,
                                     AreaId = umd.AreaId,
                                     BusinessId = umd.BusinessId,
                                     Business = business.Name,
                                     Month = es.CreatedDate.Month.ToString(),
                                     Year = es.CreatedDate.Year.ToString()
                                 }).AsNoTracking();

                    List<APIEmployeeSuggestionsGet> EmployeeSuggestionsGetsTop5 = query.ToList();

                    List<FilesAndLikesCount> filesCount = (from esf in _db.EmployeeSuggestionFile
                                                           join um in _db.UserMaster on esf.CreatedBy equals um.Id
                                                           where esf.IsDeleted == false
                                                           group esf by esf.SuggestionId into suggFileGroup
                                                           select new FilesAndLikesCount
                                                           {
                                                               Count = suggFileGroup.Count(),
                                                               SuggestionId = suggFileGroup.Key
                                                           }).ToList();

                    foreach (FilesAndLikesCount item in filesCount)
                    {
                        APIEmployeeSuggestionsGet data1 = EmployeeSuggestionsGetsTop5.Where(a => a.Id == item.SuggestionId).FirstOrDefault();
                        data1.Files = item.Count;
                    }

                    List<FilesAndLikesCount> likesCount = (from esl in _db.EmployeeSuggestionLike
                                                           join um in _db.UserMaster on esl.ReviewerId equals um.Id
                                                           where esl.IsDeleted == false
                                                           group esl by esl.SuggestionId into suggGroup
                                                           select new FilesAndLikesCount
                                                           {
                                                               Count = suggGroup.Count(),
                                                               SuggestionId = suggGroup.Key
                                                           }).ToList();

                    foreach (FilesAndLikesCount item in likesCount)
                    {
                        APIEmployeeSuggestionsGet data1 = EmployeeSuggestionsGetsTop5.Where(a => a.Id == item.SuggestionId).FirstOrDefault();
                        int like = 0, dislike = 0;
                        List<EmployeeSuggestionLike> likedata = _db.EmployeeSuggestionLike.Where(a => a.SuggestionId == item.SuggestionId).ToList();
                        foreach (EmployeeSuggestionLike item1 in likedata)
                        {
                            if (item1.Status == true)
                            {
                                like++;
                            }
                            else
                            {
                                dislike++;
                            }
                        }
                        data1.Likes = like;
                        data1.Dislikes = dislike;
                    }

                    APIEmployeeSuggestionAndDigitalAdoptionReview data = new APIEmployeeSuggestionAndDigitalAdoptionReview();

                    if (filterData.Project != null)
                    {
                        EmployeeSuggestionsGetsTop5 = EmployeeSuggestionsGetsTop5.Where(a => a.AreaId == Convert.ToInt32(filterData.Project)).ToList();
                    }

                    if (filterData.Cluster != null)
                    {
                        EmployeeSuggestionsGetsTop5 = EmployeeSuggestionsGetsTop5.Where(a => a.BusinessId == Convert.ToInt32(filterData.Cluster)).ToList();
                    }

                    if (filterData.Category != null)
                    {
                        int catId = Convert.ToInt32(filterData.Category);
                        EmployeeSuggestionsGetsTop5 = EmployeeSuggestionsGetsTop5.Where(a => a.Category == catId).ToList();
                    }
                    if (filterData.Month != null)
                    {

                        EmployeeSuggestionsGetsTop5 = EmployeeSuggestionsGetsTop5.Where(a => Convert.ToInt32(a.Month) == filterData.Month).ToList();
                    }
                    if (filterData.Year != null)
                    {
                        EmployeeSuggestionsGetsTop5 = EmployeeSuggestionsGetsTop5.Where(a => Convert.ToInt32(a.Year) == filterData.Year).ToList();
                    }

                    data.EmployeeSuggestionListandCount = EmployeeSuggestionsGetsTop5.OrderByDescending(a => a.Likes).ThenBy(a => a.Dislikes).Take(5).ToList();
                    data.employeeGroupDigitalAdoptionReviews = null;
                    return data;
                }
                else
                {
                    var QueryGroupBy = _db.DigitalAdoptionReview.GroupBy(a => new { a.EmployeeId, a.DescriptionId, a.RoleId }).Select(c => new EmployeeGroupDigitalAdoptionReview { EmployeeId = c.Key.EmployeeId, UseCase = c.Key.DescriptionId, Role = c.Key.RoleId, Total = c.Count() });
                    List<EmployeeGroupDigitalAdoptionReview> DigitalAdoptionGroupBy = QueryGroupBy.ToList();

                    foreach (EmployeeGroupDigitalAdoptionReview item in DigitalAdoptionGroupBy)
                    {
                        List<DigitalAdoptionReview> data1 = _db.DigitalAdoptionReview.Where(a => a.EmployeeId == item.EmployeeId && a.DescriptionId == item.UseCase && a.RoleId == item.Role).Select(a => new DigitalAdoptionReview { InvolvementLevel = a.InvolvementLevel, UseCaseKnowledge = a.UseCaseKnowledge, DigitalAwareness = a.DigitalAwareness }).ToList();
                        int involvementLevel = 0, digitalAwareness = 0, useCaseKnowledge = 0;

                        foreach (DigitalAdoptionReview item1 in data1)
                        {
                            involvementLevel = involvementLevel + item1.InvolvementLevel;
                            digitalAwareness = digitalAwareness + item1.DigitalAwareness;
                            useCaseKnowledge = useCaseKnowledge + item1.UseCaseKnowledge;
                        }

                        int totalScore = involvementLevel + digitalAwareness + useCaseKnowledge;
                        int average = totalScore / (item.Total * 3);

                        item.average = average;
                        data1.Clear();

                    }
                    if (filterData.UseCase != null)
                    {
                        int useCaseId = Convert.ToInt32(filterData.UseCase);
                        DigitalAdoptionGroupBy = DigitalAdoptionGroupBy.Where(a => a.UseCase == useCaseId).ToList();
                    }

                    foreach (EmployeeGroupDigitalAdoptionReview item in DigitalAdoptionGroupBy)
                    {
                        UserMaster user = _db.UserMaster.Where(a => a.Id == item.EmployeeId).FirstOrDefault();
                        item.EmployeeName = user.UserName;
                        item.UserId = Security.Decrypt(user.UserId);
                        UseCase useCase = _db.UseCase.Where(a => a.Id == item.UseCase).FirstOrDefault();
                        item.UseCaseName = useCase.Description;
                        // DigitalAdoptionReview roleid = _db.DigitalAdoptionReview.Where(a => a.EmployeeId == item.EmployeeId).FirstOrDefault();
                        //item.Role = roleid.Id;
                        DigitalRole role1 = _db.DigitalRole.Where(a => a.Id == item.Role).FirstOrDefault();
                        item.RoleName = role1.Description;

                    }
                    if (filterData.RoleId != null)
                    {
                        int roleId = Convert.ToInt32(filterData.RoleId);
                        DigitalAdoptionGroupBy = DigitalAdoptionGroupBy.Where(a => a.Role == roleId).ToList();
                    }
                    DigitalAdoptionGroupBy = DigitalAdoptionGroupBy.OrderByDescending(a => a.average).Take(5).ToList();

                    APIEmployeeSuggestionAndDigitalAdoptionReview data = new APIEmployeeSuggestionAndDigitalAdoptionReview();
                    data.employeeGroupDigitalAdoptionReviews = DigitalAdoptionGroupBy;
                    data.EmployeeSuggestionListandCount = null;

                    return data;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<APIEmployeeSuggestionsListandCount> GetEmployeeSuggestionsForUser(int page, int pageSize, int UserId)
        {
            try
            {
                var query = (from es in _db.EmployeeSuggestions
                             join um in _db.UserMaster on es.EmployeeId equals um.Id
                             join scat in _db.SuggestionCategory on es.Category equals scat.Id
                             where es.IsDeleted == false
                             select new APIEmployeeSuggestionsGet
                             {
                                 AdditionalDescription = es.AdditionalDescription,
                                 Category = es.Category,
                                 CreatedDate = es.CreatedDate,
                                 Id = es.Id,
                                 IsActive = es.IsActive,
                                 Suggestion = es.Suggestion,
                                 SuggestionsCategory = scat.SuggestionsCategory,
                                 EmployeeName = um.UserName,
                                 EmployeeId = es.EmployeeId
                             }).AsNoTracking();

                List<APIEmployeeSuggestionsGet> suggestionData = query.ToList();

              //  var fileCountQuery = _db.EmployeeSuggestionFile.Where(a => a.IsDeleted == false).GroupBy(a => a.SuggestionId).Select(c => new FilesAndLikesCount { Count = c.Count(), SuggestionId = c.Key });
                 List<FilesAndLikesCount> filesCount = (from esf in _db.EmployeeSuggestionFile
                                                       join um in _db.UserMaster on esf.CreatedBy equals um.Id
                                                       where esf.IsDeleted == false
                                                       group esf by esf.SuggestionId into suggFileGroup
                                                       select new FilesAndLikesCount
                                                       {
                                                           Count = suggFileGroup.Count(),
                                                           SuggestionId = suggFileGroup.Key
                                                       }).ToList();
                
                foreach (FilesAndLikesCount item in filesCount)
                {
                    APIEmployeeSuggestionsGet data1 = suggestionData.Where(a => a.Id == item.SuggestionId).FirstOrDefault();
                    data1.Files = item.Count;
                }

                List<FilesAndLikesCount> likesCount = (from esl in _db.EmployeeSuggestionLike
                                                       join um in _db.UserMaster on esl.ReviewerId equals um.Id
                                                       where esl.IsDeleted == false
                                                       group esl by esl.SuggestionId into suggGroup
                                                       select new FilesAndLikesCount
                                                       {
                                                           Count = suggGroup.Count(),
                                                           SuggestionId = suggGroup.Key
                                                       }).ToList();

               // var fileLikesQuery = _db.EmployeeSuggestionLike.Where(a => a.IsDeleted == false).GroupBy(a => a.SuggestionId).Select(c => new FilesAndLikesCount { Count = c.Count(), SuggestionId = c.Key });
                foreach (FilesAndLikesCount item in likesCount)
                {
                    APIEmployeeSuggestionsGet data1 = suggestionData.Where(a => a.Id == item.SuggestionId).FirstOrDefault();
                    int like = 0, dislike = 0;
                    List<EmployeeSuggestionLike> likedata = _db.EmployeeSuggestionLike.Where(a => a.SuggestionId == item.SuggestionId).ToList();
                    foreach (EmployeeSuggestionLike item1 in likedata)
                    {
                        if (item1.Status == true)
                        {
                            like++;
                        }
                        else
                        {
                            dislike++;
                        }
                    }
                    data1.Likes = like;
                    data1.Dislikes = dislike;
                }

                APIEmployeeSuggestionsListandCount data = new APIEmployeeSuggestionsListandCount();
                data.EmployeeSuggestionListandCount = suggestionData.Where(a => a.EmployeeId == UserId).OrderByDescending(x => x.CreatedDate).ToList();
                data.Count = data.EmployeeSuggestionListandCount.Count;
                data.EmployeeSuggestionListandCount = data.EmployeeSuggestionListandCount.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToList();

                return data;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<EmployeeSuggestionLike> PostEmployeeSuggestionLike(int suggestionId, EmployeeSuggestionLike data, int UserId)
        {
            data.ModifiedBy = UserId;
            data.CreatedBy = UserId;
            data.IsActive = true;
            data.IsDeleted = false;
            data.CreatedDate = DateTime.UtcNow;
            data.ModifiedDate = DateTime.UtcNow;
            data.SuggestionId = suggestionId;
            data.ReviewerId = UserId;
            await this._db.EmployeeSuggestionLike.AddAsync(data);
            await this._db.SaveChangesAsync();
            return data;
        }

        public async Task<string> UpdateEmployeeSuggestionLike(int suggestionId, EmployeeSuggestionLike RemarkData, EmployeeSuggestionLike data, int UserId)
        {
            RemarkData.ModifiedDate = DateTime.Now;
            RemarkData.ModifiedBy = UserId;
            RemarkData.SuggestionId = suggestionId;
            RemarkData.IsActive = true;
            RemarkData.IsDeleted = false;
            RemarkData.Remarks = data.Remarks;
            RemarkData.Status = data.Status;
            RemarkData.ReviewerId = UserId;

            this._db.EmployeeSuggestionLike.Update(RemarkData);
            this._db.SaveChanges();
            return "Updated Succefully!!";
        }

        public void UpdateDeleteInFileTable(EmployeeSuggestionFile file)
        {
            this._db.EmployeeSuggestionFile.Update(file);
            this._db.SaveChanges();
            return;
        }

        public void UpdateDeleteInLikeTable(EmployeeSuggestionLike like)
        {
            this._db.EmployeeSuggestionLike.Update(like);
            this._db.SaveChanges();
            return;
        }
        public List<EmployeeSuggestionLike> GetLikeData(int suggestionId)
        {
            List<EmployeeSuggestionLike> likeData = _db.EmployeeSuggestionLike.Where(a => a.SuggestionId == suggestionId).ToList();
            return likeData;
        }

        public List<EmployeeSuggestionFile> GetFileData(int suggestionId)
        {
            List<EmployeeSuggestionFile> fileData = _db.EmployeeSuggestionFile.Where(a => a.SuggestionId == suggestionId).ToList();
            return fileData;
        }


        public async Task<GetAreaListandCount> GetArea()
        {
            var Query = (from x in _db.Area
                         where x.IsDeleted == 0
                         orderby x.Name descending,
                         x.Id
                         select new Area
                         {
                             Id = x.Id,
                             Name = x.Name,
                         }).AsNoTracking();

            GetAreaListandCount ListandCount = new GetAreaListandCount();
            ListandCount.Count = Query.Distinct().Count();
            ListandCount.GetAreasListandCount = await Query.Distinct().ToListAsync();
            return ListandCount;
        }
        public async Task<List<Business>> GetCluster()
        {
            var Query = (from x in _db.Business
                         where x.IsDeleted == 0
                         orderby x.Name descending,
                         x.Id
                         select new Business
                         {
                             Id = x.Id,
                             Name = x.Name,
                         }).AsNoTracking();
            List<Business> BusinessList = await Query.Distinct().ToListAsync();
            return BusinessList;
        }

        public void savefile(EmployeeSuggestionFile employeeSuggestionFile)
        {
            _db.EmployeeSuggestionFile.Add(employeeSuggestionFile);
            _db.SaveChanges();
        }

        public async Task<AttachedFilesListandCount> GetAttachedFiles(int Id)
        {
            var Query = (from x in _db.EmployeeSuggestionFile

                         where x.SuggestionId == Id
                         select new GetAttachedFiles
                         {
                             id = x.Id,
                             FileName = x.FileName,
                             FilePath = x.FilePath,
                             FileType = x.FileType
                         }).AsNoTracking();

            AttachedFilesListandCount ListandCount = new AttachedFilesListandCount();
            ListandCount.AttachedFileListandCount = await Query.Take(Id).Distinct().ToListAsync();
            ListandCount.Count = Query.Distinct().Count();
            return ListandCount;
        }

        public async Task<List<getfilecount>> GetFileCount()
        {
            return await this._db.EmployeeSuggestionFile.GroupBy(x => x.SuggestionId).Select(x => new getfilecount { SuggestionId = x.Key, cnt = x.Count() }).ToListAsync();
        }

        public async Task<EmployeeSuggestionLike> GetRemarkData(int suggestionId, int userId)
        {
            EmployeeSuggestionLike employeeSuggestionLikedata = _db.EmployeeSuggestionLike.Where(x => x.SuggestionId == suggestionId && x.CreatedBy == userId).FirstOrDefault();
            return employeeSuggestionLikedata;
        }
    }
}
