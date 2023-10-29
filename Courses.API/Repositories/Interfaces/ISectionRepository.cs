using Courses.API.APIModel;
using Courses.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ISectionRepository : IRepository<Section>
    {
        Task<List<ApiSection>> GetByCourseCode(string courseCode);
        bool Exist(Section section);
        Task<bool> IsDependacyExist(int Id);
        Task<bool> ExistForUpdate(Section section);
    }

    public interface ILessonRepository : IRepository<Lesson>
    {

    }

}
