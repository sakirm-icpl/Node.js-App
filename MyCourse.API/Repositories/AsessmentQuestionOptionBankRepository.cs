using MyCourse.API.Model;
using MyCourse.API.Repositories.Interface;
//using MyCourse.API.Models;
using MyCourse.API.Repositories;

namespace MyCourse.API.Repositories
{
    public class AsessmentQuestionOptionBankRepository : Repository<AssessmentQuestionOption>, IAsessmentQuestionOption
    {
        private CourseContext _db;

        public AsessmentQuestionOptionBankRepository(CourseContext context) : base(context)
        {
            this._db = context;
        }
    }
}
