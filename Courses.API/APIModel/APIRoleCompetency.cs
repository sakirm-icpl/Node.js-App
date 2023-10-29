using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APIRoleCompetency
    {
        public int Id { get; set; }
        public int JobRoleId { get; set; }
        public int? CompetencyCategoryId { get; set; }
        public int CompetencyId { get; set; }
        public int? CompetencyLevelId { get; set; }
        public bool IsActive { get; set; }
        public string JobRoleName { get; set; }
        public bool IsDeleted { get; set; }
        public string CompetencyCategoryName { get; set; }
        public string CompetencyName { get; set; }
        public string CompetencyLevelName { get; set; }
    }

    public class APICompetencySkill
    {
        public int Id { get; set; }       
        public string CompetencyName { get; set; }
       
    }


    public class APIRoleCompetenciesImport
    {
        public string Role { get; set; }
        public string Category { get; set; }
        public string Competency { get; set; }
        public string CompetencyLevel { get; set; }


        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
    }


    public class APIRoleCompetenciesImportColumns
    {
        public const string Role = "Role";
        public const string Category = "Category";
        public const string Competency = "Competency";
        public const string CompetencyLevel = "CompetencyLevel";
    }
}
