using System.Collections.Generic;

namespace Competency.API.APIModel
{
    public class APICompetencyJobRole
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string RoleColumn1 { get; set; }
        public int RoleColumn1value { get; set; }
        public string RoleColumn2 { get; set; }
        public int RoleColumn2value { get; set; }
        public string Description { get; set; }
        public string Column1value { get; set; }
        public string Column2value { get; set; }
        public bool IsDeleted { get; set; }
        public int NumberOfPositions { get; set; }
        public APIJobRole[] CompetencySkillsData { get; set; }
        public int[] CompetencySkills { get; set; }
        public APIJobRole[] NextJobRolesData { get; set; }
        public int[] NextJobRoles { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
    }

    public class APIViewCompetencyJobRole
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class APIJobRole
    {
        public int? Id { get; set; }

        public string Name { get; set; }

    }

    public class APISubSubCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

    }

    public class APICourseAuthor
    {
        public string Id { get; set; }

        public string Name { get; set; }

    }
    public class APIJobRoleInUse
    {
        public bool JobRoleInUse { get; set; }

    }

    public class APIMasterTestCourse
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        public bool IsMasterCourseEnable { get; set; }
        public string JobRoleName { get; set; }
        public bool IsApprovedByManager { get; set; }
        public string AssessmentStatus { get; set; }
        public int Attempts { get; set; }
    }
    public class APIManagerEvaluationData
    {
        public List<APIMyTeamCompetencyMasterCourse> data { get; set; }
        public int TotalRecords { get; set; }
    }

        public class APIMyTeamCompetencyMasterCourse
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        public bool IsMasterCourseEnable { get; set; }
        public string JobRoleName { get; set; }
        public string AssessmentResult { get; set; }
        public string AssessmentPercentage { get; set; }
        public bool IsApprovedByManager { get; set; }
        public string AssessmentStatus { get; set; }
        public int Attempts { get; set; }
        public bool MasterTest { get; set; }
        public bool EvaluationToManager { get; set; }
        public int ModuleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
