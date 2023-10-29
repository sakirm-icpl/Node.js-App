using System.Collections.Generic;

namespace Competency.API.APIModel.Competency
{
    public class APICompetenciesMapping
    {
        public int? Id { get; set; }
        public int? CompetencyCategoryId { get; set; }
        public int? CompetencySubCategoryId { get; set; }
        public int? CompetencySubSubCategoryId { get; set; }
        public int CompetencyId { get; set; }
        public string CompetencyCategory { get; set; }
        public string Competency { get; set; }
        public string CompetencyName { get; set; }
        public string TrainingType { get; set; }
        public int? CompetencyLevelId { get; set; }
        public string CompetencyLevel { get; set; }
        public int? CourseCategoryId { get; set; }
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public string CourseCategory { get; set; }
        public string JobRole { get; set; }

        public string Course { get; set; }
        public string Module { get; set; }
        public string HighestLevel { get; set; }
    }

    public class APICompetenciesMappingMerge 
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
    public class CompetenciesMappingRecord
    {
        public int CompetencyId { get; set; }
        public int CompetencyCategoryId { get; set; }
    }

    public class APICategorywiseCompetenciesMapping
    {
        public int? CategoryId { get; set; }
        public string CompetencyCategoryCode { get; set; }
        public string CompetencyCategory { get; set; }
        public List<APICompetenciesMapping> CompetenciesMap { get; set; }
    }
    public class APICategorywiseCompetenciesDetails
    {
        public string CourseTitle { get; set; }
        public string ModuleName { get; set; }
        public string CompetencyLevel { get; set; }
        public double? AssessmentScore { get; set; }
        public decimal? Percentage { get; set; }
    }

    #region bulk upload competency mapping

    public class APICompetencyMappingImport
    {
        public string Course { get; set; }
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

    public class APICompetencyMappingImportColumns
    {
        public const string Course = "Course";
        public const string Category = "Category";
        public const string Competency = "Competency";
        public const string CompetencyLevel = "CompetencyLevel";
    }

    #endregion

}
