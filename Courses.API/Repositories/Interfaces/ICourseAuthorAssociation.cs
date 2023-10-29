using Courses.API.APIModel;
using Courses.API.Model;
using Courses.API.Model.EdCastAPI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ICourseAuthorAssociation : IRepository<CourseAuthorAssociation>
    {
       
        Task<CourseAuthorAssociation> RecordExists(int courseId, int userId);
        Task<int[]> getAuthorsCourseId(int CourseId);
        Task<List<APICourseAuthor>> GetAuthorsByCourseId(int CourseId);
        void FindElementsNotInArray(int[] CurrentAuthors, int[] oldAuthors, int CourseId);
    }
}
