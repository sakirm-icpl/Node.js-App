using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Repositories
{
    public class SectionRepository : Repository<Section>, ISectionRepository
    {
        private CourseContext _db;
        public SectionRepository(CourseContext context) : base(context)
        {
            _db = context;
        }
        public async Task<List<ApiSection>> GetByCourseCode(string courseCode)
        {
            var SectionsList = await _db.Section.Where(s => s.CourseCode.Equals(courseCode)).ToListAsync(); ;

            List<ApiSection> ApiSectionlist = new List<ApiSection>();
            ApiSectionlist = Mapper.Map<List<ApiSection>>(SectionsList);
            return ApiSectionlist.ToList();

        }
        public bool Exist(Section section)
        {
            int Count = _db.Section.Where(s => (s.Title.Equals(section.Title) || s.SectionNumber.Equals(section.SectionNumber)) && s.CourseCode.Equals(section.CourseCode)).Count();
            if (Count > 0)
                return true;
            return false;
        }
        public async Task<bool> ExistForUpdate(Section section)
        {
            int Count = await _db.Section.Where(s =>
            s.Id != section.Id
            && s.CourseCode == section.CourseCode
            &&
            (
                s.CourseCode.StartsWith(section.Title.ToLower())
                || s.SectionNumber == section.SectionNumber)
            ).CountAsync();
            if (Count > 0)
                return true;
            return false;
        }

        public async Task<bool> IsDependacyExist(int Id)
        {
            int Count = await (from association in _db.CourseModuleAssociation

                               where (association.SectionId == Id)
                               select new { association.Id }).CountAsync();
            if (Count > 0)
                return true;
            return false;
        }
    }


    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        private CourseContext _db;
        public LessonRepository(CourseContext context) : base(context)
        {
            _db = context;
        }

    }
}
