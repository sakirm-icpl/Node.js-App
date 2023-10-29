using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Competency.API.APIModel
{
    public class APICareerRoles
    {
        public int JobRoleId { get; set; }
        public string JobRoleName { get; set; }
        public APINextJobRoles[] NextJobRoles { get; set; }
        public int Id { get; set; }
        public int SkillCount { get; set; }        
    }


    public class APICompetencySkillSet
    {       
        public APICompetencySkillName[] CompetencySkill { get; set; }
    }

    public class APICompetencySkillName
    {
        public string CompetencySkill { get; set; }
        public int CompetencySkillId { get; set; }
        public string PerformanceRating { get; set; }
    }
    public class APICompetencySkillNameV4
    {
        public APICompetencySkillNameV3[] CompetencySkill { get; set; }
    }
    public class APICompetencySkillNameV3
    {
        public int Id { get; set; }
        public string CompetencySkill { get; set; }
        public int CompetencySkillId { get; set; }
        public bool IsSelfAssessmentGiven { get; set; }
        public int Level { get; set; }
        public string LevelName { get; set; }
    }
    public class APICompetencySkillNameV2
    {
        public string CompetencySkill { get; set; }
        public int CompetencySkillId { get; set; }
    }
    public class AssessmentStatus
    {
        public string UserId { get; set; }
        public int? JobroleId { get; set; }
    }
    public class APINextJobRoles
    {
        public int JobRoleId { get; set; }
        public string JobRoleName { get; set; }
        public int NumberOfPositions { get; set; }
    }
}
