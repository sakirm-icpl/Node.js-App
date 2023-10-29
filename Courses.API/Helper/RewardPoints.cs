using Courses.API.Models;
using Courses.API.Repositories.Interfaces;

namespace Courses.API.Helper
{
    public  class RewardPoints
    {
        private CourseContext _db;
        IRewardsPointRepository rewardsPointRepository;

        CacheManager.CacheManager cache = new CacheManager.CacheManager();
        public static string Replace(string Title,string Description)
        {
            string TitleDescription;
                TitleDescription = Description.Replace("[Title]", Title);
                return TitleDescription;
           
        }
    }
}
