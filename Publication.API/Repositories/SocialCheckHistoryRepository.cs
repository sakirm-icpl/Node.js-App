using AutoMapper;
using Publication.API.APIModel;
using Publication.API.Data;
using Publication.API.Models;
using Publication.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Publication.API.Repositories
{
    public class SocialCheckHistoryRepository : Repository<SocialCheckHistory>, ISocialCheckHistoryRepository
    {
        private GadgetDbContext context;
        private IArticleCategoryRepository articleCategoryRepository;
        private IInterestingArticlesRepository interestingArticlesRepository;

        public SocialCheckHistoryRepository(GadgetDbContext context,
            IArticleCategoryRepository articleCategoryController,
            IInterestingArticlesRepository interestingArticlesController) : base(context)
        {
            this.context = context;
            this.articleCategoryRepository = articleCategoryController;
            this.interestingArticlesRepository = interestingArticlesController;
        }

        public async Task<bool> NewPostToShow(int UserId)
        {
            Feed result1 = (from feed in this.context.Feeds 
                            select feed).OrderByDescending(p => p.Id).FirstOrDefault();

            if( result1 != null)
            {
                DateTime latestPostTime = result1.ModifiedDate;


                SocialCheckHistory result2 = (from feed in this.context.socialCheckHistory
                                              where feed.UserId == UserId
                                              select feed).FirstOrDefault();

                if (result2 != null && latestPostTime != null)
                {
                    if (latestPostTime > result2.LastWallfeedCheckTime || result2.LastWallfeedCheckTime == null)
                    {
                        return true;
                    }
                }
                else if (result2 == null && latestPostTime != null)
                {
                    return true;
                }
            }
            
            return false;
        }

        public async Task<bool> NewNewsToShow(int UserId)

        {
            List<APINewsUpdates> result1 = Mapper.Map<List<APINewsUpdates>>(await this.GetAllApplicableNews(UserId));

            foreach(APINewsUpdates news in result1)
            {
                DateTime latestNewTime = news.PublishDate;

                SocialCheckHistory lastCheck = (from feed in this.context.socialCheckHistory
                                              where feed.UserId == UserId
                                              select feed).FirstOrDefault();

                if (lastCheck != null && latestNewTime != null)
                {
                    if (latestNewTime > lastCheck.LastNewsCheckTime || lastCheck.LastNewsCheckTime == null)
                    {
                        return true;
                    }
                }
                else if (lastCheck == null && latestNewTime != null)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<NewsUpdates>> GetAllApplicableNews(int userId)
        {
            IQueryable<Models.NewsUpdates> Query = this.context.NewsUpdates.Where(v => v.IsDeleted == false);
            DateTime TodaysDate = DateTime.UtcNow;
            Query = Query.Where(n => n.PublishDate.Date <= TodaysDate.Date && n.ValidityDate.Date >= TodaysDate.Date);
            Query = Query.OrderByDescending(v => v.Id);
            return await Query.ToListAsync();
        }

        public async Task<bool> NewArticleToShow(int UserId)
        {
            IEnumerable<APIInterestingArticleCategory> result = await this.articleCategoryRepository.GetArticles(UserId);

            foreach(APIInterestingArticleCategory articleCategory in result)
            {
                IEnumerable<InterestingArticles> interestingArticles = await this.interestingArticlesRepository.GetAllInterestingArticlesByCategoryId(articleCategory.Id, UserId);

                foreach (InterestingArticles article in interestingArticles)
                {
                    DateTime latestArticleTime = article.Date;

                    SocialCheckHistory lastCheck = (from feed in this.context.socialCheckHistory
                                                    where feed.UserId == UserId
                                                    select feed).FirstOrDefault();

                    if (lastCheck != null && latestArticleTime != null)
                    {
                        if (latestArticleTime > lastCheck.LastArticleCheckTime || lastCheck.LastArticleCheckTime == null)
                        {
                            return true;
                        }
                    }
                    else if (lastCheck == null && latestArticleTime != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<SocialCheckHistory> CheckForDuplicate(int UserId)
        {
            SocialCheckHistory obj = new SocialCheckHistory();
            obj = this.context.socialCheckHistory.Where(a => a.UserId == UserId).FirstOrDefault();
            return obj;
        }
    }
}
