using System;
using System.Collections.Generic;

namespace Courses.API.APIModel
{
    public class APICourseGroupMappings
    {
        public int? Id { get; set; }
     
        public int? GroupId { get; set; }     
     
        public string GroupName { get; set; }
      
        public int CourseId { get; set; }
        public int? GroupCount { get; set; }
     
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string GroupCode { get; set; }
        public bool? Status { get; set; }

    }

    public class APICourseGroupMappingsData
    {
        public int? Id { get; set; }
        public int[] GroupId { get; set; }
        public int CourseId { get; set; }
    
    }
    public class APICourseGroupMappingsDelete
    {
        public int Id { get; set; }
      
    }

    public class APICourseGroupMappingMerge
    {
        public int? Id { get; set; }
        public int? CourseCategoryId { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public string CourseCategory { get; set; }
        public string Course { get; set; }
        public string Module { get; set; }
        public string CompetencyCategory { get; set; }
        public string TrainingType { get; set; }

        public int CompetencyId { get; set; }
        public int CompetencyCategoryId { get; set; }
    }
    public class CourseGroupMappingRecord
    {
        public int CompetencyId { get; set; }
        public int CompetencyCategoryId { get; set; }
    }

    public class APICourseGroup
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public int CourseCount { get; set; }
        public bool Status { get; set; }
    }
}
