// ======================================
// <copyright file="ICompetencyLevelsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Courses.API.APIModel;
using Courses.API.Model;
using Courses.API.Model.EdCastAPI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ICourseGroupMappingRepository : IRepository<CourseGroupMapping>
    {
        Task<IEnumerable<APICourseGroupMappings>> GetAllCourseGroupMapping(int page, int pageSize, string search = null);     
        Task<int> Count(string search = null);
        Task<IEnumerable<APICourseGroupMappings>> GetAllGroupsMappingByCourse(int courseId, int page, int pageSize);
        Task<int> GetCountGroupByCourseId(int courseid);
        Task<CourseGroupMapping> Exists(int courseId,  int compId);
        Task<int[]> getGroupIdByCourseId(int CourseId);
        void FindElementsNotInArray(int[] CurrentCompetencies, int[] aPIOldCompetenciesId, int CourseId);
        Task<List<APICourseGroup>> GetCourseGroupTypeAhead(string search = null);
        Task<int> GroupMappingDelete(int id);

        //Task<IEnumerable<APICompetencyWiseCourses>> GetCompetencyWiseCourses(int? comId);
        //Task<IEnumerable<CompetenciesMapping>> GetCompetency(int courseId);
        //Task<int> CountCourse(int courseid);
        //Task<CompetenciesMapping> GetRecordCourse(int courseid);
        //Task<IEnumerable<APICompetenciesMapping>> GetAllCategoryWiseCompetencies(int page, int pageSize, int? jobroleid, string search = null);
        //Task<APICategorywiseCompetenciesDetails> GetCompetanciesDetail(int competencyMappingId, int courseId, int? moduleId, int userId);
        //Task<int> GetAllCategoryWiseCompetenciesCount(string search = null,int? jobroleid = null);

        int SaveCourseGroup(CourseGroup courseGroup, int UserId);
        IEnumerable<APICourseGroup> GetAllCourseGroup(int page, int pageSize, string search = null);
        int GetAllCourseGroupCount(string search = null);
        int DeleteCourseGroup(string CourseGroupCode);
        int UpdateCourseGroup(CourseGroup courseGroup, int UserId);
        void UpdateCourseGroupCourseCount(int groupid);
    }
}
