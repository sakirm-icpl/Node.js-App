namespace Courses.API.APIModel.Competency
{
    public class APIRolewiseCompetenciesMapping 
    {
        public int? Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int CompetencyCategoryId { get; set; }
        public int CompetencyId { get; set; }
        public string CompetencyCategory { get; set; }
        public string Competency { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class APIRoleCompetenciesMappingMerge 
    {
        public int? Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public RoleCompetenciesMappingRecord[] rolecompetenciesMappingRecord { get; set; }
    }
    public class RoleCompetenciesMappingRecord
    {
        public int CompetencyId { get; set; }
        public int CompetencyCategoryId { get; set; }
    }
}
